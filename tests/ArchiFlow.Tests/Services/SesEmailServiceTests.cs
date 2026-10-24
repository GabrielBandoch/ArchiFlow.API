using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using ArchiFlow.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class SesEmailServiceTests
{
    private readonly Mock<IAmazonSimpleEmailService> _mockSes;

    public SesEmailServiceTests()
    {
        _mockSes = new Mock<IAmazonSimpleEmailService>();
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_SourceEmail_Is_Missing()
    {

        var configuration = new ConfigurationBuilder().Build();


        var act = () => new SesEmailService(configuration, _mockSes.Object);


        act.Should().Throw<ArgumentException>()
           .WithMessage("Configuração AWS_SOURCE_EMAIL não encontrada.");
    }

    [Fact]
    public void Constructor_Should_Succeed_When_SourceEmail_Is_Provided()
    {

        var inMemorySettings = new Dictionary<string, string?>
        {
            {"AWS_SOURCE_EMAIL", "noreply@archiflow.com"}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();


        var act = () => new SesEmailService(configuration, _mockSes.Object);


        act.Should().NotThrow();
    }

    [Fact]
    public async Task SendEmailAsync_Should_Call_SesClient_SendEmailAsync()
    {

        var inMemorySettings = new Dictionary<string, string?>
        {
            {"AWS_SOURCE_EMAIL", "noreply@archiflow.com"}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var service = new SesEmailService(configuration, _mockSes.Object);


        await service.SendEmailAsync("cliente@gmail.com", "Acesso ao Portal", "<p>Seu link</p>");


        _mockSes.Verify(s => s.SendEmailAsync(
            It.Is<SendEmailRequest>(r => r.Source == "noreply@archiflow.com" && r.Destination.ToAddresses.Contains("cliente@gmail.com")),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}

