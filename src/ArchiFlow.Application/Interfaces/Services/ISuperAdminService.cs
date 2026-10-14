using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Application.SuperAdmin.DTOs;

namespace ArchiFlow.Application.Interfaces.Services;

public interface ISuperAdminService
{
    Task<SuperAdminDashboardKpisDto> ObterKpisAsync();
    Task<IEnumerable<TenantAssinaturaDto>> ListarEscritoriosAsync(string? busca = null, string? status = null);
    Task<TenantAssinaturaDto?> ObterEscritorioPorIdAsync(Guid id);
    Task<TenantAssinaturaDto> CriarEscritorioAsync(CriarTenantRequestDto request);
    Task<TenantAssinaturaDto?> AtualizarStatusFinanceiroAsync(Guid id, AtualizarStatusFinanceiroRequestDto request);
    Task<TenantAssinaturaDto?> AlternarBloqueioAsync(Guid id, BloquearTenantRequestDto request);
    Task<TenantAssinaturaDto?> AtualizarNotasAsync(Guid id, AtualizarNotasRequestDto request);
    Task<TenantAssinaturaDto?> RegistrarPagamentoAsync(Guid id);
}
