using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Domain.Shared;

namespace ArchiFlow.Domain.Agenda;

public interface ICompromissoRepository : IRepository<Compromisso>
{
    Task<IEnumerable<Compromisso>> ObterPorPeriodoAsync(Guid escritorioId, DateTime inicio, DateTime fim, Guid? usuarioId = null, Guid? projetoId = null);
    Task<IEnumerable<Compromisso>> ObterProximosAsync(Guid escritorioId, int quantidade = 10, Guid? usuarioId = null);
}
