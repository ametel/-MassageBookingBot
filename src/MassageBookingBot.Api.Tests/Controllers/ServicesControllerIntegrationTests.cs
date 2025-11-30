using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MassageBookingBot.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MassageBookingBot.Api.Tests.Controllers;

public class ServicesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ServicesControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetServices_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/services");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetServices_ShouldReturnListOfServices()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/services");
        var services = await response.Content.ReadFromJsonAsync<List<ServiceDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        services.Should().NotBeNull();
    }

    [Fact]
    public async Task GetServices_WithActiveOnlyFilter_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/services?activeOnly=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetServices_WithInactiveFilter_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/services?activeOnly=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
