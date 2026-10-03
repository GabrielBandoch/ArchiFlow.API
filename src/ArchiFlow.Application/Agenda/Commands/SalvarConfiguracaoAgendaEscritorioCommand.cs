using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Agenda.Commands;

public class SalvarConfiguracaoAgendaEscritorioCommand
{
    public string? EmailAgendaEmpresa { get; set; }

    public string? GoogleCalendarId { get; set; }

    public string? ChaveGoogleServiceAccountJson { get; set; }

    public string? GoogleClientId { get; set; }

    public string? GoogleClientSecret { get; set; }

    public string? TipoIntegracao { get; set; }

    [MaxLength(200, ErrorMessage = "O nome da agenda não pode exceder 200 caracteres.")]
    public string NomeAgenda { get; set; } = "Agenda Oficial do Escritório";

    public bool SincronizacaoAutomaticaAtiva { get; set; } = true;
}
