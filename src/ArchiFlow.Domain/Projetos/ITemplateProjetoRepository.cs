using ArchiFlow.Domain.Shared;

namespace ArchiFlow.Domain.Projetos;

public interface ITemplateProjetoRepository : IRepository<TemplateProjeto>
{
    Task<IEnumerable<TemplateProjeto>> GetAllWithEtapas();
    Task<TemplateProjeto?> GetByIdWithEtapas(Guid id);
    Task<TemplateProjeto?> GetByCodigoWithEtapas(string codigo);
}
