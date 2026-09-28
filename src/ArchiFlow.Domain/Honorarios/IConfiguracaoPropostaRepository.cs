using ArchiFlow.Domain.Shared;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.Domain.Honorarios;

public interface IConfiguracaoPropostaRepository : IRepository<ConfiguracaoProposta>
{
    Task<ConfiguracaoProposta?> ObterPorUsuarioIdAsync(Guid usuarioId);
}
