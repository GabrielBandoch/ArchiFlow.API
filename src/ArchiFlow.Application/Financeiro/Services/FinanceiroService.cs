using ArchiFlow.Application.Financeiro.Commands;
using ArchiFlow.Application.Financeiro.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Financeiro;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Shared;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Financeiro.Services;

public class FinanceiroService : IFinanceiroService
{
    private readonly IParcelaFinanceiraRepository _parcelaRepository;
    private readonly IContratoFinanceiroRepository _contratoRepository;
    private readonly IDespesaProjetoRepository _despesaRepository;
    private readonly IProjetoRepository _projetoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<FinanceiroService> _logger;
    private readonly IStorageService? _storageService;

    public FinanceiroService(
        IParcelaFinanceiraRepository parcelaRepository,
        IContratoFinanceiroRepository contratoRepository,
        IDespesaProjetoRepository despesaRepository,
        IProjetoRepository projetoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<FinanceiroService> logger,
        IStorageService? storageService = null)
    {
        _parcelaRepository = parcelaRepository;
        _contratoRepository = contratoRepository;
        _despesaRepository = despesaRepository;
        _projetoRepository = projetoRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _storageService = storageService;
    }

    public async Task<PainelFinanceiroDto> ObterPainelConsolidadoAsync()
    {
        var parcelas = (await _parcelaRepository.ObterTodasComProjetoAsync()).ToList();
        var despesas = (await _despesaRepository.ObterTodasComProjetoAsync()).ToList();

        var hoje = DateTime.UtcNow.Date;
        AtualizarStatusParcelasVencidas(parcelas, hoje);

        var totalPrevisto = parcelas.Where(p => p.Status != StatusParcela.Cancelado).Sum(p => p.Valor);
        var totalRecebido = parcelas.Where(p => p.Status == StatusParcela.Pago).Sum(p => p.Valor);
        var totalPendente = parcelas.Where(p => p.Status == StatusParcela.Pendente).Sum(p => p.Valor);
        var totalAtrasado = parcelas.Where(p => p.Status == StatusParcela.Atrasado).Sum(p => p.Valor);
        var totalDespesas = despesas.Sum(d => d.Valor);
        var saldoLiquido = totalRecebido - totalDespesas;

        var variacao = CalcularVariacaoMensal(parcelas, hoje);
        var receitasMes = GerarReceitasPorMes(parcelas, despesas, hoje.Year);
        var alertas = GerarAlertasFinanceiros(parcelas, hoje);
        var parcelasRecentes = parcelas.Take(10).Select(MapearParaDto).ToList();

        return new PainelFinanceiroDto(
            totalPrevisto,
            totalRecebido,
            totalPendente,
            totalAtrasado,
            totalDespesas,
            saldoLiquido,
            variacao,
            receitasMes,
            alertas,
            parcelasRecentes
        );
    }

    public async Task<IEnumerable<ParcelaFinanceiraDto>> ObterParcelasAsync(Guid? projetoId, StatusParcela? status, DateTime? inicio, DateTime? fim)
    {
        var parcelas = await _parcelaRepository.ObterComFiltroAsync(projetoId, status, inicio, fim);
        return parcelas.Select(MapearParaDto);
    }

    public async Task<ParcelaFinanceiraDto?> ObterParcelaPorIdAsync(Guid id)
    {
        var parcela = await _parcelaRepository.GetById(id);
        return parcela is null ? null : MapearParaDto(parcela);
    }

