using ArchiFlow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Domain.Projetos;

public interface IArquivoRepository : IRepository<Arquivo>
{
    Task<IEnumerable<Arquivo>> GetByProjetoId(Guid projetoId);
}
