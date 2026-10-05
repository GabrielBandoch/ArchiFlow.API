using ArchiFlow.Domain.Fornecedores;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchiFlow.Infrastructure.Repositories.Fornecedores;

public class FornecedorRepository : Repository<Fornecedor>, IFornecedorRepository
{
    public FornecedorRepository(ArchiFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<Fornecedor>> ObterTodosComRelacionamentosAsync()
    {
        return await _dbSet
            .AsSplitQuery()
            .Include(f => f.Avaliacoes)
            .Include(f => f.ProjetosVinculados).ThenInclude(pv => pv.Projeto)
            .OrderByDescending(f => f.AvaliacaoMedia)
            .ToListAsync();
    }

    public async Task<Fornecedor?> ObterPorIdComRelacionamentosAsync(Guid id)
    {
        return await _dbSet
            .AsSplitQuery()
            .Include(f => f.Avaliacoes)
            .Include(f => f.ProjetosVinculados).ThenInclude(pv => pv.Projeto)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<Fornecedor>> ObterPorEspecialidadeAsync(string especialidade)
    {
        var esp = especialidade.Trim().ToLowerInvariant();
        return await _dbSet
            .AsSplitQuery()
            .Include(f => f.Avaliacoes)
            .Include(f => f.ProjetosVinculados).ThenInclude(pv => pv.Projeto)
            .Where(f => f.Especialidade.ToLower() == esp || f.Especialidade.ToLower().Contains(esp))
            .OrderByDescending(f => f.AvaliacaoMedia)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjetoFornecedor>> ObterFornecedoresDoProjetoAsync(Guid projetoId)
    {
        return await _context.ProjetosFornecedores
            .Include(pf => pf.Fornecedor)
            .Include(pf => pf.Projeto)
            .Where(pf => pf.ProjetoId == projetoId)
            .ToListAsync();
    }

    public async Task AdicionarAvaliacaoAsync(AvaliacaoFornecedor avaliacao)
    {
        await _context.AvaliacoesFornecedores.AddAsync(avaliacao);
    }

    public async Task AdicionarVinculoProjetoAsync(ProjetoFornecedor vinculo)
    {
        await _context.ProjetosFornecedores.AddAsync(vinculo);
    }

    public async Task<bool> RemoverVinculoProjetoAsync(Guid vinculoId)
    {
        var vinculo = await _context.ProjetosFornecedores.FindAsync(vinculoId);
        if (vinculo == null)
        {
            return false;
        }

        _context.ProjetosFornecedores.Remove(vinculo);
        return true;
    }
}
