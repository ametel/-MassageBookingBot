using FluentAssertions;
using MassageBookingBot.Domain.Entities;
using MassageBookingBot.Domain.Enums;

namespace MassageBookingBot.Domain.Tests.Entities;

public class BookingTests
{
    [Fact]
    public void Booking_ShouldBeCreatedWithValidProperties()
    {
        // Arrange & Act
        var booking = new Booking
        {
            UserId = 1,
            ServiceId = 2,
            BookingDateTime = DateTime.UtcNow.AddDays(1),
            Status = BookingStatus.Pending,
            Notes = "Test booking"
        };

        // Assert
        booking.UserId.Should().Be(1);
        booking.ServiceId.Should().Be(2);
        booking.Status.Should().Be(BookingStatus.Pending);
        booking.Notes.Should().Be("Test booking");
        booking.ConfirmationSent.Should().BeFalse();
        booking.Reminder24hSent.Should().BeFalse();
        booking.Reminder2hSent.Should().BeFalse();
    }

    [Fact]
    public void Booking_ShouldAllowStatusChange()
    {
        // Arrange
        var booking = new Booking
        {
            Status = BookingStatus.Pending
        };

        // Act
        booking.Status = BookingStatus.Confirmed;

        // Assert
        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public void Booking_ShouldStoreGoogleCalendarEventId()
    {
        // Arrange
        var booking = new Booking();
        var eventId = "google-event-123";

        // Act
        booking.GoogleCalendarEventId = eventId;

        // Assert
        booking.GoogleCalendarEventId.Should().Be(eventId);
    }

    [Fact]
    public void Booking_ShouldTrackRemindersSent()
    {
        // Arrange
        var booking = new Booking();

        // Act
        booking.ConfirmationSent = true;
        booking.Reminder24hSent = true;
        booking.Reminder2hSent = true;

        // Assert
        booking.ConfirmationSent.Should().BeTrue();
        booking.Reminder24hSent.Should().BeTrue();
        booking.Reminder2hSent.Should().BeTrue();
    }
}
