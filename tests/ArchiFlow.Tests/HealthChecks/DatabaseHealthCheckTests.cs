using ArchiFlow.API.HealthChecks;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.HealthChecks;

public class DatabaseHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_QuandoBancoDisponivel_DeveRetornarHealthy()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var healthCheck = new DatabaseHealthCheck(context);
        var healthCheckContext = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(healthCheckContext);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("sucesso");
    }

    [Fact]
    public async Task CheckHealthAsync_QuandoBancoFalharOuDisposto_DeveRetornarUnhealthy()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        context.Dispose(); // Provoca falha ao acessar o banco
        
        var healthCheck = new DatabaseHealthCheck(context);
        var healthCheckContext = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(healthCheckContext);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("Falha");
        result.Exception.Should().NotBeNull();
    }
}
