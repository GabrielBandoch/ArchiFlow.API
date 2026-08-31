using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Clientes.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Clientes.Facades;

public class ClienteFacade : IClienteFacade
{
    private readonly IClienteService _service;

    public ClienteFacade(IClienteService service) => _service = service;

    public Task<IEnumerable<ClienteDto>> GetAll() => _service.GetAll();
    public Task<ClienteDto?> GetById(Guid id) => _service.GetById(id);
    public Task<ConversaoClienteResponseDto> ConvertLead(ConvertLeadToClienteCommand command) => _service.ConvertLead(command);
    public Task<ClienteDto> Update(AtualizarClienteCommand command) => _service.Update(command);
    public Task<ClienteDto> UpdatePortalAccess(AtualizarPortalAccessCommand command) => _service.UpdatePortalAccess(command);
}
