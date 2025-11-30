namespace CivicPlusCalendar.Api.Models;

/// <summary>
/// Represents a calendar event
/// </summary>
public class CalendarEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// Event title
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the event
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Event start date and time
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Event end date and time
    /// </summary>
    public DateTime EndDate { get; set; }
}
