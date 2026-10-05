using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Agenda;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Usuarios;

namespace ArchiFlow.Application.Agenda.Services;

public class AgendaValidationService : IAgendaValidationService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IProjetoRepository _projetoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ILeadRepository? _leadRepository;

    public AgendaValidationService(
        IUsuarioRepository usuarioRepository,
        IProjetoRepository projetoRepository,
        IClienteRepository clienteRepository,
        ILeadRepository? leadRepository = null)
    {
        _usuarioRepository = usuarioRepository;
        _projetoRepository = projetoRepository;
        _clienteRepository = clienteRepository;
        _leadRepository = leadRepository;
    }

    public async Task ValidarEntidadesRelacionadasAsync(
        Guid escritorioId,
        Guid? usuarioId,
        Guid? projetoId,
        Guid? clienteId,
        Guid? leadId)
    {
        if (usuarioId.HasValue && usuarioId.Value != Guid.Empty)
        {
            await ValidarUsuarioAsync(escritorioId, usuarioId.Value);
        }

        if (projetoId.HasValue && projetoId.Value != Guid.Empty)
        {
            await ValidarProjetoAsync(projetoId.Value);
        }

        if (clienteId.HasValue && clienteId.Value != Guid.Empty)
        {
            await ValidarClienteAsync(clienteId.Value);
        }

        if (leadId.HasValue && leadId.Value != Guid.Empty && _leadRepository is not null)
        {
            await ValidarLeadAsync(leadId.Value);
        }
    }

    private async Task ValidarUsuarioAsync(Guid escritorioId, Guid usuarioId)
    {
        var usuario = await _usuarioRepository.GetById(usuarioId);
        if (usuario is null)
            throw new InvalidOperationException($"Usuário responsável com ID '{usuarioId}' não foi encontrado.");

        var usuarioEscritorio = usuario.EscritorioId ?? usuario.Id;
        if (usuarioEscritorio != escritorioId)
            throw new UnauthorizedAccessException($"O usuário '{usuario.Nome}' pertence a outro escritório e não pode ser vinculado a este compromisso.");
    }

    private async Task ValidarProjetoAsync(Guid projetoId)
    {
        var projeto = await _projetoRepository.GetById(projetoId);
        if (projeto is null)
            throw new InvalidOperationException($"Projeto com ID '{projetoId}' não foi encontrado.");
    }

    private async Task ValidarClienteAsync(Guid clienteId)
    {
        var cliente = await _clienteRepository.GetById(clienteId);
        if (cliente is null)
            throw new InvalidOperationException($"Cliente com ID '{clienteId}' não foi encontrado.");
    }

    private async Task ValidarLeadAsync(Guid leadId)
    {
        var lead = await _leadRepository!.GetById(leadId);
        if (lead is null)
            throw new InvalidOperationException($"Lead com ID '{leadId}' não foi encontrado.");
    }

    public async Task<(string? NomeProjeto, string? NomeCliente, string? NomeLead, string? NomeUsuario)> ObterNomesRelacionadosAsync(
        Guid? projetoId,
        Guid? clienteId,
        Guid? leadId,
        Guid? usuarioId)
    {
        string? nomeProjeto = null;
        if (projetoId.HasValue && projetoId.Value != Guid.Empty)
        {
            var p = await _projetoRepository.GetById(projetoId.Value);
            nomeProjeto = p?.Nome;
        }

        string? nomeCliente = null;
        if (clienteId.HasValue && clienteId.Value != Guid.Empty)
        {
            var c = await _clienteRepository.GetById(clienteId.Value);
            nomeCliente = c?.Nome;
        }

        string? nomeLead = null;
        if (leadId.HasValue && leadId.Value != Guid.Empty && _leadRepository is not null)
        {
            var l = await _leadRepository.GetById(leadId.Value);
            nomeLead = l?.Nome;
        }

        string? nomeUsuario = null;
        if (usuarioId.HasValue && usuarioId.Value != Guid.Empty)
        {
            var u = await _usuarioRepository.GetById(usuarioId.Value);
            nomeUsuario = u?.Nome;
        }

        return (nomeProjeto, nomeCliente, nomeLead, nomeUsuario);
    }

    public async Task<NomesRelacionadosBatch> ObterNomesEmLoteAsync(IEnumerable<Compromisso> compromissos)
    {
        var batch = new NomesRelacionadosBatch();
        var lista = compromissos.ToList();
        if (lista.Count == 0) return batch;

        await CarregarProjetosBatchAsync(batch, lista.Where(c => c.ProjetoId.HasValue).Select(c => c.ProjetoId!.Value).Distinct());
        await CarregarClientesBatchAsync(batch, lista.Where(c => c.ClienteId.HasValue).Select(c => c.ClienteId!.Value).Distinct());
        await CarregarLeadsBatchAsync(batch, lista.Where(c => c.LeadId.HasValue).Select(c => c.LeadId!.Value).Distinct());
        await CarregarUsuariosBatchAsync(batch, lista.Where(c => c.UsuarioId.HasValue).Select(c => c.UsuarioId!.Value).Distinct());

        return batch;
    }

    private async Task CarregarProjetosBatchAsync(NomesRelacionadosBatch batch, IEnumerable<Guid> ids)
    {
        foreach (var pid in ids)
        {
            var p = await _projetoRepository.GetById(pid);
            if (p?.Nome is not null) batch.Projetos[pid] = p.Nome;
        }
    }

    private async Task CarregarClientesBatchAsync(NomesRelacionadosBatch batch, IEnumerable<Guid> ids)
    {
        foreach (var cid in ids)
        {
            var cl = await _clienteRepository.GetById(cid);
            if (cl?.Nome is not null) batch.Clientes[cid] = cl.Nome;
        }
    }

    private async Task CarregarLeadsBatchAsync(NomesRelacionadosBatch batch, IEnumerable<Guid> ids)
    {
        if (_leadRepository is null) return;

        foreach (var lid in ids)
        {
            var ld = await _leadRepository.GetById(lid);
            if (ld?.Nome is not null) batch.Leads[lid] = ld.Nome;
        }
    }

    private async Task CarregarUsuariosBatchAsync(NomesRelacionadosBatch batch, IEnumerable<Guid> ids)
    {
        foreach (var uid in ids)
        {
            var u = await _usuarioRepository.GetById(uid);
            if (u?.Nome is not null) batch.Usuarios[uid] = u.Nome;
        }
    }
}
