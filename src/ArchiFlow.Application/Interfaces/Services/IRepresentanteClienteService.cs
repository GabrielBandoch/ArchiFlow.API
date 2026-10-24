using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Clientes.DTOs;

namespace ArchiFlow.Application.Interfaces.Services;

public interface IRepresentanteClienteService
{
    Task<IEnumerable<RepresentanteClienteDto>> ObterPorClienteIdAsync(Guid clienteId);
    Task<RepresentanteClienteDto?> ObterPorIdAsync(Guid id);
    Task<RepresentanteClienteDto> ConvidarRepresentanteAsync(Guid clienteId, ConvidarRepresentanteCommand command, string? baseUrl = null);
    Task ReenviarConviteAsync(Guid representanteId, string? baseUrl = null);
    Task<RepresentanteClienteDto> AlterarStatusAsync(Guid representanteId, bool ativo);
    Task RemoverRepresentanteAsync(Guid representanteId);
    Task<ResultadoValidacaoConviteDto> ValidarTokenConviteAsync(string token);
    Task AtivarContaAsync(AtivarContaRepresentanteCommand command);
    Task DispararConviteProjetoCriadoAsync(Guid clienteId, string? baseUrl = null);
}

