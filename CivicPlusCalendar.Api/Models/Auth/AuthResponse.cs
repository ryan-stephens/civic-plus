using System.Text.Json.Serialization;

namespace CivicPlusCalendar.Api.Models;

/// <summary>
/// Response from the CivicPlus authentication endpoint
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT access token for API requests
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Type of token (typically "Bearer")
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in seconds
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
