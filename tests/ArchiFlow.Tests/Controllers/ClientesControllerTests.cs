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

public class ClientesControllerTests
{
    private readonly Mock<IClienteFacade> _facadeMock = new();
    private readonly Mock<IRepresentanteClienteFacade> _repFacadeMock = new();
    private readonly Mock<IAppUrlService> _appUrlServiceMock = new();
    private readonly ClientesController _sut;

    public ClientesControllerTests()
    {
        _sut = new ClientesController(_facadeMock.Object, _repFacadeMock.Object, _appUrlServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task GetAll_DeveRetornarOkComClientes()
    {
        var clientes = new List<ClienteDto>();
        _facadeMock.Setup(f => f.GetAll()).ReturnsAsync(clientes);

        var result = await _sut.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(clientes);
    }

    [Fact]
    public async Task GetById_QuandoExiste_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var dto = new ClienteDto(id, null, "Nome", "email@teste.com", null, null, null, true, 0, null);
        _facadeMock.Setup(f => f.GetById(id)).ReturnsAsync(dto);

        var result = await _sut.GetById(id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetById_QuandoNaoExiste_DeveRetornarNotFound()
    {
        _facadeMock.Setup(f => f.GetById(It.IsAny<Guid>())).ReturnsAsync((ClienteDto?)null);

        var result = await _sut.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ConvertLead_DeveRetornarOkComResposta()
    {
        var command = new ConvertLeadToClienteCommand(Guid.NewGuid());
        var response = new ConversaoClienteResponseDto(
            new ClienteDto(Guid.NewGuid(), command.LeadId, "Nome", "email@teste.com", null, null, null, true, 0, null),
            "Arch@1234"
        );
        _facadeMock.Setup(f => f.ConvertLead(command)).ReturnsAsync(response);

        var result = await _sut.ConvertLead(command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task Update_DeveRetornarOkComClienteAtualizado()
    {
        var id = Guid.NewGuid();
        var command = new AtualizarClienteCommand(id, "Novo Nome", "novo@email.com", null, null, null, null);
        var dto = new ClienteDto(id, null, "Novo Nome", "novo@email.com", null, null, null, true, 0, null);
        _facadeMock.Setup(f => f.Update(command)).ReturnsAsync(dto);

        var result = await _sut.Update(command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task UpdatePortalAccess_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var command = new AtualizarPortalAccessCommand(id, false);
        var dto = new ClienteDto(id, null, "Nome", "email@teste.com", null, null, null, false, 0, null);
        _facadeMock.Setup(f => f.UpdatePortalAccess(command)).ReturnsAsync(dto);

        var result = await _sut.UpdatePortalAccess(command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
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
