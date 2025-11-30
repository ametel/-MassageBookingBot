using FluentAssertions;
using MassageBookingBot.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace MassageBookingBot.Infrastructure.Tests.Services;

public class GoogleCalendarServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<GoogleCalendarService>> _loggerMock;

    public GoogleCalendarServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<GoogleCalendarService>>();

        // Setup basic configuration
        _configurationMock.Setup(x => x["GoogleCalendar:CalendarId"]).Returns("primary");
        _configurationMock.Setup(x => x["GoogleCalendar:TimeZone"]).Returns("UTC");
        _configurationMock.Setup(x => x["GoogleCalendar:ApplicationName"]).Returns("Test App");
    }

    [Fact]
    public void Constructor_ShouldNotThrow_WhenConfigurationIsValid()
    {
        // Arrange
        _configurationMock.Setup(x => x["GoogleCalendar:ServiceAccountKeyPath"])
            .Returns("test-path.json");

        // Act
        var act = () => new GoogleCalendarService(_configurationMock.Object, _loggerMock.Object);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task CreateEventAsync_ShouldThrowArgumentException_WhenTitleIsEmpty()
    {
        // Arrange
        _configurationMock.Setup(x => x["GoogleCalendar:ServiceAccountKeyPath"])
            .Returns("test-path.json");
        
        var service = new GoogleCalendarService(_configurationMock.Object, _loggerMock.Object);
        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(1);

        // Act
        var act = async () => await service.CreateEventAsync("", "description", startTime, endTime);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Title*");
    }

    [Fact]
    public async Task CreateEventAsync_ShouldThrowArgumentException_WhenEndTimeBeforeStartTime()
    {
        // Arrange
        _configurationMock.Setup(x => x["GoogleCalendar:ServiceAccountKeyPath"])
            .Returns("test-path.json");
        
        var service = new GoogleCalendarService(_configurationMock.Object, _loggerMock.Object);
        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(-1);

        // Act
        var act = async () => await service.CreateEventAsync("Test", "description", startTime, endTime);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*End time*");
    }

    [Fact]
    public async Task UpdateEventAsync_ShouldThrowArgumentException_WhenEventIdIsEmpty()
    {
        // Arrange
        _configurationMock.Setup(x => x["GoogleCalendar:ServiceAccountKeyPath"])
            .Returns("test-path.json");
        
        var service = new GoogleCalendarService(_configurationMock.Object, _loggerMock.Object);
        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(1);

        // Act
        var act = async () => await service.UpdateEventAsync("", "title", "description", startTime, endTime);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Event ID*");
    }

    [Fact]
    public async Task DeleteEventAsync_ShouldThrowArgumentException_WhenEventIdIsEmpty()
    {
        // Arrange
        _configurationMock.Setup(x => x["GoogleCalendar:ServiceAccountKeyPath"])
            .Returns("test-path.json");
        
        var service = new GoogleCalendarService(_configurationMock.Object, _loggerMock.Object);

        // Act
        var act = async () => await service.DeleteEventAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Event ID*");
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        _configurationMock.Setup(x => x["GoogleCalendar:ServiceAccountKeyPath"])
            .Returns("test-path.json");
        
        var service = new GoogleCalendarService(_configurationMock.Object, _loggerMock.Object);

        // Act
        var act = () => service.Dispose();

        // Assert
        act.Should().NotThrow();
    }
}
