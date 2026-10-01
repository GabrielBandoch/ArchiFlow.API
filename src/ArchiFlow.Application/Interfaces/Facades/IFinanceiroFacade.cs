using ArchiFlow.Application.Financeiro.Commands;
using ArchiFlow.Application.Financeiro.DTOs;
using ArchiFlow.Domain.Financeiro;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Interfaces.Facades;

public interface IFinanceiroFacade
{
    Task<PainelFinanceiroDto> ObterPainelConsolidadoAsync();
    Task<IEnumerable<ParcelaFinanceiraDto>> ObterParcelasAsync(Guid? projetoId, StatusParcela? status, DateTime? inicio, DateTime? fim);
    Task<ParcelaFinanceiraDto?> ObterParcelaPorIdAsync(Guid id);
    Task<ContratoFinanceiroDto> CriarContratoAsync(CriarContratoCommand command);
    Task<ContratoFinanceiroDto?> ObterContratoPorProjetoIdAsync(Guid projetoId);
    Task<ParcelaFinanceiraDto> RegistrarParcelaAsync(CriarParcelaCommand command);
    Task<ParcelaFinanceiraDto> AtualizarParcelaAsync(Guid id, AtualizarParcelaCommand command);
    Task<ParcelaFinanceiraDto> DarBaixaParcelaAsync(Guid id, DarBaixaParcelaCommand command);
    Task ExcluirParcelaAsync(Guid id);
    Task<IEnumerable<DespesaProjetoDto>> ObterDespesasAsync(Guid? projetoId, CategoriaDespesa? categoria, DateTime? inicio, DateTime? fim);
    Task<DespesaProjetoDto?> ObterDespesaPorIdAsync(Guid id);
    Task<DespesaProjetoDto> CriarDespesaAsync(CriarDespesaCommand command);
    Task<DespesaProjetoDto> AtualizarDespesaAsync(Guid id, AtualizarDespesaCommand command);
    Task ExcluirDespesaAsync(Guid id);
    Task<IEnumerable<AlertaFinanceiroDto>> ObterAlertasAsync();
    Task<ComprovanteUploadResultDto> UploadComprovanteAsync(UploadComprovanteCommand command);
    Task ExcluirComprovanteAsync(string fileUrl);
}