    public async Task<ContratoFinanceiroDto> CriarContratoAsync(CriarContratoCommand command)
    {
        var projeto = await _projetoRepository.GetById(command.ProjetoId);
        if (projeto is null)
            throw new KeyNotFoundException($"Projeto com Id {command.ProjetoId} não encontrado.");

        var contrato = new ContratoFinanceiro
        {
            Id = Guid.NewGuid(),
            ProjetoId = command.ProjetoId,
            ValorTotal = command.ValorTotal,
            CondicoesPagamento = command.CondicoesPagamento,
            Observacoes = command.Observacoes,
            CriadoEm = DateTime.UtcNow
        };

        await _contratoRepository.Create(contrato);

        var parcelas = GerarParcelasContrato(contrato.Id, command);
        foreach (var parcela in parcelas)
        {
            await _parcelaRepository.Create(parcela);
        }

        contrato.Parcelas = parcelas;

        await _unitOfWork.Commit();

        return MapearContratoDto(contrato, projeto.Nome ?? string.Empty);
    }

    public async Task<ContratoFinanceiroDto?> ObterContratoPorProjetoIdAsync(Guid projetoId)
    {
        var contrato = await _contratoRepository.ObterPorProjetoIdAsync(projetoId);
        if (contrato is null) return null;

        var projetoNome = contrato.Projeto?.Nome ?? string.Empty;
        return MapearContratoDto(contrato, projetoNome);
    }

    public async Task<ParcelaFinanceiraDto> RegistrarParcelaAsync(CriarParcelaCommand command)
    {
        var projeto = await _projetoRepository.GetById(command.ProjetoId);
        if (projeto is null)
            throw new KeyNotFoundException($"Projeto com Id {command.ProjetoId} não encontrado.");

        var parcela = new ParcelaFinanceira
        {
            Id = Guid.NewGuid(),
            ProjetoId = command.ProjetoId,
            ContratoFinanceiroId = command.ContratoFinanceiroId,
            NumeroParcela = command.NumeroParcela > 0 ? command.NumeroParcela : 1,
            TotalParcelas = command.TotalParcelas > 0 ? command.TotalParcelas : 1,
            Descricao = command.Descricao,
            Valor = command.Valor,
            DataVencimento = command.DataVencimento,
            Status = command.DataVencimento.Date < DateTime.UtcNow.Date ? StatusParcela.Atrasado : StatusParcela.Pendente,
            Observacoes = command.Observacoes,
            CriadoEm = DateTime.UtcNow
        };

        parcela.Projeto = projeto;

        await _parcelaRepository.Create(parcela);
        await _unitOfWork.Commit();

        _logger.LogInformation("Parcela {ParcelaId} criada para o projeto {ProjetoId}", parcela.Id, command.ProjetoId);
        return MapearParaDto(parcela);
    }

    public async Task<ParcelaFinanceiraDto> AtualizarParcelaAsync(Guid id, AtualizarParcelaCommand command)
    {
        var parcela = await _parcelaRepository.GetById(id);
        if (parcela is null)
            throw new KeyNotFoundException($"Parcela com Id {id} não encontrada.");

        parcela.Descricao = command.Descricao;
        parcela.Valor = command.Valor;
        parcela.DataVencimento = command.DataVencimento;
        parcela.Status = command.Status;
        parcela.DataPagamento = command.DataPagamento;
        parcela.FormaPagamento = command.FormaPagamento;
        parcela.Observacoes = command.Observacoes;
        parcela.ComprovanteUrl = command.ComprovanteUrl;
        parcela.AtualizadoEm = DateTime.UtcNow;

        await _parcelaRepository.Update(parcela);
        await _unitOfWork.Commit();

        return MapearParaDto(parcela);
    }

    public async Task<ParcelaFinanceiraDto> DarBaixaParcelaAsync(Guid id, DarBaixaParcelaCommand command)
    {
        var parcela = await _parcelaRepository.GetById(id);
        if (parcela is null)
            throw new KeyNotFoundException($"Parcela com Id {id} não encontrada.");

        parcela.Status = StatusParcela.Pago;
        parcela.DataPagamento = command.DataPagamento;
        parcela.FormaPagamento = command.FormaPagamento;
        if (!string.IsNullOrWhiteSpace(command.Observacoes))
            parcela.Observacoes = command.Observacoes;
        if (!string.IsNullOrWhiteSpace(command.ComprovanteUrl))
            parcela.ComprovanteUrl = command.ComprovanteUrl;

        parcela.AtualizadoEm = DateTime.UtcNow;

        await _parcelaRepository.Update(parcela);
        await _unitOfWork.Commit();

        _logger.LogInformation("Baixa realizada com sucesso na parcela {ParcelaId}", id);
        return MapearParaDto(parcela);
    }

