using ArchiFlow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Domain.Projetos;

public interface ITemplateProjetoRepository : IRepository<TemplateProjeto>
{
    Task<IEnumerable<TemplateProjeto>> GetAllWithEtapas();
    Task<TemplateProjeto?> GetByIdWithEtapas(Guid id);
    Task<TemplateProjeto?> GetByCodigoWithEtapas(string codigo);
    void RemoveEtapas(IEnumerable<TemplateEtapa> etapas);
    void AddEtapas(IEnumerable<TemplateEtapa> etapas);
}
