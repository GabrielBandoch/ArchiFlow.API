using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.SuperAdmin.DTOs;
using ArchiFlow.Domain.MultiTenancy;
using ArchiFlow.Domain.Shared;

namespace ArchiFlow.Application.SuperAdmin.Services;

public class SuperAdminService : ISuperAdminService
{
    private readonly ITenantAssinaturaRepository _repository;
    private readonly ITenantDatabaseProvisioner _provisioner;

    public SuperAdminService(
        ITenantAssinaturaRepository repository,
        ITenantDatabaseProvisioner provisioner)
    {
        _repository = repository;
        _provisioner = provisioner;
    }

    public async Task<SuperAdminDashboardKpisDto> ObterKpisAsync()
    {
        var todos = (await _repository.ListarTodosAsync()).ToList();

        var totalEscritorios = todos.Count;
        var totalAtivos = todos.Count(t => !t.BloqueadoInadimplencia && string.Equals(t.StatusFinanceiro, "Adimplente", StringComparison.OrdinalIgnoreCase));
        var totalInadimplentes = todos.Count(t => string.Equals(t.StatusFinanceiro, "EmDebito", StringComparison.OrdinalIgnoreCase));
        var totalBloqueados = todos.Count(t => t.BloqueadoInadimplencia);
        var mrrTotal = todos.Where(t => !t.BloqueadoInadimplencia).Sum(t => t.ValorMensalidade);
        var valorTotalEmAtraso = todos.Where(t => string.Equals(t.StatusFinanceiro, "EmDebito", StringComparison.OrdinalIgnoreCase)).Sum(t => t.ValorMensalidade);

        return new SuperAdminDashboardKpisDto
        {
            TotalEscritorios = totalEscritorios,
            TotalAtivos = totalAtivos,
            TotalInadimplentes = totalInadimplentes,
            TotalBloqueados = totalBloqueados,
            MrrTotal = mrrTotal,
            ValorTotalEmAtraso = valorTotalEmAtraso
        };
    }

    public async Task<IEnumerable<TenantAssinaturaDto>> ListarEscritoriosAsync(string? busca = null, string? status = null)
    {
        var lista = await _repository.ListarTodosAsync(busca, status);
        return lista.Select(MapearParaDto);
    }

    public async Task<TenantAssinaturaDto?> ObterEscritorioPorIdAsync(Guid id)
    {
        var entity = await _repository.ObterPorIdAsync(id);
        return entity is null ? null : MapearParaDto(entity);
    }

    public async Task<TenantAssinaturaDto> CriarEscritorioAsync(CriarTenantRequestDto request)
    {
        var cleanSubdominio = request.Subdominio.Trim().ToLowerInvariant();

        var existente = await _repository.ObterPorSubdominioAsync(cleanSubdominio);
        if (existente != null)
        {
            throw new InvalidOperationException($"Já existe um escritório cadastrado com o subdomínio '{cleanSubdominio}'.");
        }

        var novo = new TenantAssinatura
        {
            Id = Guid.NewGuid(),
            Subdominio = cleanSubdominio,
            NomeEscritorio = request.NomeEscritorio.Trim(),
            EmailResponsavel = request.EmailAdministrador.Trim(),
            NomeResponsavel = request.NomeEscritorio.Trim(),
            TelefoneResponsavel = request.TelefoneContato?.Trim(),
            Plano = string.IsNullOrWhiteSpace(request.Plano) ? "Professional" : request.Plano.Trim(),
            ValorMensalidade = request.ValorMensalidade,
            StatusFinanceiro = "Trial",
            DataVencimento = DateTime.UtcNow.AddDays(14),
            NotasInternas = request.AnotacoesInternas?.Trim(),
            BloqueadoInadimplencia = false,
            CriadoEm = DateTime.UtcNow
        };

        await _repository.AdicionarAsync(novo);
        await _provisioner.EnsureTenantDatabaseAsync(cleanSubdominio);

        return MapearParaDto(novo);
    }

