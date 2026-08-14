using ArchiFlow.Domain.Leads;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Repositories.Leads;

public class OrigemLeadRepository : Repository<OrigemLead>, IOrigemLeadRepository
{
    public OrigemLeadRepository(ArchiFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<OrigemLead>> GetAllActive() =>
        await _context.OrigensLead
            .Where(o => o.Ativo)
            .OrderBy(o => o.Descricao)
            .ToListAsync();
}
