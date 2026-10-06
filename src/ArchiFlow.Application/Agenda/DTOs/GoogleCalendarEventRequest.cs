using ArchiFlow.Domain.Agenda;

namespace ArchiFlow.Application.Agenda.DTOs;

public class GoogleCalendarEventRequest
{
    public Compromisso Compromisso { get; set; } = null!;
    public string CalendarId { get; set; } = "primary";
    public string? NomeProjeto { get; set; }
    public string? NomeCliente { get; set; }
    public string? NomeLead { get; set; }
    public bool SolicitarGoogleMeet { get; set; }
}

public class GoogleCalendarSyncResult
{
    public bool Sucesso { get; set; }
    public string? GoogleEventId { get; set; }
    public string? LinkGoogleMeet { get; set; }
}
