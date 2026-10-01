using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class SmtpEmailServiceTests
{
    private readonly Mock<ILogger<SmtpEmailService>> _loggerMock;

    public SmtpEmailServiceTests()
    {
        _loggerMock = new Mock<ILogger<SmtpEmailService>>();
    }

    [Fact]
    public async Task SendEmailAsync_SemCredenciaisConfiguradas_NaoEnviaERetornaSemExcecao()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"SMTP_HOST", "smtp.hostinger.com"},
            {"SMTP_PORT", "587"},
            {"SMTP_USER", null},
            {"SMTP_PASSWORD", null}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var service = new SmtpEmailService(configuration, _loggerMock.Object);

        // Act
        var act = async () => await service.SendEmailAsync("destino@teste.com", "Assunto", "Mensagem");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendEmailAsync_ComSenhaVazia_NaoEnviaERetornaSemExcecao()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"SMTP_HOST", "smtp.hostinger.com"},
            {"SMTP_PORT", "587"},
            {"SMTP_USER", "usuario@teste.com"},
            {"SMTP_PASSWORD", "   "}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var service = new SmtpEmailService(configuration, _loggerMock.Object);

        // Act
        var act = async () => await service.SendEmailAsync("destino@teste.com", "Assunto", "Mensagem");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendEmailAsync_ComCredenciaisMasFalhaDeConexao_LancaInvalidOperationExceptionComContexto()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"SMTP_HOST", "127.0.0.1"},
            {"SMTP_PORT", "65530"},
            {"SMTP_USER", "usuario@teste.com"},
            {"SMTP_PASSWORD", "senha123"},
            {"SMTP_FROM_EMAIL", "remetente@teste.com"},
            {"SMTP_FROM_NAME", "ArchiFlow Test"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var service = new SmtpEmailService(configuration, _loggerMock.Object);

        // Act & Assert - test plain text HTML formatting branch
        var act = async () => await service.SendEmailAsync("destino@teste.com", "Assunto", "Linha 1\nLinha 2");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Falha no envio de e-mail via SMTP para destino@teste.com.*");
    }

    [Fact]
    public async Task SendEmailAsync_ComHtmlJaExistenteMasFalhaDeConexao_LancaInvalidOperationExceptionComContexto()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"SMTP_HOST", "127.0.0.1"},
            {"SMTP_PORT", "65530"},
            {"SMTP_USER", "usuario@teste.com"},
            {"SMTP_PASSWORD", "senha123"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var service = new SmtpEmailService(configuration, _loggerMock.Object);

        // Act & Assert - test HTML branch
        var act = async () => await service.SendEmailAsync("destino@teste.com", "Assunto", "<html><body>Ola</body></html>");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Falha no envio de e-mail via SMTP para destino@teste.com.*");
    }
}
