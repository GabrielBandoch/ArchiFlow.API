using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Domain.Agenda;

namespace ArchiFlow.Application.Interfaces.Services;

public class NomesRelacionadosBatch
{
    public Dictionary<Guid, string> Projetos { get; set; } = new();
    public Dictionary<Guid, string> Clientes { get; set; } = new();
    public Dictionary<Guid, string> Leads { get; set; } = new();
    public Dictionary<Guid, string> Usuarios { get; set; } = new();
}

public interface IAgendaValidationService
{
    Task ValidarEntidadesRelacionadasAsync(
        Guid escritorioId,
        Guid? usuarioId,
        Guid? projetoId,
        Guid? clienteId,
        Guid? leadId);

    Task<(string? NomeProjeto, string? NomeCliente, string? NomeLead, string? NomeUsuario)> ObterNomesRelacionadosAsync(
        Guid? projetoId,
        Guid? clienteId,
        Guid? leadId,
        Guid? usuarioId);

    Task<NomesRelacionadosBatch> ObterNomesEmLoteAsync(IEnumerable<Compromisso> compromissos);
}
