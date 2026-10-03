using System;
using System.Threading.Tasks;
using ArchiFlow.Domain.Agenda;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchiFlow.Infrastructure.Repositories.Agenda;

public class ConfiguracaoAgendaRepository : Repository<ConfiguracaoAgendaEscritorio>, IConfiguracaoAgendaRepository
{
    public ConfiguracaoAgendaRepository(ArchiFlowDbContext context) : base(context)
    {
    }

    public async Task<ConfiguracaoAgendaEscritorio?> ObterPorEscritorioIdAsync(Guid escritorioId)
    {
        return await _context.ConfiguracoesAgendaEscritorio
            .FirstOrDefaultAsync(c => c.EscritorioId == escritorioId);
    }
}
