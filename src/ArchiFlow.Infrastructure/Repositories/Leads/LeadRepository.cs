using ArchiFlow.Domain.Leads;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchiFlow.Infrastructure.Repositories.Leads;

public class LeadRepository : Repository<Lead>, ILeadRepository
{
    public LeadRepository(ArchiFlowDbContext context) : base(context) { }

    public async Task<Lead?> GetByIdWithHistorico(Guid id) =>
        await _context.Leads
            .Include(l => l.HistoricoContatos)
            .Include(l => l.Origem)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<IEnumerable<Lead>> GetAllWithHistorico() =>
        await _context.Leads
            .Include(l => l.HistoricoContatos)
            .Include(l => l.Origem)
            .AsNoTracking()
            .OrderByDescending(l => l.CriadoEm)
            .ToListAsync();

    public async Task<Lead?> GetByEmail(string email) =>
        await _context.Leads
            .Include(l => l.Origem)
            .FirstOrDefaultAsync(l => l.Email == email);

    public async Task<HistoricoContatoLead> CreateHistorico(HistoricoContatoLead historico)
    {
        await _context.HistoricosContatoLead.AddAsync(historico);
        return historico;
    }
}
