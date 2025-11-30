using System.Text.Json.Serialization;

namespace CivicPlusCalendar.Api.Models;

/// <summary>
/// Response containing a collection of calendar events from the CivicPlus API
/// </summary>
public class CalendarEventResponse
{
    /// <summary>
    /// List of calendar events in this response
    /// </summary>
    [JsonPropertyName("items")]
    public List<CalendarEvent> Items { get; set; } = new();
}
