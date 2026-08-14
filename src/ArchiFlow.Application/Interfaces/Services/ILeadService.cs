using ArchiFlow.Application.Leads.Commands;
using ArchiFlow.Application.Leads.DTOs;

namespace ArchiFlow.Application.Interfaces.Services;

public interface ILeadService
{
    Task<IEnumerable<LeadDto>> GetAll();
    Task<LeadDto?> GetById(Guid id);
    Task<LeadDto> Create(CriarLeadCommand command);
    Task<LeadDto> Update(AtualizarLeadCommand command);
    Task<LeadDto> UpdateStatus(AtualizarStatusLeadCommand command);
    Task<HistoricoContatoLeadDto> RegisterContact(RegistrarContatoLeadCommand command);
    Task Delete(Guid id);
}
