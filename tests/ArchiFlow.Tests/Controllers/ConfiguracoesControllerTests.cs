using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Configuracoes.DTOs;
using ArchiFlow.Application.Configuracoes.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class ConfiguracoesControllerTests
{
    private readonly Mock<IConfiguracaoSistemaService> _serviceMock;
    private readonly ConfiguracoesController _controller;

    public ConfiguracoesControllerTests()
    {
        _serviceMock = new Mock<IConfiguracaoSistemaService>();
        _controller = new ConfiguracoesController(_serviceMock.Object);
    }

    [Fact]
    public async Task ObterPorCategoria_DeveRetornarOkComOpcoes()
    {
        _serviceMock.Setup(s => s.ObterPorCategoriaAsync("TiposProjeto")).ReturnsAsync(new List<OpcaoConfiguracaoDto>());

        var result = await _controller.ObterPorCategoria("TiposProjeto");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ObterTodas_DeveRetornarOkComDicionario()
    {
        _serviceMock.Setup(s => s.ObterTodasAgrupadasAsync()).ReturnsAsync(new Dictionary<string, IEnumerable<OpcaoConfiguracaoDto>>());

        var result = await _controller.ObterTodas();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SalvarOpcao_DeveRetornarOkComOpcaoSalva()
    {
        var cmd = new SalvarOpcaoConfiguracaoCommand { Categoria = "Test", Chave = "K", Rotulo = "R" };
        var dto = new OpcaoConfiguracaoDto { Id = Guid.NewGuid(), Categoria = "Test", Chave = "K", Rotulo = "R" };
        _serviceMock.Setup(s => s.SalvarOpcaoAsync(cmd)).ReturnsAsync(dto);

        var result = await _controller.SalvarOpcao(cmd);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }
}
