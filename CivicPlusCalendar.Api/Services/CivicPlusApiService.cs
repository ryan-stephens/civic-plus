using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CivicPlusCalendar.Api.Models;

namespace CivicPlusCalendar.Api.Services;

/// <summary>
/// Service for interacting with the CivicPlus API
/// Handles event management operations (GET/POST)
/// Authentication is delegated to ICivicPlusAuthService
/// </summary>
public class CivicPlusApiService : ICivicPlusApiService
{
    private readonly HttpClient _httpClient;
    private readonly ICivicPlusAuthService _authService;
    private readonly ILogger<CivicPlusApiService> _logger;

    // Configuration constants
    private const string JsonMediaType = "application/json";

    // Cached JSON serializer options to reduce heap allocations
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CivicPlusApiService(
        HttpClient httpClient,
        ICivicPlusAuthService authService,
        ILogger<CivicPlusApiService> logger)
    {
        _httpClient = httpClient;
        _authService = authService;
        _logger = logger;
    }

    public async Task<CalendarEventResponse> GetEventsAsync(int top = 20, int skip = 0, string? filter = null, string? orderBy = null)
    {
        try
        {
            var token = await _authService.GetAccessTokenAsync();

            // Use StringBuilder for efficient query string building (stack-allocated)
            var queryBuilder = new StringBuilder();
            queryBuilder.Append($"$top={top}&$skip={skip}");

            if (!string.IsNullOrWhiteSpace(filter))
            {
                queryBuilder.Append($"&$filter={Uri.EscapeDataString(filter)}");
            }

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                queryBuilder.Append($"&$orderBy={Uri.EscapeDataString(orderBy)}");
            }

            var url = $"Events?{queryBuilder}";
            
            _logger.LogInformation("Fetching events from: {Url}", url);
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            using var response = await _httpClient.SendAsync(request);
            
            _logger.LogInformation("Events response status: {StatusCode}", response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Events response content length: {Length}", content.Length);

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Events endpoint returned empty response");
                return new CalendarEventResponse();
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch events. Status: {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var events = JsonSerializer.Deserialize<CalendarEventResponse>(content, JsonOptions);

            _logger.LogInformation("Successfully fetched {Count} events", events?.Items?.Count ?? 0);

            return events ?? new CalendarEventResponse();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching calendar events");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error fetching calendar events");
            throw;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout fetching calendar events");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching calendar events");
            throw;
        }
    }

    public async Task<CalendarEvent?> GetEventByIdAsync(string id)
    {
        try
        {
            var token = await _authService.GetAccessTokenAsync();

            using var request = new HttpRequestMessage(HttpMethod.Get, $"Events/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var eventItem = JsonSerializer.Deserialize<CalendarEvent>(content, JsonOptions);

            return eventItem;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching calendar event by ID: {EventId}", id);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error fetching calendar event by ID: {EventId}", id);
            throw;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout fetching calendar event by ID: {EventId}", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching calendar event by ID: {EventId}", id);
            throw;
        }
    }

    public async Task<CalendarEvent> CreateEventAsync(CreateEventRequest request)
    {
        try
        {
            var token = await _authService.GetAccessTokenAsync();

            var jsonContent = JsonSerializer.Serialize(request);
            using var httpContent = new StringContent(jsonContent, Encoding.UTF8, JsonMediaType);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "Events")
            {
                Content = httpContent
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var createdEvent = JsonSerializer.Deserialize<CalendarEvent>(content, JsonOptions);

            return createdEvent ?? throw new InvalidOperationException("Failed to deserialize created event");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error creating calendar event");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON serialization/deserialization error creating calendar event");
            throw;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout creating calendar event");
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation creating calendar event");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating calendar event");
            throw;
        }
    }

}
