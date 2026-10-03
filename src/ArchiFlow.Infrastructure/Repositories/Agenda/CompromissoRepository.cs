using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiFlow.Domain.Agenda;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchiFlow.Infrastructure.Repositories.Agenda;

public class CompromissoRepository : Repository<Compromisso>, ICompromissoRepository
{
    public CompromissoRepository(ArchiFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<Compromisso>> ObterPorPeriodoAsync(
        Guid escritorioId, 
        DateTime inicio, 
        DateTime fim, 
        Guid? usuarioId = null, 
        Guid? projetoId = null)
    {
        var query = _dbSet
            .Where(c => c.EscritorioId == escritorioId)
            .Where(c => c.DataHoraInicio <= fim && c.DataHoraFim >= inicio);

        if (usuarioId.HasValue)
        {
            query = query.Where(c => c.UsuarioId == usuarioId.Value);
        }

        if (projetoId.HasValue)
        {
            query = query.Where(c => c.ProjetoId == projetoId.Value);
        }

        return await query
            .OrderBy(c => c.DataHoraInicio)
            .ToListAsync();
    }

    public async Task<IEnumerable<Compromisso>> ObterProximosAsync(
        Guid escritorioId, 
        int quantidade = 10, 
        Guid? usuarioId = null)
    {
        var agora = DateTime.UtcNow.AddMinutes(-15);
        var query = _dbSet
            .Where(c => c.EscritorioId == escritorioId)
            .Where(c => c.DataHoraFim >= agora && c.Status != StatusCompromisso.Cancelado);

        if (usuarioId.HasValue)
        {
            query = query.Where(c => c.UsuarioId == usuarioId.Value);
        }

        return await query
            .OrderBy(c => c.DataHoraInicio)
            .Take(quantidade)
            .ToListAsync();
    }
}
