using System.Collections.Generic;
using ArchiFlow.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class AppUrlServiceTests
{
    [Fact]
    public void ObterUrlBaseConfiavel_QuandoOriginPermitida_DeveRetornarOrigin()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "AllowedOrigins", "https://app.cotu.com.br,https://staging.cotu.com.br" },
            { "AppUrl", "https://cotu.com.br" }
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        var sut = new AppUrlService(config);

        var result = sut.ObterUrlBaseConfiavel("https://staging.cotu.com.br");

        result.Should().Be("https://staging.cotu.com.br");
    }

    [Fact]
    public void ObterUrlBaseConfiavel_QuandoOriginInseguraOuNaoPermitida_DeveRetornarFallbackConfigurado()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "AllowedOrigins", "https://app.cotu.com.br" },
            { "AppUrl", "https://app.cotu.com.br" }
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        var sut = new AppUrlService(config);

        var result = sut.ObterUrlBaseConfiavel("https://attacker.evil.com");

        result.Should().Be("https://app.cotu.com.br");
    }

    [Fact]
    public void ObterUrlBaseConfiavel_QuandoOriginNulaOuVazia_DeveRetornarFallbackConfigurado()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "AppUrl", "https://app.cotu.com.br" }
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        var sut = new AppUrlService(config);

        var result = sut.ObterUrlBaseConfiavel(null);

        result.Should().Be("https://app.cotu.com.br");
    }

    [Fact]
    public void ObterUrlBaseConfiavel_QuandoFallbackPadraoSemConfiguracao_DeveRetornarLocalhost()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var sut = new AppUrlService(config);

        var result = sut.ObterUrlBaseConfiavel("javascript:alert(1)");

        result.Should().Be("http://localhost:4200");
    }
}
