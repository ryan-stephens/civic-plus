using System.ComponentModel.DataAnnotations;

namespace CivicPlusCalendar.Api.Models;

/// <summary>
/// Request model for creating a new calendar event
/// </summary>
public class CreateEventRequest
{
    /// <summary>
    /// Event title (required)
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the event (required)
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Event start date and time (required)
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Event end date and time (required)
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }
}
