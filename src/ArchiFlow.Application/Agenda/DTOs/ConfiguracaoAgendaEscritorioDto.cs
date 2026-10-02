using System;

namespace ArchiFlow.Application.Agenda.DTOs;

public class ConfiguracaoAgendaEscritorioDto
{
    public Guid Id { get; set; }
    public Guid EscritorioId { get; set; }
    public string EmailAgendaEmpresa { get; set; } = string.Empty;
    public string? GoogleCalendarId { get; set; }
    public string? ChaveGoogleServiceAccountJson { get; set; }
    public bool PossuiChaveServiceAccount { get; set; }
    public string? GoogleOAuthEmail { get; set; }
    public bool PossuiOAuthConectado { get; set; }
    public string? GoogleClientId { get; set; }
    public string TipoIntegracao { get; set; } = "ServiceAccount"; // "ServiceAccount", "OAuth", "Nenhum"
    public string NomeAgenda { get; set; } = string.Empty;
    public bool SincronizacaoAutomaticaAtiva { get; set; }
    public string LinkEmbedGoogleCalendar { get; set; } = string.Empty;
    public DateTime ConectadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}
