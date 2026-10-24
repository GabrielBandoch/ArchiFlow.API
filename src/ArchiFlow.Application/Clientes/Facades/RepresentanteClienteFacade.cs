using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Clientes.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;

namespace ArchiFlow.Application.Clientes.Facades;

public class RepresentanteClienteFacade : IRepresentanteClienteFacade
{
    private readonly IRepresentanteClienteService _service;

    public RepresentanteClienteFacade(IRepresentanteClienteService service)
    {
        _service = service;
    }

    public Task<IEnumerable<RepresentanteClienteDto>> ObterPorClienteIdAsync(Guid clienteId) =>
        _service.ObterPorClienteIdAsync(clienteId);

    public Task<RepresentanteClienteDto?> ObterPorIdAsync(Guid id) =>
        _service.ObterPorIdAsync(id);

    public Task<RepresentanteClienteDto> ConvidarRepresentanteAsync(Guid clienteId, ConvidarRepresentanteCommand command, string? baseUrl = null) =>
        _service.ConvidarRepresentanteAsync(clienteId, command, baseUrl);

    public Task ReenviarConviteAsync(Guid representanteId, string? baseUrl = null) =>
        _service.ReenviarConviteAsync(representanteId, baseUrl);

    public Task<RepresentanteClienteDto> AlterarStatusAsync(Guid representanteId, bool ativo) =>
        _service.AlterarStatusAsync(representanteId, ativo);

    public Task RemoverRepresentanteAsync(Guid representanteId) =>
        _service.RemoverRepresentanteAsync(representanteId);

    public Task<ResultadoValidacaoConviteDto> ValidarTokenConviteAsync(string token) =>
        _service.ValidarTokenConviteAsync(token);

    public Task AtivarContaAsync(AtivarContaRepresentanteCommand command) =>
        _service.AtivarContaAsync(command);
}

