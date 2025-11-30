using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MassageBookingBot.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MassageBookingBot.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MassageBookingBot.Api.Tests.Controllers;

public class BookingsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BookingsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<IApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<IApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });
    }

    [Fact]
    public async Task GetBookings_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/bookings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBookings_ShouldReturnListOfBookings()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/bookings");
        var bookings = await response.Content.ReadFromJsonAsync<List<BookingDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        bookings.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateBooking_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidBooking = new CreateBookingDto
        {
            UserId = 0, // Invalid
            ServiceId = 1,
            BookingDateTime = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/bookings", invalidBooking);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateBooking_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var updateBooking = new UpdateBookingDto
        {
            BookingDateTime = DateTime.UtcNow.AddDays(2),
            Status = Domain.Enums.BookingStatus.Confirmed
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/bookings/99999", updateBooking);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelBooking_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/bookings/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
