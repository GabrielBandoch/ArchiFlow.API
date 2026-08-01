using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchiFlow.Infrastructure.Repositories.Projetos;

public class ProjetoRepository : Repository<Projeto>, IProjetoRepository
{
    public ProjetoRepository(ArchiFlowDbContext context) : base(context) { }

    public async Task<Projeto?> GetByIdWithEtapas(Guid id) =>
        await _context.Projetos
            .Include(p => p.Etapas)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Projeto>> GetAllWithEtapas() =>
        await _context.Projetos
            .Include(p => p.Etapas)
            .AsNoTracking()
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync();

    public async Task<IEnumerable<Projeto>> GetByClienteId(Guid clienteId) =>
        await _context.Projetos
            .Where(p => p.ClienteId == clienteId)
            .AsNoTracking()
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync();

    public async Task<IEnumerable<Projeto>> GetByStatus(StatusProjeto status) =>
        await _context.Projetos
            .Where(p => p.Status == status)
            .AsNoTracking()
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync();

    public async Task<EtapaProjeto?> GetEtapaById(Guid etapaId) =>
        await _context.EtapasProjeto.FindAsync(etapaId);

    public async Task<EtapaProjeto> CreateEtapa(EtapaProjeto etapa)
    {
        await _context.EtapasProjeto.AddAsync(etapa);
        return etapa;
    }

    public Task<EtapaProjeto> UpdateEtapa(EtapaProjeto etapa)
    {
        _context.EtapasProjeto.Update(etapa);
        return Task.FromResult(etapa);
    }
}
