using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Clientes.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class ClienteRepresentantesControllerTests
{
    private readonly Mock<IRepresentanteClienteFacade> _repFacadeMock = new();
    private readonly Mock<IAppUrlService> _appUrlServiceMock = new();
    private readonly ClienteRepresentantesController _sut;

    public ClienteRepresentantesControllerTests()
    {
        _sut = new ClienteRepresentantesController(_repFacadeMock.Object, _appUrlServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task ObterRepresentantes_DeveRetornarOkComLista()
    {
        var clienteId = Guid.NewGuid();
        var lista = new List<RepresentanteClienteDto>
        {
            new(Guid.NewGuid(), clienteId, "João", "joao@teste.com", null, null, true, "Ativo", DateTime.UtcNow, null)
        };
        _repFacadeMock.Setup(r => r.ObterPorClienteIdAsync(clienteId)).ReturnsAsync(lista);

        var result = await _sut.ObterRepresentantes(clienteId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(lista);
    }

    [Fact]
    public async Task ConvidarRepresentante_DeveRetornarOk()
    {
        var clienteId = Guid.NewGuid();
        var command = new ConvidarRepresentanteCommand("Maria", "maria@teste.com");
        var rep = new RepresentanteClienteDto(Guid.NewGuid(), clienteId, "Maria", "maria@teste.com", null, null, true, "Pendente", DateTime.UtcNow, null);
        _appUrlServiceMock.Setup(a => a.ObterUrlBaseConfiavel(It.IsAny<string?>())).Returns("https://app.cotu.com.br");
        _repFacadeMock.Setup(r => r.ConvidarRepresentanteAsync(clienteId, command, "https://app.cotu.com.br")).ReturnsAsync(rep);

        var result = await _sut.ConvidarRepresentante(clienteId, command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(rep);
    }

    [Fact]
    public async Task ReenviarConvite_DeveRetornarNoContent()
    {
        var repId = Guid.NewGuid();
        _appUrlServiceMock.Setup(a => a.ObterUrlBaseConfiavel(It.IsAny<string?>())).Returns("https://app.cotu.com.br");

        var result = await _sut.ReenviarConvite(repId);

        result.Should().BeOfType<NoContentResult>();
        _repFacadeMock.Verify(r => r.ReenviarConviteAsync(repId, "https://app.cotu.com.br"), Times.Once);
    }

    [Fact]
    public async Task AlterarStatusRepresentante_DeveRetornarOk()
    {
        var repId = Guid.NewGuid();
        var command = new AlterarStatusRepresentanteCommand(false);
        var rep = new RepresentanteClienteDto(repId, Guid.NewGuid(), "Carlos", "carlos@teste.com", null, null, false, "Inativo", DateTime.UtcNow, null);
        _repFacadeMock.Setup(r => r.AlterarStatusAsync(repId, false)).ReturnsAsync(rep);

        var result = await _sut.AlterarStatusRepresentante(repId, command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(rep);
    }

    [Fact]
    public async Task RemoverRepresentante_DeveRetornarNoContent()
    {
        var repId = Guid.NewGuid();

        var result = await _sut.RemoverRepresentante(repId);

        result.Should().BeOfType<NoContentResult>();
        _repFacadeMock.Verify(r => r.RemoverRepresentanteAsync(repId), Times.Once);
    }
}

