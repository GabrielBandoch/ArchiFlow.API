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
                .ThenInclude(e => e.Tarefas)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Projeto>> GetAllWithEtapas() =>
        await _context.Projetos
            .Include(p => p.Etapas)
                .ThenInclude(e => e.Tarefas)
            .AsNoTracking()
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync();

    public async Task<IEnumerable<Projeto>> GetByClienteId(Guid clienteId) =>
        await _context.Projetos
            .Where(p => p.ClienteId == clienteId)
            .Include(p => p.Etapas)
                .ThenInclude(e => e.Tarefas)
            .AsNoTracking()
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync();

    public async Task<IEnumerable<Projeto>> GetByStatus(StatusProjeto status) =>
        await _context.Projetos
            .Where(p => p.Status == status)
            .Include(p => p.Etapas)
                .ThenInclude(e => e.Tarefas)
            .AsNoTracking()
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync();

    public async Task<EtapaProjeto?> GetEtapaById(Guid etapaId) =>
        await _context.EtapasProjeto
            .Include(e => e.Tarefas)
            .FirstOrDefaultAsync(e => e.Id == etapaId);

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

    public async Task<TarefaEtapa?> GetTarefaById(Guid id) =>
        await _context.TarefasEtapa.FindAsync(id);

    public async Task<TarefaEtapa> CreateTarefa(TarefaEtapa tarefa)
    {
        await _context.TarefasEtapa.AddAsync(tarefa);
        return tarefa;
    }

    public Task<TarefaEtapa> UpdateTarefa(TarefaEtapa tarefa)
    {
        _context.TarefasEtapa.Update(tarefa);
        return Task.FromResult(tarefa);
    }

    public Task DeleteTarefa(TarefaEtapa tarefa)
    {
        _context.TarefasEtapa.Remove(tarefa);
        return Task.CompletedTask;
    }
}
