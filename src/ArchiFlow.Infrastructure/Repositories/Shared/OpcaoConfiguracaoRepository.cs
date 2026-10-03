using ArchiFlow.Domain.Shared;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchiFlow.Infrastructure.Repositories.Shared;

public class OpcaoConfiguracaoRepository : Repository<OpcaoConfiguracao>, IOpcaoConfiguracaoRepository
{
    public OpcaoConfiguracaoRepository(ArchiFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<OpcaoConfiguracao>> ObterPorCategoriaAsync(string categoria)
    {
        var cat = categoria.Trim().ToLowerInvariant();
        return await _dbSet
            .Where(o => o.Ativo && o.Categoria.ToLower() == cat)
            .OrderBy(o => o.Ordem)
            .ToListAsync();
    }

    public async Task<IDictionary<string, IEnumerable<OpcaoConfiguracao>>> ObterTodasAgrupadasAsync()
    {
        var todas = await _dbSet
            .Where(o => o.Ativo)
            .OrderBy(o => o.Categoria)
            .ThenBy(o => o.Ordem)
            .ToListAsync();

        return todas
            .GroupBy(o => o.Categoria)
            .ToDictionary(g => g.Key, g => g.AsEnumerable());
    }
}
