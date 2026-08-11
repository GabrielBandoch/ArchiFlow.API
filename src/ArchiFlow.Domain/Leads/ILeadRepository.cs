using ArchiFlow.Domain.Shared;

namespace ArchiFlow.Domain.Leads;

public interface ILeadRepository : IRepository<Lead>
{
    Task<Lead?> GetByIdWithHistorico(Guid id);
    Task<IEnumerable<Lead>> GetAllWithHistorico();
    Task<Lead?> GetByEmail(string email);
    Task<HistoricoContatoLead> CreateHistorico(HistoricoContatoLead historico);
}
