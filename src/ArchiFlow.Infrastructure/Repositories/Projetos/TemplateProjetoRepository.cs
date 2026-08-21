using ArchiFlow.Domain.Projetos;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Repositories.Projetos;

public class TemplateProjetoRepository : Repository<TemplateProjeto>, ITemplateProjetoRepository
{
    public TemplateProjetoRepository(ArchiFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<TemplateProjeto>> GetAllWithEtapas() =>
        await _context.TemplatesProjeto
            .Include(t => t.Etapas)
            .Where(t => t.Ativo)
            .OrderBy(t => t.Nome)
            .AsNoTracking()
            .ToListAsync();

    public async Task<TemplateProjeto?> GetByIdWithEtapas(Guid id) =>
        await _context.TemplatesProjeto
            .Include(t => t.Etapas)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<TemplateProjeto?> GetByCodigoWithEtapas(string codigo) =>
        await _context.TemplatesProjeto
            .Include(t => t.Etapas)
            .FirstOrDefaultAsync(t => t.Codigo == codigo);
}
