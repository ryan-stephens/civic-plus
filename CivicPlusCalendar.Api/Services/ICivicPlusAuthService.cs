namespace CivicPlusCalendar.Api.Services;

/// <summary>
/// Service responsible for authentication with the CivicPlus API
/// </summary>
public interface ICivicPlusAuthService
{
    /// <summary>
    /// Ensures a valid authentication token is available and returns it
    /// </summary>
    /// <returns>A task representing the asynchronous operation that returns the access token</returns>
    Task<string> GetAccessTokenAsync();
}
