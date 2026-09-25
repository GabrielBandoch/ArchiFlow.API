using ArchiFlow.Application.Honorarios.Commands;
using ArchiFlow.Application.Honorarios.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Interfaces.Facades;

public interface IPropostaHonorarioFacade
{
    SimulacaoResultadoDto Simular(SimulacaoParametrosDto parametros);
    Task<IEnumerable<PropostaHonorarioDto>> GetAll();
    Task<PropostaHonorarioDto?> GetById(Guid id);
    Task<IEnumerable<PropostaHonorarioDto>> GetByClienteId(Guid clienteId);
    Task<IEnumerable<PropostaHonorarioDto>> GetByLeadId(Guid leadId);
    Task<PropostaHonorarioDto> Criar(CriarPropostaCommand command);
    Task<PropostaHonorarioDto> AtualizarStatus(Guid id, AtualizarStatusPropostaCommand command);
    Task<PropostaHonorarioDto> AjustarValor(Guid id, AjustarValorPropostaCommand command);
    Task<bool> Excluir(Guid id);
}
