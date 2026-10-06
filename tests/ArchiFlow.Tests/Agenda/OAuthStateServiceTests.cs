using System;
using ArchiFlow.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace ArchiFlow.Tests.Agenda;

public class OAuthStateServiceTests
{
    private readonly OAuthStateService _service = new();

    [Fact]
    public void GerarState_DeveRetornarStringHexadecimalCriptograficaValida()
    {
        var usuarioId = Guid.NewGuid();
        var escritorioId = Guid.NewGuid();

        var state = _service.GerarState(usuarioId, escritorioId);

        state.Should().NotBeNullOrWhiteSpace();
        state.Length.Should().Be(64); // 32 bytes em formato hex
    }

    [Fact]
    public void ValidarEConsumirState_ComParametrosCorretos_DeveRetornarTrue()
    {
        var usuarioId = Guid.NewGuid();
        var escritorioId = Guid.NewGuid();

        var state = _service.GerarState(usuarioId, escritorioId);
        var valido = _service.ValidarEConsumirState(state, usuarioId, escritorioId);

        valido.Should().BeTrue();
    }

    [Fact]
    public void ValidarEConsumirState_QuandoUsadoPelaSegundaVez_DeveRetornarFalseDevidoAUsoUnico()
    {
        var usuarioId = Guid.NewGuid();
        var escritorioId = Guid.NewGuid();

        var state = _service.GerarState(usuarioId, escritorioId);
        var primeiraTentativa = _service.ValidarEConsumirState(state, usuarioId, escritorioId);
        var segundaTentativa = _service.ValidarEConsumirState(state, usuarioId, escritorioId);

        primeiraTentativa.Should().BeTrue();
        segundaTentativa.Should().BeFalse();
    }

    [Fact]
    public void ValidarEConsumirState_ComUsuarioDiferente_DeveRetornarFalse()
    {
        var usuarioId = Guid.NewGuid();
        var escritorioId = Guid.NewGuid();
        var outroUsuarioId = Guid.NewGuid();

        var state = _service.GerarState(usuarioId, escritorioId);
        var valido = _service.ValidarEConsumirState(state, outroUsuarioId, escritorioId);

        valido.Should().BeFalse();
    }

    [Fact]
    public void ValidarEConsumirState_ComEscritorioDiferente_DeveRetornarFalse()
    {
        var usuarioId = Guid.NewGuid();
        var escritorioId = Guid.NewGuid();
        var outroEscritorioId = Guid.NewGuid();

        var state = _service.GerarState(usuarioId, escritorioId);
        var valido = _service.ValidarEConsumirState(state, usuarioId, outroEscritorioId);

        valido.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("estado-inexistente-123")]
    public void ValidarEConsumirState_ComStateInvalidoOuNulo_DeveRetornarFalse(string? stateInvalido)
    {
        var usuarioId = Guid.NewGuid();
        var escritorioId = Guid.NewGuid();

        var valido = _service.ValidarEConsumirState(stateInvalido, usuarioId, escritorioId);

        valido.Should().BeFalse();
    }
}