    public async Task ExcluirParcelaAsync(Guid id)
    {
        var parcela = await _parcelaRepository.GetById(id);
        if (parcela is null)
            throw new KeyNotFoundException($"Parcela com Id {id} não encontrada.");

        await _parcelaRepository.Delete(id);
        await _unitOfWork.Commit();
    }

    public async Task<IEnumerable<DespesaProjetoDto>> ObterDespesasAsync(Guid? projetoId, CategoriaDespesa? categoria, DateTime? inicio, DateTime? fim)
    {
        var despesas = await _despesaRepository.ObterComFiltroAsync(projetoId, categoria, inicio, fim);
        return despesas.Select(MapearDespesaDto);
    }

    public async Task<DespesaProjetoDto?> ObterDespesaPorIdAsync(Guid id)
    {
        var despesa = await _despesaRepository.GetById(id);
        return despesa is null ? null : MapearDespesaDto(despesa);
    }

    public async Task<DespesaProjetoDto> CriarDespesaAsync(CriarDespesaCommand command)
    {
        var projeto = await _projetoRepository.GetById(command.ProjetoId);
        if (projeto is null)
            throw new KeyNotFoundException($"Projeto com Id {command.ProjetoId} não encontrado.");

        var despesa = new DespesaProjeto
        {
            Id = Guid.NewGuid(),
            ProjetoId = command.ProjetoId,
            Descricao = command.Descricao,
            Valor = command.Valor,
            DataDespesa = command.DataDespesa,
            Categoria = command.Categoria,
            Observacoes = command.Observacoes,
            ComprovanteUrl = command.ComprovanteUrl,
            CriadoEm = DateTime.UtcNow,
            Projeto = projeto
        };

        await _despesaRepository.Create(despesa);
        await _unitOfWork.Commit();

        return MapearDespesaDto(despesa);
    }

    public async Task<DespesaProjetoDto> AtualizarDespesaAsync(Guid id, AtualizarDespesaCommand command)
    {
        var despesa = await _despesaRepository.GetById(id);
        if (despesa is null)
            throw new KeyNotFoundException($"Despesa com Id {id} não encontrada.");

        despesa.Descricao = command.Descricao;
        despesa.Valor = command.Valor;
        despesa.DataDespesa = command.DataDespesa;
        despesa.Categoria = command.Categoria;
        despesa.Observacoes = command.Observacoes;
        despesa.ComprovanteUrl = command.ComprovanteUrl;
        despesa.AtualizadoEm = DateTime.UtcNow;

        await _despesaRepository.Update(despesa);
        await _unitOfWork.Commit();

        return MapearDespesaDto(despesa);
    }

    public async Task ExcluirDespesaAsync(Guid id)
    {
        var despesa = await _despesaRepository.GetById(id);
        if (despesa is null)
            throw new KeyNotFoundException($"Despesa com Id {id} não encontrada.");

        await _despesaRepository.Delete(id);
        await _unitOfWork.Commit();
    }

    public async Task<IEnumerable<AlertaFinanceiroDto>> ObterAlertasAsync()
    {
        var parcelas = (await _parcelaRepository.ObterAlertasVencimentoAsync(7)).ToList();
        var hoje = DateTime.UtcNow.Date;
        AtualizarStatusParcelasVencidas(parcelas, hoje);

        return GerarAlertasFinanceiros(parcelas, hoje);
    }

    #region Private Static Helper Methods (Clean Code & Low Cognitive Complexity)

