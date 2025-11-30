using CivicPlusCalendar.Api.Models;

namespace CivicPlusCalendar.Api.Services;

public interface ICivicPlusApiService
{
    Task<CalendarEventResponse> GetEventsAsync(int top = 20, int skip = 0, string? filter = null, string? orderBy = null);
    Task<CalendarEvent?> GetEventByIdAsync(string id);
    Task<CalendarEvent> CreateEventAsync(CreateEventRequest request);
}
