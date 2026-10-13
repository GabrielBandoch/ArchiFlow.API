using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArchiFlow.Application.SuperAdmin.DTOs;
using ArchiFlow.Domain.Assinaturas;
using ArchiFlow.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace ArchiFlow.Application.SuperAdmin.Services;

public class SuperAdminTenantService : ISuperAdminTenantService
{
    private readonly ITenantAssinaturaRepository _repository;
    private readonly ITenantDatabaseProvisioner _provisioner;
    private readonly ILogger<SuperAdminTenantService> _logger;

    public SuperAdminTenantService(
        ITenantAssinaturaRepository repository,
        ITenantDatabaseProvisioner provisioner,
        ILogger<SuperAdminTenantService> logger)
    {
        _repository = repository;
        _provisioner = provisioner;
        _logger = logger;
    }

    public async Task<SuperAdminDashboardKpisDto> ObterKpisAsync()
    {
        var todos = (await _repository.ListarTodosAsync()).ToList();

        var total = todos.Count;
        var adimplentes = todos.Count(t => t.StatusFinanceiro.Equals("Adimplente", StringComparison.OrdinalIgnoreCase));
        var emDebito = todos.Count(t => t.StatusFinanceiro.Equals("EmDebito", StringComparison.OrdinalIgnoreCase));
        var suspensos = todos.Count(t => t.StatusFinanceiro.Equals("Suspenso", StringComparison.OrdinalIgnoreCase) || t.BloqueadoPorInadimplencia);
        var trial = todos.Count(t => t.StatusFinanceiro.Equals("Trial", StringComparison.OrdinalIgnoreCase));

        // MRR considera clientes ativos (Adimplente, EmDebito, Trial pagos futuramente)
        var mrr = todos
            .Where(t => !t.StatusFinanceiro.Equals("Suspenso", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.ValorMensalidade);

        var totalEmAtrasoReais = todos
            .Where(t => t.StatusFinanceiro.Equals("EmDebito", StringComparison.OrdinalIgnoreCase) || t.StatusFinanceiro.Equals("Suspenso", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.ValorMensalidade);

        var taxaAdimplencia = total > 0 ? Math.Round((decimal)adimplentes / total * 100m, 1) : 100m;

        return new SuperAdminDashboardKpisDto
        {
            TotalEscritorios = total,
            TotalAdimplentes = adimplentes,
            TotalEmDebito = emDebito,
            TotalSuspensos = suspensos,
            TotalTrial = trial,
            MrrTotal = mrr,
            TotalEmAtrasoReais = totalEmAtrasoReais,
            TaxaAdimplenciaPercentual = taxaAdimplencia
        };
    }

    public async Task<IEnumerable<TenantAssinaturaDto>> ListarEscritoriosAsync(string? termoBusca = null, string? statusFiltro = null)
    {
        var entidades = await _repository.ListarTodosAsync(termoBusca, statusFiltro);
        return entidades.Select(MapearParaDto);
    }

    public async Task<TenantAssinaturaDto?> ObterPorIdAsync(Guid id)
    {
        var entidade = await _repository.ObterPorIdAsync(id);
        return entidade != null ? MapearParaDto(entidade) : null;
    }

    public async Task<TenantAssinaturaDto> CriarEscritorioAsync(CriarTenantAssinaturaRequestDto dto)
    {
        var cleanSubdominio = Regex.Replace(dto.Subdominio.Trim().ToLowerInvariant(), @"[^a-z0-9-]", "");
        if (string.IsNullOrWhiteSpace(cleanSubdominio))
        {
            throw new ArgumentException("Subdomínio inválido.");
        }

        if (await _repository.SubdominioExisteAsync(cleanSubdominio))
        {
            throw new InvalidOperationException($"O subdomínio '{cleanSubdominio}' já está em uso por outro escritório.");
        }

        var entidade = new TenantAssinatura
        {
            Id = Guid.NewGuid(),
            Subdominio = cleanSubdominio,
            NomeEscritorio = dto.NomeEscritorio.Trim(),
            RazaoSocial = dto.RazaoSocial?.Trim(),
            CnpjCpf = dto.CnpjCpf?.Trim(),
            NomeResponsavel = dto.NomeResponsavel.Trim(),
            EmailResponsavel = dto.EmailResponsavel.Trim().ToLowerInvariant(),
            TelefoneResponsavel = dto.TelefoneResponsavel?.Trim(),
            Plano = dto.Plano.Trim(),
            CicloFaturamento = dto.CicloFaturamento.Trim(),
            ValorMensalidade = dto.ValorMensalidade,
            StatusFinanceiro = dto.StatusFinanceiro.Trim(),
            DataVencimento = dto.DataVencimento ?? DateTime.UtcNow.AddDays(30),
            UltimoPagamento = dto.StatusFinanceiro.Equals("Adimplente", StringComparison.OrdinalIgnoreCase) ? DateTime.UtcNow : null,
            BloqueadoPorInadimplencia = false,
            NotasInternas = dto.NotasInternas?.Trim(),
            CriadoEm = DateTime.UtcNow
        };

        await _repository.AdicionarAsync(entidade);
        _logger.LogInformation("Novo escritório assinante cadastrado: '{Nome}' ({Subdominio})", entidade.NomeEscritorio, entidade.Subdominio);

        // Provisiona o banco de dados dedicado do tenant
        try
        {
            _logger.LogInformation("Iniciando provisionamento automático do banco para '{Subdominio}'...", cleanSubdominio);
            await _provisioner.EnsureTenantDatabaseAsync(cleanSubdominio);
            _logger.LogInformation("Banco para '{Subdominio}' provisionado com sucesso.", cleanSubdominio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aviso: Banco de dados para '{Subdominio}' será provisionado na primeira requisição.", cleanSubdominio);
        }

        return MapearParaDto(entidade);
    }

    public async Task<TenantAssinaturaDto> AtualizarStatusFinanceiroAsync(Guid id, AtualizarStatusFinanceiroDto dto)
    {
        var entidade = await _repository.ObterPorIdAsync(id);
        if (entidade == null)
        {
            throw new KeyNotFoundException("Escritório assinante não encontrado.");
        }

        entidade.StatusFinanceiro = dto.StatusFinanceiro.Trim();

        if (dto.RegistrarPagamentoHoje || entidade.StatusFinanceiro.Equals("Adimplente", StringComparison.OrdinalIgnoreCase))
        {
            entidade.UltimoPagamento = DateTime.UtcNow;
            entidade.DataVencimento = entidade.CicloFaturamento.Equals("Anual", StringComparison.OrdinalIgnoreCase)
                ? DateTime.UtcNow.AddYears(1)
                : DateTime.UtcNow.AddMonths(1);

            // Desbloqueia automaticamente ao registrar pagamento
            entidade.BloqueadoPorInadimplencia = false;
            entidade.MotivoBloqueio = null;
        }
        else if (dto.NovaDataVencimento.HasValue)
        {
            entidade.DataVencimento = dto.NovaDataVencimento.Value;
        }

        if (entidade.StatusFinanceiro.Equals("Suspenso", StringComparison.OrdinalIgnoreCase))
        {
            entidade.BloqueadoPorInadimplencia = true;
            if (string.IsNullOrWhiteSpace(entidade.MotivoBloqueio))
            {
                entidade.MotivoBloqueio = "Suspensão por inadimplência da assinatura COTU.";
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.NotasInternas))
        {
            entidade.NotasInternas = dto.NotasInternas;
        }

        await _repository.AtualizarAsync(entidade);
        return MapearParaDto(entidade);
    }

    public async Task<TenantAssinaturaDto> AlternarBloqueioAcessoAsync(Guid id, BloquearTenantRequestDto dto)
    {
        var entidade = await _repository.ObterPorIdAsync(id);
        if (entidade == null)
        {
            throw new KeyNotFoundException("Escritório assinante não encontrado.");
        }

        entidade.BloqueadoPorInadimplencia = dto.Bloquear;
        entidade.MotivoBloqueio = dto.Bloquear
            ? (dto.Motivo ?? "Acesso suspenso por pendência financeira na assinatura COTU.")
            : null;

        if (dto.Bloquear)
        {
            entidade.StatusFinanceiro = "Suspenso";
        }
        else if (entidade.StatusFinanceiro.Equals("Suspenso", StringComparison.OrdinalIgnoreCase))
        {
            entidade.StatusFinanceiro = "Adimplente";
        }

        await _repository.AtualizarAsync(entidade);
        return MapearParaDto(entidade);
    }

    public async Task<TenantAssinaturaDto> AtualizarNotasInternasAsync(Guid id, AtualizarNotasRequestDto dto)
    {
        var entidade = await _repository.ObterPorIdAsync(id);
        if (entidade == null)
        {
            throw new KeyNotFoundException("Escritório assinante não encontrado.");
        }

        entidade.NotasInternas = dto.NotasInternas;
        await _repository.AtualizarAsync(entidade);
        return MapearParaDto(entidade);
    }

    public async Task<TenantAssinaturaDto> RegistrarPagamentoAsync(Guid id)
    {
        var entidade = await _repository.ObterPorIdAsync(id);
        if (entidade == null)
        {
            throw new KeyNotFoundException("Escritório assinante não encontrado.");
        }

        entidade.StatusFinanceiro = "Adimplente";
        entidade.UltimoPagamento = DateTime.UtcNow;
        entidade.DataVencimento = entidade.CicloFaturamento.Equals("Anual", StringComparison.OrdinalIgnoreCase)
            ? DateTime.UtcNow.AddYears(1)
            : DateTime.UtcNow.AddMonths(1);
        entidade.BloqueadoPorInadimplencia = false;
        entidade.MotivoBloqueio = null;

        await _repository.AtualizarAsync(entidade);
        return MapearParaDto(entidade);
    }

    public async Task<(bool Bloqueado, string? Motivo)> VerificarStatusAcessoAsync(string subdominio)
    {
        if (string.IsNullOrWhiteSpace(subdominio))
        {
            return (false, null);
        }

        var clean = subdominio.Trim().ToLowerInvariant();
        if (clean == "default" || clean == "app" || clean == "staging" || clean == "cotu")
        {
            return (false, null);
        }

        var entidade = await _repository.ObterPorSubdominioAsync(clean);
        if (entidade == null)
        {
            return (false, null);
        }

        return (entidade.BloqueadoPorInadimplencia, entidade.MotivoBloqueio);
    }

    private static TenantAssinaturaDto MapearParaDto(TenantAssinatura t)
    {
        var diasAtraso = t.CalcularDiasEmAtraso();
        var linkAcesso = $"https://{t.Subdominio}.cotu.com.br";
        var whatsApp = GerarLinkWhatsAppCobranca(t, diasAtraso);

        return new TenantAssinaturaDto
        {
            Id = t.Id,
            Subdominio = t.Subdominio,
            NomeEscritorio = t.NomeEscritorio,
            RazaoSocial = t.RazaoSocial,
            CnpjCpf = t.CnpjCpf,
            NomeResponsavel = t.NomeResponsavel,
            EmailResponsavel = t.EmailResponsavel,
            TelefoneResponsavel = t.TelefoneResponsavel,
            Plano = t.Plano,
            CicloFaturamento = t.CicloFaturamento,
            ValorMensalidade = t.ValorMensalidade,
            StatusFinanceiro = t.StatusFinanceiro,
            DataVencimento = t.DataVencimento,
            UltimoPagamento = t.UltimoPagamento,
            BloqueadoPorInadimplencia = t.BloqueadoPorInadimplencia,
            MotivoBloqueio = t.MotivoBloqueio,
            NotasInternas = t.NotasInternas,
            DiasEmAtraso = diasAtraso,
            LinkAcesso = linkAcesso,
            LinkWhatsAppCobranca = whatsApp,
            CriadoEm = t.CriadoEm,
            AtualizadoEm = t.AtualizadoEm
        };
    }

    private static string GerarLinkWhatsAppCobranca(TenantAssinatura t, int diasAtraso)
    {
        if (string.IsNullOrWhiteSpace(t.TelefoneResponsavel))
        {
            return string.Empty;
        }

        var numerosApenas = Regex.Replace(t.TelefoneResponsavel, @"\D", "");
        if (numerosApenas.Length < 10)
        {
            return string.Empty;
        }

        if (!numerosApenas.StartsWith("55") && (numerosApenas.Length == 10 || numerosApenas.Length == 11))
        {
            numerosApenas = "55" + numerosApenas;
        }

        string mensagem;
        if (t.BloqueadoPorInadimplencia)
        {
            mensagem = $"Olá {t.NomeResponsavel}! Aqui é da equipe da COTU. Notamos que a assinatura do escritório {t.NomeEscritorio} está temporariamente suspensa por pendência financeira (R$ {t.ValorMensalidade:N2}). Segue nosso canal para regularizar seu acesso e reativar a plataforma imediatamente.";
        }
        else if (diasAtraso > 0)
        {
            mensagem = $"Olá {t.NomeResponsavel}! Tudo bem? Passando para lembrar da mensalidade da COTU para o escritório {t.NomeEscritorio} (R$ {t.ValorMensalidade:N2}), com vencimento em {t.DataVencimento:dd/MM/yyyy} ({diasAtraso} dias em aberto). Precisando de 2ª via ou código Pix, estamos à disposição!";
        }
        else
        {
            mensagem = $"Olá {t.NomeResponsavel}! Aqui é da equipe de suporte COTU. Como estão as operações no {t.NomeEscritorio}? Qualquer dúvida sobre o sistema, estamos por aqui!";
        }

        return $"https://wa.me/{numerosApenas}?text={WebUtility.UrlEncode(mensagem)}";
    }
}