    private static void AtualizarStatusParcelasVencidas(IEnumerable<ParcelaFinanceira> parcelas, DateTime hoje)
    {
        foreach (var parcela in parcelas)
        {
            if (parcela.Status == StatusParcela.Pendente && parcela.DataVencimento.Date < hoje)
            {
                parcela.Status = StatusParcela.Atrasado;
            }
        }
    }

    private static decimal CalcularVariacaoMensal(List<ParcelaFinanceira> parcelas, DateTime hoje)
    {
        var mesAtual = hoje.Month;
        var anoAtual = hoje.Year;
        var mesAnterior = mesAtual == 1 ? 12 : mesAtual - 1;
        var anoAnterior = mesAtual == 1 ? anoAtual - 1 : anoAtual;

        var recebidoMesAtual = parcelas
            .Where(p => p.Status == StatusParcela.Pago && p.DataPagamento.HasValue &&
                        p.DataPagamento.Value.Month == mesAtual && p.DataPagamento.Value.Year == anoAtual)
            .Sum(p => p.Valor);

        var recebidoMesAnterior = parcelas
            .Where(p => p.Status == StatusParcela.Pago && p.DataPagamento.HasValue &&
                        p.DataPagamento.Value.Month == mesAnterior && p.DataPagamento.Value.Year == anoAnterior)
            .Sum(p => p.Valor);

        if (recebidoMesAnterior == 0)
            return recebidoMesAtual > 0 ? 100m : 0m;

        return Math.Round(((recebidoMesAtual - recebidoMesAnterior) / recebidoMesAnterior) * 100m, 1);
    }

    private static List<ReceitaMesDto> GerarReceitasPorMes(List<ParcelaFinanceira> parcelas, List<DespesaProjeto> despesas, int ano)
    {
        var lista = new List<ReceitaMesDto>();
        var culture = new CultureInfo("pt-BR");

        for (var mes = 1; mes <= 12; mes++)
        {
            var nomeMes = culture.DateTimeFormat.GetAbbreviatedMonthName(mes).ToUpperInvariant().TrimEnd('.');

            var recebido = parcelas
                .Where(p => p.Status == StatusParcela.Pago && p.DataPagamento.HasValue &&
                            p.DataPagamento.Value.Month == mes && p.DataPagamento.Value.Year == ano)
                .Sum(p => p.Valor);

            var previsto = parcelas
                .Where(p => p.Status != StatusParcela.Cancelado &&
                            p.DataVencimento.Month == mes && p.DataVencimento.Year == ano)
                .Sum(p => p.Valor);

            var despesaMes = despesas
                .Where(d => d.DataDespesa.Month == mes && d.DataDespesa.Year == ano)
                .Sum(d => d.Valor);

            lista.Add(new ReceitaMesDto(nomeMes, mes, ano, recebido, previsto, despesaMes));
        }

        return lista;
    }

    private static List<AlertaFinanceiroDto> GerarAlertasFinanceiros(List<ParcelaFinanceira> parcelas, DateTime hoje)
    {
        var alertas = new List<AlertaFinanceiroDto>();

        var pendentesOuAtrasadas = parcelas
            .Where(p => p.Status == StatusParcela.Atrasado || (p.Status == StatusParcela.Pendente && p.DataVencimento.Date <= hoje.AddDays(7)))
            .OrderBy(p => p.DataVencimento)
            .ToList();

        foreach (var p in pendentesOuAtrasadas)
        {
            var dias = (p.DataVencimento.Date - hoje).Days;
            var emAtraso = dias < 0;
            var subtitulo = FormatarSubtituloAlerta(dias);

            alertas.Add(new AlertaFinanceiroDto(
                p.Id,
                p.ProjetoId,
                $"{p.Projeto?.Nome ?? "Projeto"} - Parcela {p.NumeroParcela}/{p.TotalParcelas}",
                subtitulo,
                p.Valor,
                p.DataVencimento,
                p.Status,
                Math.Abs(dias),
                emAtraso
            ));
        }

        return alertas;
    }

