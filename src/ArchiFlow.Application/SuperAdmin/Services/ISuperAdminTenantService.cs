using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Application.SuperAdmin.DTOs;

namespace ArchiFlow.Application.SuperAdmin.Services;

public interface ISuperAdminTenantService
{
    Task<SuperAdminDashboardKpisDto> ObterKpisAsync();
    Task<IEnumerable<TenantAssinaturaDto>> ListarEscritoriosAsync(string? termoBusca = null, string? statusFiltro = null);
    Task<TenantAssinaturaDto?> ObterPorIdAsync(Guid id);
    Task<TenantAssinaturaDto> CriarEscritorioAsync(CriarTenantAssinaturaRequestDto dto);
    Task<TenantAssinaturaDto> AtualizarStatusFinanceiroAsync(Guid id, AtualizarStatusFinanceiroDto dto);
    Task<TenantAssinaturaDto> AlternarBloqueioAcessoAsync(Guid id, BloquearTenantRequestDto dto);
    Task<TenantAssinaturaDto> AtualizarNotasInternasAsync(Guid id, AtualizarNotasRequestDto dto);
    Task<TenantAssinaturaDto> RegistrarPagamentoAsync(Guid id);
    Task<(bool Bloqueado, string? Motivo)> VerificarStatusAcessoAsync(string subdominio);
}
