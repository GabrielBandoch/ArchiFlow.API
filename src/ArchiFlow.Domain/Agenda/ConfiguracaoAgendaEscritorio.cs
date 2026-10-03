using System;

namespace ArchiFlow.Domain.Agenda;

public class ConfiguracaoAgendaEscritorio
{
    public Guid Id { get; set; }
    public Guid EscritorioId { get; set; }
    public string EmailAgendaEmpresa { get; set; } = string.Empty;
    public string? GoogleCalendarId { get; set; }
    public string? ChaveGoogleServiceAccountJson { get; set; }
    public string? GoogleOAuthRefreshToken { get; set; }
    public string? GoogleOAuthEmail { get; set; }
    public string? GoogleClientId { get; set; }
    public string? GoogleClientSecret { get; set; }
    public string TipoIntegracao { get; set; } = "ServiceAccount"; // "ServiceAccount", "OAuth", "Nenhum"
    public string NomeAgenda { get; set; } = "Agenda Oficial do Escritório";
    public bool SincronizacaoAutomaticaAtiva { get; set; } = true;
    public DateTime ConectadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }
}
