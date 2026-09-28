using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Repositories.Honorarios;

public class ConfiguracaoPropostaRepository : Repository<ConfiguracaoProposta>, IConfiguracaoPropostaRepository
{
    public ConfiguracaoPropostaRepository(ArchiFlowDbContext context) : base(context) { }

    public async Task<ConfiguracaoProposta?> ObterPorUsuarioIdAsync(Guid usuarioId)
    {
        return await _context.ConfiguracoesProposta
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
    }
}
