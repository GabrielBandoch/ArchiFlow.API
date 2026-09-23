using ArchiFlow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Domain.Honorarios;

public interface IPropostaHonorarioRepository : IRepository<PropostaHonorario>
{
    Task<PropostaHonorario?> GetByIdWithItens(Guid id);
    Task<IEnumerable<PropostaHonorario>> GetAllWithItens();
    Task<IEnumerable<PropostaHonorario>> GetByClienteId(Guid clienteId);
    Task<IEnumerable<PropostaHonorario>> GetByLeadId(Guid leadId);
    Task<string> GerarProximoCodigo();
}
