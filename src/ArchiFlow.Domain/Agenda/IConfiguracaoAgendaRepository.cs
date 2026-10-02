using System;
using System.Threading.Tasks;
using ArchiFlow.Domain.Shared;

namespace ArchiFlow.Domain.Agenda;

public interface IConfiguracaoAgendaRepository : IRepository<ConfiguracaoAgendaEscritorio>
{
    Task<ConfiguracaoAgendaEscritorio?> ObterPorEscritorioIdAsync(Guid escritorioId);
}
