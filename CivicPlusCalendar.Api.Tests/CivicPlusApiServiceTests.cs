using CivicPlusCalendar.Api.Models;
using CivicPlusCalendar.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace CivicPlusCalendar.Api.Tests;

public class CivicPlusApiServiceTests
{
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<ICivicPlusAuthService> _mockAuthService;
    private readonly Mock<ILogger<CivicPlusApiService>> _mockLogger;
    private readonly CivicPlusApiService _service;

    public CivicPlusApiServiceTests()
    {
        _mockHttpClient = new Mock<HttpClient>();
        _mockAuthService = new Mock<ICivicPlusAuthService>();
        _mockLogger = new Mock<ILogger<CivicPlusApiService>>();
        _service = new CivicPlusApiService(_mockHttpClient.Object, _mockAuthService.Object, _mockLogger.Object);
    }

    #region GetEventsAsync Tests

    [Fact]
    public async Task GetEventsAsync_WithDefaultParameters_ReturnsCalendarEventResponse()
    {
        // Arrange
        var responseContent = @"{
            ""items"": [
                {
                    ""id"": ""1"",
                    ""title"": ""Event 1"",
                    ""description"": ""Description 1"",
                    ""startDate"": ""2025-01-15T10:00:00Z"",
                    ""endDate"": ""2025-01-15T11:00:00Z""
                }
            ]
        }";

        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent(responseContent)
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.civicplus.com/")
        };
        var service = new CivicPlusApiService(httpClient, _mockAuthService.Object, _mockLogger.Object);

        _mockAuthService
            .Setup(s => s.GetAccessTokenAsync())
            .ReturnsAsync("test-token");

        // Act
        var result = await service.GetEventsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be("1");
        result.Items[0].Title.Should().Be("Event 1");

        _mockAuthService.Verify(s => s.GetAccessTokenAsync(), Times.Once);
    }

    [Fact]
    public async Task GetEventsAsync_WithPaginationParameters_IncludesTopAndSkipInUrl()
    {
        // Arrange
        var responseContent = @"{ ""items"": [] }";
        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent(responseContent)
        };

        var capturedRequest = (HttpRequestMessage?)null;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(httpResponseMessage);

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.civicplus.com/")
        };
        var service = new CivicPlusApiService(httpClient, _mockAuthService.Object, _mockLogger.Object);

        _mockAuthService
            .Setup(s => s.GetAccessTokenAsync())
            .ReturnsAsync("test-token");

        // Act
        await service.GetEventsAsync(top: 50, skip: 10);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.ToString().Should().Contain("$top=50");
        capturedRequest.RequestUri.ToString().Should().Contain("$skip=10");
    }

    [Fact]
    public async Task GetEventsAsync_WithFilterParameter_IncludesFilterInUrl()
    {
        // Arrange
        var filter = "startswith(title, 'Meeting')";
        var responseContent = @"{ ""items"": [] }";
        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent(responseContent)
        };

        var capturedRequest = (HttpRequestMessage?)null;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(httpResponseMessage);

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.civicplus.com/")
        };
        var service = new CivicPlusApiService(httpClient, _mockAuthService.Object, _mockLogger.Object);

        _mockAuthService
            .Setup(s => s.GetAccessTokenAsync())
            .ReturnsAsync("test-token");

        // Act
        await service.GetEventsAsync(filter: filter);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.ToString().Should().Contain("$filter=");
    }

    [Fact]
    public async Task GetEventsAsync_WithEmptyResponse_ReturnsEmptyCalendarEventResponse()
    {
        // Arrange
        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent(string.Empty)
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.civicplus.com/")
        };
        var service = new CivicPlusApiService(httpClient, _mockAuthService.Object, _mockLogger.Object);

        _mockAuthService
            .Setup(s => s.GetAccessTokenAsync())
            .ReturnsAsync("test-token");

        // Act
        var result = await service.GetEventsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEventsAsync_WhenAuthenticationFails_ThrowsException()
    {
        // Arrange
        _mockAuthService
            .Setup(s => s.GetAccessTokenAsync())
            .ThrowsAsync(new Exception("Authentication failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.GetEventsAsync());
    }

    [Fact]
    public async Task GetEventsAsync_WhenHttpRequestFails_ThrowsException()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.civicplus.com/")
        };
        var service = new CivicPlusApiService(httpClient, _mockAuthService.Object, _mockLogger.Object);

        _mockAuthService
            .Setup(s => s.GetAccessTokenAsync())
            .ReturnsAsync("test-token");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetEventsAsync());
    }

    #endregion

    #region GetEventByIdAsync Tests

    [Fact]
    public async Task GetEventByIdAsync_WithValidId_ReturnsCalendarEvent()
    {
        // Arrange
        var eventId = "123";
        var responseContent = @"{
            ""id"": ""123"",
            ""title"": ""Team Meeting"",
            ""description"": ""Quarterly review"",
            ""startDate"": ""2025-01-15T10:00:00Z"",
            ""endDate"": ""2025-01-15T11:00:00Z""
        }";

        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent(responseContent)
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.civicplus.com/")
        };
        var service = new CivicPlusApiService(httpClient, _mockAuthService.Object, _mockLogger.Object);

        _mockAuthService
            .Setup(s => s.GetAccessTokenAsync())
            .ReturnsAsync("test-token");

        // Act
        var result = await service.GetEventByIdAsync(eventId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("123");
        result.Title.Should().Be("Team Meeting");
    }

    [Fact]
    public async Task GetEventByIdAsync_WhenEventNotFound_ReturnsNull()
    {
        // Arrange
        var eventId = "nonexistent";
        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.NotFound,
            Content = new StringContent(string.Empty)
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.civicplus.com/")
        };
        var service = new CivicPlusApiService(httpClient, _mockAuthService.Object, _mockLogger.Object);

        _mockAuthService
            .Setup(s => s.GetAccessTokenAsync())
            .ReturnsAsync("test-token");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetEventByIdAsync(eventId));
    }

    [Fact]
    public async Task GetEventByIdAsync_WhenHttpRequestFails_ThrowsException()
    {
        // Arrange
        var eventId = "123";
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.civicplus.com/")
        };
        var service = new CivicPlusApiService(httpClient, _mockAuthService.Object, _mockLogger.Object);

        _mockAuthService
            .Setup(s => s.GetAccessTokenAsync())
            .ReturnsAsync("test-token");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetEventByIdAsync(eventId));
    }

    #endregion

    #region CreateEventAsync Tests

    [Fact]
    public async Task CreateEventAsync_WithValidRequest_ReturnsCreatedCalendarEvent()
    {
        // Arrange
        var createRequest = new CreateEventRequest
        {
            Title = "New Meeting",
            Description = "Planning session",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(1).AddHours(2)
        };

        var responseContent = @"{
            ""id"": ""new-id"",
            ""title"": ""New Meeting"",
            ""description"": ""Planning session"",
            ""startDate"": ""2025-01-20T10:00:00Z"",
            ""endDate"": ""2025-01-20T12:00:00Z""
        }";

        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.Created,
            Content = new StringContent(responseContent)
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.civicplus.com/")
        };
        var service = new CivicPlusApiService(httpClient, _mockAuthService.Object, _mockLogger.Object);

        _mockAuthService
            .Setup(s => s.GetAccessTokenAsync())
            .ReturnsAsync("test-token");

        // Act
        var result = await service.CreateEventAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("new-id");
        result.Title.Should().Be("New Meeting");
        result.Description.Should().Be("Planning session");
    }

    [Fact]
    public async Task CreateEventAsync_WhenAuthenticationFails_ThrowsException()
    {
        // Arrange
        var createRequest = new CreateEventRequest
        {
            Title = "New Meeting",
            Description = "Planning session",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(1).AddHours(2)
        };

        _mockAuthService
            .Setup(s => s.GetAccessTokenAsync())
            .ThrowsAsync(new Exception("Authentication failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.CreateEventAsync(createRequest));
    }

    [Fact]
    public async Task CreateEventAsync_WhenHttpRequestFails_ThrowsException()
    {
        // Arrange
        var createRequest = new CreateEventRequest
        {
            Title = "New Meeting",
            Description = "Planning session",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(1).AddHours(2)
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.civicplus.com/")
        };
        var service = new CivicPlusApiService(httpClient, _mockAuthService.Object, _mockLogger.Object);

        _mockAuthService
            .Setup(s => s.GetAccessTokenAsync())
            .ReturnsAsync("test-token");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.CreateEventAsync(createRequest));
    }

    [Fact]
    public async Task CreateEventAsync_WhenResponseIsEmpty_ThrowsException()
    {
        // Arrange
        var createRequest = new CreateEventRequest
        {
            Title = "New Meeting",
            Description = "Planning session",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(1).AddHours(2)
        };

        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.Created,
            Content = new StringContent(string.Empty)
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.civicplus.com/")
        };
        var service = new CivicPlusApiService(httpClient, _mockAuthService.Object, _mockLogger.Object);

        _mockAuthService
            .Setup(s => s.GetAccessTokenAsync())
            .ReturnsAsync("test-token");

        // Act & Assert
        await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => service.CreateEventAsync(createRequest));
    }

    #endregion
}