    private static string FormatarSubtituloAlerta(int dias)
    {
        if (dias < 0)
        {
            var abs = Math.Abs(dias);
            return abs == 1 ? "Atrasado há 1 dia" : $"Atrasado há {abs} dias";
        }

        if (dias == 0) return "Vence hoje";
        if (dias == 1) return "Vence amanhã";
        return $"Vence em {dias} dias";
    }

    private static List<ParcelaFinanceira> GerarParcelasContrato(Guid contratoId, CriarContratoCommand command)
    {
        var parcelas = new List<ParcelaFinanceira>();
        var valorPorParcela = Math.Round(command.ValorTotal / command.NumeroParcelas, 2);
        var diferencaCentavos = command.ValorTotal - (valorPorParcela * command.NumeroParcelas);
        var dataAtual = command.DataPrimeiroVencimento;

        for (var i = 1; i <= command.NumeroParcelas; i++)
        {
            var valorFinal = (i == command.NumeroParcelas) ? (valorPorParcela + diferencaCentavos) : valorPorParcela;

            parcelas.Add(new ParcelaFinanceira
            {
                Id = Guid.NewGuid(),
                ProjetoId = command.ProjetoId,
                ContratoFinanceiroId = contratoId,
                NumeroParcela = i,
                TotalParcelas = command.NumeroParcelas,
                Descricao = $"Parcela {i}/{command.NumeroParcelas}",
                Valor = valorFinal,
                DataVencimento = dataAtual,
                Status = dataAtual.Date < DateTime.UtcNow.Date ? StatusParcela.Atrasado : StatusParcela.Pendente,
                CriadoEm = DateTime.UtcNow
            });

            dataAtual = dataAtual.AddDays(command.IntervaloDias > 0 ? command.IntervaloDias : 30);
        }

        return parcelas;
    }

    private static ParcelaFinanceiraDto MapearParaDto(ParcelaFinanceira p)
    {
        return new ParcelaFinanceiraDto(
            p.Id,
            p.ProjetoId,
            p.Projeto?.Nome ?? string.Empty,
            p.Projeto?.ClienteId,
            null,
            p.ContratoFinanceiroId,
            p.NumeroParcela,
            p.TotalParcelas,
            p.Descricao,
            p.Valor,
            p.DataVencimento,
            p.DataPagamento,
            p.Status,
            p.Status.ToString(),
            p.FormaPagamento,
            p.FormaPagamento?.ToString(),
            p.Observacoes,
            p.ComprovanteUrl,
            p.CriadoEm
        );
    }

    private static ContratoFinanceiroDto MapearContratoDto(ContratoFinanceiro c, string projetoNome)
    {
        return new ContratoFinanceiroDto(
            c.Id,
            c.ProjetoId,
            projetoNome,
            c.ValorTotal,
            c.CondicoesPagamento,
            c.Observacoes,
            c.CriadoEm,
            c.Parcelas.Select(MapearParaDto).ToList()
        );
    }

    private static DespesaProjetoDto MapearDespesaDto(DespesaProjeto d)
    {
        return new DespesaProjetoDto(
            d.Id,
            d.ProjetoId,
            d.Projeto?.Nome ?? string.Empty,
            d.Descricao,
            d.Valor,
            d.DataDespesa,
            d.Categoria,
            d.Categoria.ToString(),
            d.Observacoes,
            d.ComprovanteUrl,
            d.CriadoEm
        );
    }

    public async Task<ComprovanteUploadResultDto> UploadComprovanteAsync(UploadComprovanteCommand command)
    {
        if (command.File == null || command.File.Length == 0)
        {
            throw new ArgumentException("Nenhum arquivo enviado.");
        }

        if (_storageService == null)
        {
            throw new InvalidOperationException("Serviço de armazenamento não configurado.");
        }

        using var stream = command.File.OpenReadStream();
        var url = await _storageService.UploadAsync(stream, command.File.FileName, command.File.ContentType);
        return new ComprovanteUploadResultDto(url, command.File.FileName);
    }

    #endregion
}
