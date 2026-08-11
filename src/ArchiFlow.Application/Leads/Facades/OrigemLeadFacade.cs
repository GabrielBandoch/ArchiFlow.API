using ArchiFlow.Application.Leads.Commands;
using ArchiFlow.Application.Leads.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Leads.Facades;

public class OrigemLeadFacade : IOrigemLeadFacade
{
    private readonly IOrigemLeadService _service;

    public OrigemLeadFacade(IOrigemLeadService service) => _service = service;

    public Task<IEnumerable<OrigemLeadDto>> GetAll()                           => _service.GetAll();
    public Task<IEnumerable<OrigemLeadDto>> GetAllActive()                     => _service.GetAllActive();
    public Task<OrigemLeadDto?> GetById(Guid id)                              => _service.GetById(id);
    public Task<OrigemLeadDto> Create(CriarOrigemLeadCommand command)          => _service.Create(command);
    public Task<OrigemLeadDto> Update(AtualizarOrigemLeadCommand command)      => _service.Update(command);
    public Task<OrigemLeadDto> Desativar(Guid id)                             => _service.Desativar(id);
    public Task<OrigemLeadDto> Reativar(Guid id)                              => _service.Reativar(id);
}
