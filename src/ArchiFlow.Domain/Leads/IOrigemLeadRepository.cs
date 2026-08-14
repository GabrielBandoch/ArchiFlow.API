using ArchiFlow.Domain.Shared;

namespace ArchiFlow.Domain.Leads;

public interface IOrigemLeadRepository : IRepository<OrigemLead>
{
    Task<IEnumerable<OrigemLead>> GetAllActive();
}
