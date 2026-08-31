using ArchiFlow.Domain.Usuarios;
using ArchiFlow.Infrastructure.Data;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Data;

public class DbSeederTests
{
    [Fact]
    public async Task SeedAsync_QuandoBancoVazio_DeveInserirUsuariosPadrao()
    {
        using var context = TestDbContextFactory.Create();

        await DbSeeder.SeedAsync(context);

        var expectedAdminEmail = Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL") ?? "admin@archiflow.com";
        context.Usuarios.Should().HaveCount(3);
        context.Usuarios.Should().Contain(u => (u.Email == expectedAdminEmail || u.Email == "admin@archiflow.com") && u.Role == Roles.Administrador);
        context.Usuarios.Should().Contain(u => u.Role == Roles.Gerente);
        context.Usuarios.Should().Contain(u => u.Role == Roles.Colaborador);
    }

    [Fact]
    public async Task SeedAsync_QuandoBancoJaTemUsuarios_NaoDeveInserirNada()
    {
        using var context = TestDbContextFactory.Create();
        context.Usuarios.Add(new Usuario 
        { 
            Id = Guid.NewGuid(), 
            Nome = "Existente", 
            Email = "existente@test.com", 
            SenhaHash = "hash", 
            Role = Roles.Administrador, 
            Ativo = true 
        });
        await context.SaveChangesAsync();

        await DbSeeder.SeedAsync(context);

        context.Usuarios.Should().HaveCount(1);
    }

    [Fact]
    public async Task MigrateAndSeedAsync_QuandoFalhaMigracao_DeveLogarErro()
    {
        var servicesMock = new Mock<IServiceProvider>();
        var scopeMock = new Mock<IServiceScope>();
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();

        using var context = TestDbContextFactory.Create();

        servicesMock.Setup(s => s.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactoryMock.Object);
        scopeFactoryMock.Setup(s => s.CreateScope()).Returns(scopeMock.Object);
        
        var scopedServicesMock = new Mock<IServiceProvider>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(scopedServicesMock.Object);
        
        scopedServicesMock.Setup(s => s.GetService(typeof(ArchiFlowDbContext))).Returns(context);

        var loggerMock = new Mock<ILogger>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        
        scopedServicesMock.Setup(s => s.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

        var act = async () => await DbSeeder.MigrateAndSeedAsync(servicesMock.Object);

        await act.Should().NotThrowAsync();

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
