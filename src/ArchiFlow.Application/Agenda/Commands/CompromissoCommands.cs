using System;
using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Agenda.Commands;

public record CriarCompromissoCommand(
    [Required(ErrorMessage = "Título é obrigatório.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Título deve ter entre 3 e 200 caracteres.")]
    string Titulo,

    DateTime DataHoraInicio,

    DateTime DataHoraFim,

    string? Tipo,

    string? Descricao,

    string? Local,

    Guid? ProjetoId,

    Guid? ClienteId,

    Guid? UsuarioId,

    bool GerarGoogleMeet = false,

    Guid? LeadId = null
);

public record AtualizarCompromissoCommand(
    [Required(ErrorMessage = "Título é obrigatório.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Título deve ter entre 3 e 200 caracteres.")]
    string Titulo,

    DateTime DataHoraInicio,

    DateTime DataHoraFim,

    string? Tipo,

    string? Status,

    string? Descricao,

    string? Local,

    string? LinkGoogleMeet,

    Guid? ProjetoId,

    Guid? ClienteId,

    Guid? UsuarioId,

    Guid? LeadId = null
);

public record AlterarStatusCompromissoCommand(
    [Required(ErrorMessage = "Status é obrigatório.")]
    string Status
);
