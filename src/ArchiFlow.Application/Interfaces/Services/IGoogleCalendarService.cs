using System;
using System.Collections.Generic;
using ArchiFlow.Domain.Agenda;

namespace ArchiFlow.Application.Interfaces.Services;

public interface IGoogleCalendarService
{
    string GerarLinkWebAdicionarEvento(Compromisso compromisso, string? nomeProjeto = null, string? nomeCliente = null);
    string GerarLinkGoogleMeet(string identificador);
    string ExportarIcs(IEnumerable<Compromisso> compromissos, string nomeCalendario = "ArchiFlow - Agenda");
}
