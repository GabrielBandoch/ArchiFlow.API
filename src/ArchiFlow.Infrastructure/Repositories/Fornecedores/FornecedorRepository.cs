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
            .Include(f => f.ProjetosVinculados)
            .OrderByDescending(f => f.AvaliacaoMedia)
            .ToListAsync();
    }

    public async Task<Fornecedor?> ObterPorIdComRelacionamentosAsync(Guid id)
    {
        return await _dbSet
            .AsSplitQuery()
            .Include(f => f.Avaliacoes)
            .Include(f => f.ProjetosVinculados)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<Fornecedor>> ObterPorEspecialidadeAsync(string especialidade)
    {
        var esp = especialidade.Trim().ToLowerInvariant();
        return await _dbSet
            .AsSplitQuery()
            .Include(f => f.Avaliacoes)
            .Include(f => f.ProjetosVinculados)
            .Where(f => f.Especialidade.ToLower() == esp || f.Especialidade.ToLower().Contains(esp))
            .OrderByDescending(f => f.AvaliacaoMedia)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjetoFornecedor>> ObterFornecedoresDoProjetoAsync(Guid projetoId)
    {
        return await _context.ProjetosFornecedores
            .Include(pf => pf.Fornecedor)
            .Where(pf => pf.ProjetoId == projetoId)
            .ToListAsync();
    }

    public async Task AdicionarAvaliacaoAsync(AvaliacaoFornecedor avaliacao)
    {
        await _context.AvaliacoesFornecedores.AddAsync(avaliacao);
        await _context.SaveChangesAsync();

        var fornecedor = await ObterPorIdComRelacionamentosAsync(avaliacao.FornecedorId);
        if (fornecedor != null)
        {
            fornecedor.RecalcularMedia();
            await _context.SaveChangesAsync();
        }
    }

    public async Task AdicionarVinculoProjetoAsync(ProjetoFornecedor vinculo)
    {
        await _context.ProjetosFornecedores.AddAsync(vinculo);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverVinculoProjetoAsync(Guid vinculoId)
    {
        var vinculo = await _context.ProjetosFornecedores.FindAsync(vinculoId);
        if (vinculo != null)
        {
            _context.ProjetosFornecedores.Remove(vinculo);
            await _context.SaveChangesAsync();
        }
    }
}
