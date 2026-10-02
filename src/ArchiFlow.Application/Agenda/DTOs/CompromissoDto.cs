using System;

namespace ArchiFlow.Application.Agenda.DTOs;

public record CompromissoDto(
    Guid Id,
    Guid EscritorioId,
    Guid? UsuarioId,
    string? NomeUsuario,
    Guid? ProjetoId,
    string? NomeProjeto,
    Guid? ClienteId,
    string? NomeCliente,
    string Titulo,
    string? Descricao,
    string Tipo,
    string Status,
    DateTime DataHoraInicio,
    DateTime DataHoraFim,
    string? Local,
    string? LinkGoogleMeet,
    string? GoogleEventId,
    string LinkGoogleCalendarWeb,
    DateTime CriadoEm,
    DateTime? AtualizadoEm,
    Guid? LeadId = null,
    string? NomeLead = null
);
