using System;

namespace ArchiFlow.Application.SuperAdmin.DTOs;

public class SuperAdminDashboardKpisDto
{
    public int TotalEscritorios { get; set; }
    public int TotalAtivos { get; set; }
    public int TotalInadimplentes { get; set; }
    public int TotalBloqueados { get; set; }
    public decimal MrrTotal { get; set; }
    public decimal ValorTotalEmAtraso { get; set; }
}

public class TenantAssinaturaDto
{
    public Guid Id { get; set; }
    public string Subdominio { get; set; } = string.Empty;
    public string NomeEscritorio { get; set; } = string.Empty;
    public string Plano { get; set; } = string.Empty;
    public string StatusFinanceiro { get; set; } = string.Empty;
    public bool AcessoBloqueado { get; set; }
    public string? MotivoBloqueio { get; set; }
    public string? BloqueadoEm { get; set; }
    public decimal ValorMensalidade { get; set; }
    public string ProximoVencimento { get; set; } = string.Empty;
    public int DiasAtraso { get; set; }
    public string EmailContato { get; set; } = string.Empty;
    public string? TelefoneContato { get; set; }
    public string? AnotacoesInternas { get; set; }
    public string CriadoEm { get; set; } = string.Empty;
    public string? AtualizadoEm { get; set; }
}

public class CriarTenantRequestDto
{
    public string NomeEscritorio { get; set; } = string.Empty;
    public string Subdominio { get; set; } = string.Empty;
    public string EmailAdministrador { get; set; } = string.Empty;
    public string SenhaInicial { get; set; } = string.Empty;
    public string Plano { get; set; } = "Professional";
    public decimal ValorMensalidade { get; set; }
    public string? TelefoneContato { get; set; }
    public string? AnotacoesInternas { get; set; }
}

public class AtualizarStatusFinanceiroRequestDto
{
    public string NovoStatus { get; set; } = string.Empty;
    public string? Motivo { get; set; }
}

public class BloquearTenantRequestDto
{
    public bool Bloquear { get; set; }
    public string? Motivo { get; set; }
}

public class AtualizarNotasRequestDto
{
    public string AnotacoesInternas { get; set; } = string.Empty;
}
