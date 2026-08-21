using ArchiFlow.Domain.Shared;
using ArchiFlow.Domain.Projetos.Enum;

namespace ArchiFlow.Domain.Projetos;

public interface IProjetoRepository : IRepository<Projeto>
{
    Task<Projeto?> GetByIdWithEtapas(Guid id);
    Task<IEnumerable<Projeto>> GetAllWithEtapas();
    Task<IEnumerable<Projeto>> GetByClienteId(Guid clienteId);
    Task<IEnumerable<Projeto>> GetByStatus(StatusProjeto status);
    Task<EtapaProjeto?> GetEtapaById(Guid etapaId);
    Task<EtapaProjeto> CreateEtapa(EtapaProjeto etapa);
    Task<EtapaProjeto> UpdateEtapa(EtapaProjeto etapa);
    Task<TarefaEtapa?> GetTarefaById(Guid id);
    Task<TarefaEtapa> CreateTarefa(TarefaEtapa tarefa);
    Task<TarefaEtapa> UpdateTarefa(TarefaEtapa tarefa);
    Task DeleteTarefa(TarefaEtapa tarefa);
}
