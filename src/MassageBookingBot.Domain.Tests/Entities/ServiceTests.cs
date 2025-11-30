using FluentAssertions;
using MassageBookingBot.Domain.Entities;

namespace MassageBookingBot.Domain.Tests.Entities;

public class ServiceTests
{
    [Fact]
    public void Service_ShouldBeCreatedWithValidProperties()
    {
        // Arrange & Act
        var service = new Service
        {
            Name = "Deep Tissue Massage",
            Description = "60-minute deep tissue massage",
            Price = 100.50m,
            DurationMinutes = 60,
            IsActive = true
        };

        // Assert
        service.Name.Should().Be("Deep Tissue Massage");
        service.Description.Should().Be("60-minute deep tissue massage");
        service.Price.Should().Be(100.50m);
        service.DurationMinutes.Should().Be(60);
        service.IsActive.Should().BeTrue();
        service.Bookings.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Service_Price_ShouldAcceptDecimalValues()
    {
        // Arrange
        var service = new Service();

        // Act
        service.Price = 99.99m;

        // Assert
        service.Price.Should().Be(99.99m);
    }

    [Fact]
    public void Service_IsActive_ShouldBeToggleable()
    {
        // Arrange
        var service = new Service { IsActive = true };

        // Act
        service.IsActive = false;

        // Assert
        service.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Service_Bookings_ShouldBeInitializedAsEmptyCollection()
    {
        // Arrange & Act
        var service = new Service();

        // Assert
        service.Bookings.Should().NotBeNull();
        service.Bookings.Should().BeEmpty();
    }
}
