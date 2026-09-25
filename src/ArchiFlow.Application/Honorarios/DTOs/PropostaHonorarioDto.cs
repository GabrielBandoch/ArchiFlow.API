using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Projetos.Enum;
using System;
using System.Collections.Generic;

namespace ArchiFlow.Application.Honorarios.DTOs;

public record PropostaHonorarioDto(
    Guid Id,
    string Titulo,
    string Codigo,
    Guid? ClienteId,
    string? ClienteNome,
    Guid? LeadId,
    string? LeadNome,
    TipoProjeto TipoProjeto,
    string TipoProjetoNome,
    PadraoImovel PadraoImovel,
    string PadraoImovelNome,
    decimal MetragemQuadrada,
    decimal ValorHoraBase,
    decimal ValorMetroQuadradoBase,
    decimal HorasEstimadasTotal,
    decimal ValorBase,
    decimal ValorFatorPadrao,
    decimal ValorFatorTipologia,
    decimal ValorEscopo,
    decimal ValorTotalSugerido,
    decimal ValorFinalAjustado,
    StatusProposta Status,
    string StatusNome,
    string? Observacoes,
    DateTime CriadoEm,
    DateTime? AtualizadoEm,
    List<ItemPropostaEtapaDto> ItensEtapa
);
