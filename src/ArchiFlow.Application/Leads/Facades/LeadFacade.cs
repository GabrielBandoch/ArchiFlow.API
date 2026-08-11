using ArchiFlow.Application.Leads.Commands;
using ArchiFlow.Application.Leads.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;

namespace ArchiFlow.Application.Leads.Facades;

public class LeadFacade : ILeadFacade
{
    private readonly ILeadService _service;

    public LeadFacade(ILeadService service) => _service = service;

    public Task<IEnumerable<LeadDto>> GetAll()                               => _service.GetAll();
    public Task<LeadDto?> GetById(Guid id)                                    => _service.GetById(id);
    public Task<LeadDto> Create(CriarLeadCommand command)                      => _service.Create(command);
    public Task<LeadDto> Update(AtualizarLeadCommand command)               => _service.Update(command);
    public Task<LeadDto> UpdateStatus(AtualizarStatusLeadCommand command)   => _service.UpdateStatus(command);
    public Task<HistoricoContatoLeadDto> RegisterContact(RegistrarContatoLeadCommand command) => _service.RegisterContact(command);
    public Task Delete(Guid id)                                                     => _service.Delete(id);
}
