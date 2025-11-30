using CivicPlusCalendar.Api.Models;
using CivicPlusCalendar.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CivicPlusCalendar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalendarEventsController : ControllerBase
{
    private readonly ICivicPlusApiService _civicPlusApiService;
    private readonly ILogger<CalendarEventsController> _logger;

    public CalendarEventsController(
        ICivicPlusApiService civicPlusApiService,
        ILogger<CalendarEventsController> logger)
    {
        _civicPlusApiService = civicPlusApiService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all calendar events with optional pagination and filtering
    /// </summary>
    /// <param name="top">Maximum number of events to return (default: 20)</param>
    /// <param name="skip">Number of events to skip for pagination (default: 0)</param>
    /// <param name="filter">OData filter expression</param>
    /// <param name="orderBy">OData orderby expression</param>
    /// <returns>A paginated list of calendar events</returns>
    /// <response code="200">Successfully retrieved events</response>
    /// <response code="500">Server error occurred while fetching events</response>
    [HttpGet]
    [ProducesResponseType(typeof(CalendarEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CalendarEventResponse>> GetEvents(
        [FromQuery] int top = 20, 
        [FromQuery] int skip = 0,
        [FromQuery] string? filter = null,
        [FromQuery] string? orderBy = null)
    {
        try
        {
            var events = await _civicPlusApiService.GetEventsAsync(top, skip, filter, orderBy);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving calendar events");
            return StatusCode(500, new { message = "Error retrieving calendar events", error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a specific calendar event by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the event</param>
    /// <returns>The calendar event with the specified ID</returns>
    /// <response code="200">Successfully retrieved the event</response>
    /// <response code="404">Event with the specified ID was not found</response>
    /// <response code="500">Server error occurred while fetching the event</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CalendarEvent), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CalendarEvent>> GetEventById(string id)
    {
        try
        {
            var eventItem = await _civicPlusApiService.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound(new { message = $"Event with ID {id} not found" });
            }
            return Ok(eventItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving calendar event by ID: {EventId}", id);
            return StatusCode(500, new { message = "Error retrieving calendar event", error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new calendar event
    /// </summary>
    /// <param name="request">The event details including title, description, start date, and end date</param>
    /// <returns>The newly created calendar event</returns>
    /// <response code="201">Event successfully created</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Server error occurred while creating the event</response>
    [HttpPost]
    [ProducesResponseType(typeof(CalendarEvent), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CalendarEvent>> CreateEvent([FromBody] CreateEventRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdEvent = await _civicPlusApiService.CreateEventAsync(request);
            return CreatedAtAction(nameof(GetEvents), new { id = createdEvent.Id }, createdEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating calendar event");
            return StatusCode(500, new { message = "Error creating calendar event", error = ex.Message });
        }
    }
}
