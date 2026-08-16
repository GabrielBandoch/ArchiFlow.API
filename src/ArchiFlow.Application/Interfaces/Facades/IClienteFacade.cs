using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Clientes.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Interfaces.Facades;

public interface IClienteFacade
{
    Task<IEnumerable<ClienteDto>> GetAll();
    Task<ClienteDto?> GetById(Guid id);
    Task<ConversaoClienteResponseDto> ConvertLead(ConvertLeadToClienteCommand command);
    Task<ClienteDto> Update(AtualizarClienteCommand command);
    Task<ClienteDto> UpdatePortalAccess(AtualizarPortalAccessCommand command);
}
