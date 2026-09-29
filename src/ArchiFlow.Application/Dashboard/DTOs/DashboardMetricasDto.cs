using System;
using System.Collections.Generic;

namespace ArchiFlow.Application.Dashboard.DTOs;

public record DashboardKpisDto(
    int TotalProjetosAtivos,
    int TotalProjetosConcluidos,
    int TotalLeadsAtivos,
    int TotalLeadsConvertidos,
    decimal TaxaConversaoLeads,
    int TotalClientes,
    int TotalPropostas,
    decimal ValorTotalPropostas,
    decimal ValorMedioProposta
);

public record ProjetosPorStatusDto(
    string Status,
    string NomeStatus,
    int Quantidade,
    decimal Percentual
);

public record ProjetosPorTipoDto(
    string Tipo,
    string NomeTipo,
    int Quantidade,
    decimal Percentual
);

public record LeadsPorStatusDto(
    string Status,
    string NomeStatus,
    int Quantidade,
    decimal Percentual
);

public record LeadsPorOrigemDto(
    string Origem,
    int Quantidade,
    decimal Percentual
);

public record PropostasMensalDto(
    string MesAno,
    string RotuloMes,
    int Quantidade,
    decimal ValorTotal
);

public record ProjetoResumoDashboardDto(
    Guid Id,
    string Nome,
    string? ClienteNome,
    string Status,
    string Tipo,
    decimal? MetragemTotal,
    int TotalEtapas,
    int EtapasConcluidas,
    decimal ProgressoPercentual,
    DateTime? DataInicio,
    DateTime? DataPrevistaEntrega
);

public record LeadResumoDashboardDto(
    Guid Id,
    string Nome,
    string Email,
    string? Telefone,
    string Status,
    string? OrigemNome,
    DateTime CriadoEm
);

public record PropostaResumoDashboardDto(
    Guid Id,
    string Titulo,
    string Codigo,
    string? ClienteOuLeadNome,
    decimal MetragemQuadrada,
    decimal ValorFinal,
    string Status,
    DateTime CriadoEm
);

public record PreferenciaDashboardDto(
    Guid UsuarioId,
    string LayoutJson,
    DateTime AtualizadoEm
);

public record SalvarPreferenciaDashboardCommand(
    string LayoutJson
);

public record DashboardMetricasDto(
    DashboardKpisDto Kpis,
    IReadOnlyList<ProjetosPorStatusDto> ProjetosPorStatus,
    IReadOnlyList<ProjetosPorTipoDto> ProjetosPorTipo,
    IReadOnlyList<LeadsPorStatusDto> LeadsPorStatus,
    IReadOnlyList<LeadsPorOrigemDto> LeadsPorOrigem,
    IReadOnlyList<PropostasMensalDto> PropostasMensais,
    IReadOnlyList<ProjetoResumoDashboardDto> ProjetosRecentes,
    IReadOnlyList<LeadResumoDashboardDto> LeadsRecentes,
    IReadOnlyList<PropostaResumoDashboardDto> PropostasRecentes
);
