using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CivicPlusCalendar.Api.Configuration;
using CivicPlusCalendar.Api.Models;
using Microsoft.Extensions.Options;

namespace CivicPlusCalendar.Api.Services;

/// <summary>
/// Service for handling authentication with the CivicPlus API
/// Manages token caching, expiration, and refresh logic
/// </summary>
public class CivicPlusAuthService : ICivicPlusAuthService
{
    private readonly HttpClient _httpClient;
    private readonly CivicPlusApiSettings _settings;
    private readonly ILogger<CivicPlusAuthService> _logger;

    // Token caching with thread safety
    private string? _cachedAccessToken;
    private DateTime _tokenExpiration = DateTime.MinValue;
    private readonly SemaphoreSlim _authLock = new(1, 1);

    // Configuration constants
    private const int TokenExpirationBufferMinutes = 5;
    private const string BearerScheme = "Bearer";
    private const string JsonMediaType = "application/json";

    public CivicPlusAuthService(
        HttpClient httpClient,
        IOptions<CivicPlusApiSettings> settings,
        ILogger<CivicPlusAuthService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Gets a valid authentication token.
    /// Uses token caching with expiration buffer and thread-safe locking to prevent duplicate auth requests.
    /// </summary>
    public async Task<string> GetAccessTokenAsync()
    {
        // Fast path: check if cached token is still valid (without locking)
        if (IsTokenValid())
        {
            _logger.LogDebug("Using cached access token");
            return _cachedAccessToken!;
        }

        // Slow path: acquire lock and authenticate
        await _authLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock (another thread may have already authenticated)
            if (IsTokenValid())
            {
                _logger.LogDebug("Using cached access token (acquired after lock)");
                return _cachedAccessToken!;
            }

            await AuthenticateAsync();
            return _cachedAccessToken!;
        }
        finally
        {
            _authLock.Release();
        }
    }

    /// <summary>
    /// Checks if the cached token is still valid with a buffer for expiration
    /// </summary>
    private bool IsTokenValid()
    {
        if (string.IsNullOrEmpty(_cachedAccessToken))
        {
            return false;
        }

        // Token is valid if current time is before expiration minus buffer
        var expirationThreshold = _tokenExpiration.AddMinutes(-TokenExpirationBufferMinutes);
        return DateTime.UtcNow < expirationThreshold;
    }

    /// <summary>
    /// Authenticates with the CivicPlus API and caches the token
    /// </summary>
    private async Task AuthenticateAsync()
    {
        try
        {
            _logger.LogInformation("Authenticating with CivicPlus API...");

            var authRequest = new
            {
                clientId = _settings.ClientId,
                clientSecret = _settings.ClientSecret
            };

            var jsonContent = JsonSerializer.Serialize(authRequest);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, JsonMediaType);

            _logger.LogDebug("Sending auth request to: {BaseUrl}Auth", _settings.BaseUrl);

            // Clear any existing authorization header before authentication
            _httpClient.DefaultRequestHeaders.Authorization = null;

            var response = await _httpClient.PostAsync("Auth", httpContent);

            _logger.LogInformation("Auth response status: {StatusCode}", response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();

            // Validate response before processing
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Authentication failed with status {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("Auth endpoint returned empty response");
            }

            var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (authResponse?.AccessToken == null)
            {
                _logger.LogError("Auth response missing access token. Response: {Content}", content);
                throw new InvalidOperationException("Failed to obtain access token from response");
            }

            _cachedAccessToken = authResponse.AccessToken.Trim();
            _tokenExpiration = DateTime.UtcNow.AddSeconds(authResponse.ExpiresIn);

            _logger.LogInformation("Successfully authenticated with CivicPlus API. Token expires at: {Expiration}", _tokenExpiration);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during authentication with CivicPlus API. BaseUrl: {BaseUrl}", _settings.BaseUrl);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error during authentication");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication with CivicPlus API");
            throw;
        }
    }
}
