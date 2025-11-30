using CivicPlusCalendar.Api.Controllers;
using CivicPlusCalendar.Api.Models;
using CivicPlusCalendar.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CivicPlusCalendar.Api.Tests;

public class CalendarEventsControllerTests
{
    private readonly Mock<ICivicPlusApiService> _mockApiService;
    private readonly Mock<ILogger<CalendarEventsController>> _mockLogger;
    private readonly CalendarEventsController _controller;

    public CalendarEventsControllerTests()
    {
        _mockApiService = new Mock<ICivicPlusApiService>();
        _mockLogger = new Mock<ILogger<CalendarEventsController>>();
        _controller = new CalendarEventsController(_mockApiService.Object, _mockLogger.Object);
    }

    #region GetEvents Tests

    [Fact]
    public async Task GetEvents_WithDefaultParameters_ReturnsOkResultWithEvents()
    {
        // Arrange
        var expectedEvents = new CalendarEventResponse
        {
            Items = new List<CalendarEvent>
            {
                new() { Id = "1", Title = "Event 1", Description = "Desc 1", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(1) },
                new() { Id = "2", Title = "Event 2", Description = "Desc 2", StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(1).AddHours(1) }
            }
        };

        _mockApiService
            .Setup(s => s.GetEventsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedEvents);

        // Act
        var result = await _controller.GetEvents();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedEvents = okResult.Value.Should().BeOfType<CalendarEventResponse>().Subject;
        returnedEvents.Items.Should().HaveCount(2);
        returnedEvents.Items[0].Title.Should().Be("Event 1");
        returnedEvents.Items[1].Title.Should().Be("Event 2");

        _mockApiService.Verify(s => s.GetEventsAsync(20, 0, null, null), Times.Once);
    }

    [Fact]
    public async Task GetEvents_WithCustomPaginationParameters_PassesParametersToService()
    {
        // Arrange
        var expectedEvents = new CalendarEventResponse { Items = new List<CalendarEvent>() };
        _mockApiService
            .Setup(s => s.GetEventsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedEvents);

        // Act
        var result = await _controller.GetEvents(top: 50, skip: 10);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockApiService.Verify(s => s.GetEventsAsync(50, 10, null, null), Times.Once);
    }

    [Fact]
    public async Task GetEvents_WithFilterAndOrderBy_PassesParametersToService()
    {
        // Arrange
        var filter = "startswith(title, 'Meeting')";
        var orderBy = "startDate desc";
        var expectedEvents = new CalendarEventResponse { Items = new List<CalendarEvent>() };

        _mockApiService
            .Setup(s => s.GetEventsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedEvents);

        // Act
        var result = await _controller.GetEvents(filter: filter, orderBy: orderBy);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockApiService.Verify(s => s.GetEventsAsync(20, 0, filter, orderBy), Times.Once);
    }

    [Fact]
    public async Task GetEvents_WhenServiceThrowsException_Returns500InternalServerError()
    {
        // Arrange
        var exceptionMessage = "Database connection failed";
        _mockApiService
            .Setup(s => s.GetEventsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _controller.GetEvents();

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetEvents_WhenServiceThrowsException_LogsError()
    {
        // Arrange
        var exception = new Exception("Test error");
        _mockApiService
            .Setup(s => s.GetEventsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(exception);

        // Act
        await _controller.GetEvents();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region GetEventById Tests

    [Fact]
    public async Task GetEventById_WithValidId_ReturnsOkResultWithEvent()
    {
        // Arrange
        var eventId = "123";
        var expectedEvent = new CalendarEvent
        {
            Id = eventId,
            Title = "Team Meeting",
            Description = "Quarterly review",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddHours(1)
        };

        _mockApiService
            .Setup(s => s.GetEventByIdAsync(eventId))
            .ReturnsAsync(expectedEvent);

        // Act
        var result = await _controller.GetEventById(eventId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedEvent = okResult.Value.Should().BeOfType<CalendarEvent>().Subject;
        returnedEvent.Id.Should().Be(eventId);
        returnedEvent.Title.Should().Be("Team Meeting");

        _mockApiService.Verify(s => s.GetEventByIdAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task GetEventById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var eventId = "nonexistent";
        _mockApiService
            .Setup(s => s.GetEventByIdAsync(eventId))
            .ReturnsAsync((CalendarEvent?)null);

        // Act
        var result = await _controller.GetEventById(eventId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetEventById_WhenServiceThrowsException_Returns500InternalServerError()
    {
        // Arrange
        var eventId = "123";
        _mockApiService
            .Setup(s => s.GetEventByIdAsync(eventId))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetEventById(eventId);

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region CreateEvent Tests

    [Fact]
    public async Task CreateEvent_WithValidRequest_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var createRequest = new CreateEventRequest
        {
            Title = "New Meeting",
            Description = "Planning session",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(1).AddHours(2)
        };

        var createdEvent = new CalendarEvent
        {
            Id = "new-id",
            Title = createRequest.Title,
            Description = createRequest.Description,
            StartDate = createRequest.StartDate,
            EndDate = createRequest.EndDate
        };

        _mockApiService
            .Setup(s => s.CreateEventAsync(createRequest))
            .ReturnsAsync(createdEvent);

        // Act
        var result = await _controller.CreateEvent(createRequest);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(CalendarEventsController.GetEvents));
        createdResult.Value.Should().BeOfType<CalendarEvent>();

        _mockApiService.Verify(s => s.CreateEventAsync(createRequest), Times.Once);
    }

    [Fact]
    public async Task CreateEvent_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var createRequest = new CreateEventRequest
        {
            Title = string.Empty, // Invalid: required field
            Description = "Planning session",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(1).AddHours(2)
        };

        _controller.ModelState.AddModelError("Title", "The Title field is required.");

        // Act
        var result = await _controller.CreateEvent(createRequest);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateEvent_WhenServiceThrowsException_Returns500InternalServerError()
    {
        // Arrange
        var createRequest = new CreateEventRequest
        {
            Title = "New Meeting",
            Description = "Planning session",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(1).AddHours(2)
        };

        _mockApiService
            .Setup(s => s.CreateEventAsync(It.IsAny<CreateEventRequest>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CreateEvent(createRequest);

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task CreateEvent_WithValidRequest_LogsError()
    {
        // Arrange
        var createRequest = new CreateEventRequest
        {
            Title = "New Meeting",
            Description = "Planning session",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(1).AddHours(2)
        };

        var exception = new Exception("Creation failed");
        _mockApiService
            .Setup(s => s.CreateEventAsync(It.IsAny<CreateEventRequest>()))
            .ThrowsAsync(exception);

        // Act
        await _controller.CreateEvent(createRequest);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}
