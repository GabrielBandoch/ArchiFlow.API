using ArchiFlow.Application.Financeiro.Commands;
using ArchiFlow.Application.Financeiro.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Financeiro;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Financeiro.Facades;

public class FinanceiroFacade : IFinanceiroFacade
{
    private readonly IFinanceiroService _financeiroService;

    public FinanceiroFacade(IFinanceiroService financeiroService)
    {
        _financeiroService = financeiroService;
    }

    public Task<PainelFinanceiroDto> ObterPainelConsolidadoAsync() =>
        _financeiroService.ObterPainelConsolidadoAsync();

    public Task<IEnumerable<ParcelaFinanceiraDto>> ObterParcelasAsync(Guid? projetoId, StatusParcela? status, DateTime? inicio, DateTime? fim) =>
        _financeiroService.ObterParcelasAsync(projetoId, status, inicio, fim);

    public Task<ParcelaFinanceiraDto?> ObterParcelaPorIdAsync(Guid id) =>
        _financeiroService.ObterParcelaPorIdAsync(id);

    public Task<ContratoFinanceiroDto> CriarContratoAsync(CriarContratoCommand command) =>
        _financeiroService.CriarContratoAsync(command);

    public Task<ContratoFinanceiroDto?> ObterContratoPorProjetoIdAsync(Guid projetoId) =>
        _financeiroService.ObterContratoPorProjetoIdAsync(projetoId);

    public Task<ParcelaFinanceiraDto> RegistrarParcelaAsync(CriarParcelaCommand command) =>
        _financeiroService.RegistrarParcelaAsync(command);

    public Task<ParcelaFinanceiraDto> AtualizarParcelaAsync(Guid id, AtualizarParcelaCommand command) =>
        _financeiroService.AtualizarParcelaAsync(id, command);

    public Task<ParcelaFinanceiraDto> DarBaixaParcelaAsync(Guid id, DarBaixaParcelaCommand command) =>
        _financeiroService.DarBaixaParcelaAsync(id, command);

    public Task ExcluirParcelaAsync(Guid id) =>
        _financeiroService.ExcluirParcelaAsync(id);

    public Task<IEnumerable<DespesaProjetoDto>> ObterDespesasAsync(Guid? projetoId, CategoriaDespesa? categoria, DateTime? inicio, DateTime? fim) =>
        _financeiroService.ObterDespesasAsync(projetoId, categoria, inicio, fim);

    public Task<DespesaProjetoDto?> ObterDespesaPorIdAsync(Guid id) =>
        _financeiroService.ObterDespesaPorIdAsync(id);

    public Task<DespesaProjetoDto> CriarDespesaAsync(CriarDespesaCommand command) =>
        _financeiroService.CriarDespesaAsync(command);

    public Task<DespesaProjetoDto> AtualizarDespesaAsync(Guid id, AtualizarDespesaCommand command) =>
        _financeiroService.AtualizarDespesaAsync(id, command);

    public Task ExcluirDespesaAsync(Guid id) =>
        _financeiroService.ExcluirDespesaAsync(id);

    public Task<IEnumerable<AlertaFinanceiroDto>> ObterAlertasAsync() =>
        _financeiroService.ObterAlertasAsync();

    public Task<ComprovanteUploadResultDto> UploadComprovanteAsync(UploadComprovanteCommand command) =>
        _financeiroService.UploadComprovanteAsync(command);

    public Task ExcluirComprovanteAsync(string fileUrl) =>
        _financeiroService.ExcluirComprovanteAsync(fileUrl);
}
