using ArchiFlow.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class ConsoleEmailServiceTests
{
    [Fact]
    public async Task SendEmailAsync_Should_Execute_Without_Exceptions()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ConsoleEmailService>>();
        var service = new ConsoleEmailService(mockLogger.Object);

        // Act
        var act = async () => await service.SendEmailAsync(
            "cliente@exemplo.com", 
            "Assunto de Teste", 
            "<p>Corpo do e-mail de teste</p>"
        );

        // Assert
        await act.Should().NotThrowAsync();
    }
}
