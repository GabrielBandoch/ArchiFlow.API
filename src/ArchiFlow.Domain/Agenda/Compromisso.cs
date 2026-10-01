using System;

namespace ArchiFlow.Domain.Agenda;

public static class TiposCompromisso
{
    public const string ReuniaoCliente = "ReuniaoCliente";
    public const string VisitaObra = "VisitaObra";
    public const string MedicaoTecnica = "MedicaoTecnica";
    public const string ApresentacaoProjeto = "ApresentacaoProjeto";
    public const string EntregaEtapa = "EntregaEtapa";
    public const string Geral = "Geral";

    public static readonly string[] Todos =
    {
        ReuniaoCliente,
        VisitaObra,
        MedicaoTecnica,
        ApresentacaoProjeto,
        EntregaEtapa,
        Geral
    };
}

public static class StatusCompromisso
{
    public const string Agendado = "Agendado";
    public const string Concluido = "Concluido";
    public const string Cancelado = "Cancelado";

    public static readonly string[] Todos =
    {
        Agendado,
        Concluido,
        Cancelado
    };
}

public class Compromisso
{
    public Guid Id { get; set; }
    public Guid EscritorioId { get; set; }
    public Guid? UsuarioId { get; set; }
    public Guid? ProjetoId { get; set; }
    public Guid? ClienteId { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string Tipo { get; set; } = TiposCompromisso.Geral;
    public string Status { get; set; } = StatusCompromisso.Agendado;

    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public string? Local { get; set; }
    public string? LinkGoogleMeet { get; set; }
    public string? GoogleEventId { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }
}