    public async Task<TenantAssinaturaDto?> AtualizarStatusFinanceiroAsync(Guid id, AtualizarStatusFinanceiroRequestDto request)
    {
        var entity = await _repository.ObterPorIdAsync(id);
        if (entity is null) return null;

        entity.StatusFinanceiro = request.NovoStatus;
        entity.AtualizadoEm = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Motivo))
        {
            entity.NotasInternas = string.IsNullOrWhiteSpace(entity.NotasInternas)
                ? $"[{DateTime.UtcNow:dd/MM/yyyy}] {request.Motivo}"
                : $"{entity.NotasInternas}\n[{DateTime.UtcNow:dd/MM/yyyy}] {request.Motivo}";
        }

        await _repository.AtualizarAsync(entity);
        return MapearParaDto(entity);
    }

    public async Task<TenantAssinaturaDto?> AlternarBloqueioAsync(Guid id, BloquearTenantRequestDto request)
    {
        var entity = await _repository.ObterPorIdAsync(id);
        if (entity is null) return null;

        entity.BloqueadoInadimplencia = request.Bloquear;
        entity.MotivoBloqueio = request.Bloquear ? request.Motivo : null;
        entity.AtualizadoEm = DateTime.UtcNow;

        await _repository.AtualizarAsync(entity);
        return MapearParaDto(entity);
    }

    public async Task<TenantAssinaturaDto?> AtualizarNotasAsync(Guid id, AtualizarNotasRequestDto request)
    {
        var entity = await _repository.ObterPorIdAsync(id);
        if (entity is null) return null;

        entity.NotasInternas = request.AnotacoesInternas;
        entity.AtualizadoEm = DateTime.UtcNow;

        await _repository.AtualizarAsync(entity);
        return MapearParaDto(entity);
    }

    public async Task<TenantAssinaturaDto?> RegistrarPagamentoAsync(Guid id)
    {
        var entity = await _repository.ObterPorIdAsync(id);
        if (entity is null) return null;

        entity.StatusFinanceiro = "Adimplente";
        entity.UltimoPagamento = DateTime.UtcNow;
        entity.DataVencimento = DateTime.UtcNow.AddMonths(1);
        entity.BloqueadoInadimplencia = false;
        entity.MotivoBloqueio = null;
        entity.AtualizadoEm = DateTime.UtcNow;

        await _repository.AtualizarAsync(entity);
        return MapearParaDto(entity);
    }

    private static TenantAssinaturaDto MapearParaDto(TenantAssinatura entity)
    {
        var diasAtraso = 0;
        if (string.Equals(entity.StatusFinanceiro, "EmDebito", StringComparison.OrdinalIgnoreCase) && entity.DataVencimento < DateTime.UtcNow)
        {
            diasAtraso = (int)(DateTime.UtcNow - entity.DataVencimento).TotalDays;
            if (diasAtraso < 0) diasAtraso = 0;
        }

        return new TenantAssinaturaDto
        {
            Id = entity.Id,
            Subdominio = entity.Subdominio,
            NomeEscritorio = entity.NomeEscritorio,
            Plano = entity.Plano,
            StatusFinanceiro = entity.StatusFinanceiro,
            AcessoBloqueado = entity.BloqueadoInadimplencia,
            MotivoBloqueio = entity.MotivoBloqueio,
            BloqueadoEm = entity.BloqueadoInadimplencia && entity.AtualizadoEm.HasValue
                ? entity.AtualizadoEm.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")
                : null,
            ValorMensalidade = entity.ValorMensalidade,
            ProximoVencimento = entity.DataVencimento.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            DiasAtraso = diasAtraso,
            EmailContato = entity.EmailResponsavel,
            TelefoneContato = entity.TelefoneResponsavel,
            AnotacoesInternas = entity.NotasInternas,
            CriadoEm = entity.CriadoEm.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            AtualizadoEm = entity.AtualizadoEm?.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }
}
