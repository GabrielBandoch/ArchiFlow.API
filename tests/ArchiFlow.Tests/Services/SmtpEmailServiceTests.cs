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
}
