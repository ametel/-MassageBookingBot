using FluentAssertions;
using MassageBookingBot.Application.DTOs;
using MassageBookingBot.Application.Validators;

namespace MassageBookingBot.Application.Tests.Validators;

public class CreateBookingDtoValidatorTests
{
    private readonly CreateBookingDtoValidator _validator;

    public CreateBookingDtoValidatorTests()
    {
        _validator = new CreateBookingDtoValidator();
    }

    [Fact]
    public async Task Validate_ValidBooking_ShouldPass()
    {
        // Arrange
        var booking = new CreateBookingDto
        {
            UserId = 1,
            ServiceId = 1,
            BookingDateTime = DateTime.UtcNow.AddDays(1),
            Notes = "Test booking"
        };

        // Act
        var result = await _validator.ValidateAsync(booking);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_InvalidUserId_ShouldFail(int userId)
    {
        // Arrange
        var booking = new CreateBookingDto
        {
            UserId = userId,
            ServiceId = 1,
            BookingDateTime = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = await _validator.ValidateAsync(booking);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingDto.UserId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_InvalidServiceId_ShouldFail(int serviceId)
    {
        // Arrange
        var booking = new CreateBookingDto
        {
            UserId = 1,
            ServiceId = serviceId,
            BookingDateTime = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = await _validator.ValidateAsync(booking);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingDto.ServiceId));
    }

    [Fact]
    public async Task Validate_PastBookingDateTime_ShouldFail()
    {
        // Arrange
        var booking = new CreateBookingDto
        {
            UserId = 1,
            ServiceId = 1,
            BookingDateTime = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = await _validator.ValidateAsync(booking);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingDto.BookingDateTime));
    }

    [Fact]
    public async Task Validate_NotesTooLong_ShouldFail()
    {
        // Arrange
        var booking = new CreateBookingDto
        {
            UserId = 1,
            ServiceId = 1,
            BookingDateTime = DateTime.UtcNow.AddDays(1),
            Notes = new string('x', 501) // 501 characters
        };

        // Act
        var result = await _validator.ValidateAsync(booking);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingDto.Notes));
    }

    [Fact]
    public async Task Validate_NotesExactlyMaxLength_ShouldPass()
    {
        // Arrange
        var booking = new CreateBookingDto
        {
            UserId = 1,
            ServiceId = 1,
            BookingDateTime = DateTime.UtcNow.AddDays(1),
            Notes = new string('x', 500) // Exactly 500 characters
        };

        // Act
        var result = await _validator.ValidateAsync(booking);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
