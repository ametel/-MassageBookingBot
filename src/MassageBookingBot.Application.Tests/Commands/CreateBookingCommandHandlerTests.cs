using FluentAssertions;
using MassageBookingBot.Application.Commands.Bookings;
using MassageBookingBot.Application.DTOs;
using MassageBookingBot.Application.Interfaces;
using MassageBookingBot.Domain.Entities;
using MassageBookingBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace MassageBookingBot.Application.Tests.Commands;

public class CreateBookingCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICalendarService> _calendarServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<CreateBookingCommandHandler>> _loggerMock;
    private readonly CreateBookingCommandHandler _handler;

    public CreateBookingCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _calendarServiceMock = new Mock<ICalendarService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<CreateBookingCommandHandler>>();

        // Setup DbSets
        var bookings = new List<Booking>().AsQueryable();
        var mockBookingsSet = CreateMockDbSet(bookings);
        _contextMock.Setup(c => c.Bookings).Returns(mockBookingsSet.Object);

        var services = new List<Service>
        {
            new Service { Id = 1, Name = "Test Service", DurationMinutes = 60, Price = 100 }
        }.AsQueryable();
        var mockServicesSet = CreateMockDbSet(services);
        _contextMock.Setup(c => c.Services).Returns(mockServicesSet.Object);

        var users = new List<User>
        {
            new User { Id = 1, TelegramUserId = 123456, FirstName = "John", LastName = "Doe", PhoneNumber = "1234567890" }
        }.AsQueryable();
        var mockUsersSet = CreateMockDbSet(users);
        _contextMock.Setup(c => c.Users).Returns(mockUsersSet.Object);

        // Setup Database mock for transaction
        var mockDatabase = new Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade>(_contextMock.Object);
        var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);
        _contextMock.Setup(c => c.Database).Returns(mockDatabase.Object);

        _handler = new CreateBookingCommandHandler(
            _contextMock.Object,
            _calendarServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidBooking_ShouldCreateBookingSuccessfully()
    {
        // Arrange
        var bookingDto = new CreateBookingDto
        {
            UserId = 1,
            ServiceId = 1,
            BookingDateTime = DateTime.UtcNow.AddDays(1),
            Notes = "Test booking"
        };

        var command = new CreateBookingCommand(bookingDto);

        _calendarServiceMock
            .Setup(x => x.CreateEventAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("google-event-123");

        _notificationServiceMock
            .Setup(x => x.SendConfirmationAsync(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _calendarServiceMock.Verify(x => x.CreateEventAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidUserId_ShouldThrowException()
    {
        // Arrange
        var bookingDto = new CreateBookingDto
        {
            UserId = 999, // Non-existent user
            ServiceId = 1,
            BookingDateTime = DateTime.UtcNow.AddDays(1)
        };

        var command = new CreateBookingCommand(bookingDto);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*User not found*");
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        
        // Setup FindAsync for the mock DbSet
        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((ids, ct) =>
            {
                var id = (int)ids[0];
                var item = data.FirstOrDefault(e => EF.Property<int>(e, "Id") == id);
                return new ValueTask<T?>(item);
            });

        return mockSet;
    }
}
