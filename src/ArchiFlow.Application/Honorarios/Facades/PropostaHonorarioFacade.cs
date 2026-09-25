using ArchiFlow.Application.Honorarios.Commands;
using ArchiFlow.Application.Honorarios.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Honorarios.Facades;

public class PropostaHonorarioFacade : IPropostaHonorarioFacade
{
    private readonly ICalculadoraHonorariosService _calculadora;
    private readonly IPropostaHonorarioService _service;

    public PropostaHonorarioFacade(
        ICalculadoraHonorariosService calculadora,
        IPropostaHonorarioService service)
    {
        _calculadora = calculadora;
        _service = service;
    }

    public SimulacaoResultadoDto Simular(SimulacaoParametrosDto parametros)
    {
        return _calculadora.Calcular(parametros);
    }

    public async Task<IEnumerable<PropostaHonorarioDto>> GetAll()
    {
        return await _service.GetAll();
    }

    public async Task<PropostaHonorarioDto?> GetById(Guid id)
    {
        return await _service.GetById(id);
    }

    public async Task<IEnumerable<PropostaHonorarioDto>> GetByClienteId(Guid clienteId)
    {
        return await _service.GetByClienteId(clienteId);
    }

    public async Task<IEnumerable<PropostaHonorarioDto>> GetByLeadId(Guid leadId)
    {
        return await _service.GetByLeadId(leadId);
    }

    public async Task<PropostaHonorarioDto> Criar(CriarPropostaCommand command)
    {
        return await _service.Criar(command);
    }

    public async Task<PropostaHonorarioDto> AtualizarStatus(Guid id, AtualizarStatusPropostaCommand command)
    {
        return await _service.AtualizarStatus(id, command);
    }

    public async Task<PropostaHonorarioDto> AjustarValor(Guid id, AjustarValorPropostaCommand command)
    {
        return await _service.AjustarValor(id, command);
    }

    public async Task<bool> Excluir(Guid id)
    {
        return await _service.Excluir(id);
    }
}
