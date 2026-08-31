using AutoMapper;
using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Projetos.DTOs;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Domain.Clientes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Projetos.Services;

public class ProjetoService : IProjetoService
{
    private readonly IProjetoRepository _repository;
    private readonly ITemplateProjetoRepository? _templateRepository;
    private readonly IClienteRepository? _clienteRepository;
    private readonly IUnitOfWork        _unitOfWork;
    private readonly IMapper            _mapper;

    public ProjetoService(
        IProjetoRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IClienteRepository? clienteRepository = null,
        ITemplateProjetoRepository? templateRepository = null)
    {
        _repository         = repository;
        _unitOfWork         = unitOfWork;
        _mapper             = mapper;
        _clienteRepository  = clienteRepository;
        _templateRepository = templateRepository;
    }

    public async Task<IEnumerable<ProjetoDto>> GetAll()
    {
        var projetos = await _repository.GetAllWithEtapas();
        var clientesMap = new Dictionary<Guid, string>();
        if (_clienteRepository != null)
        {
            var clientes = await _clienteRepository.GetAll();
            clientesMap = clientes.ToDictionary(c => c.Id, c => c.Nome);
        }

        return projetos.Select(p => ToDto(p, p.ClienteId.HasValue && clientesMap.TryGetValue(p.ClienteId.Value, out var nome) ? nome : null));
    }

    public async Task<ProjetoDto?> GetById(Guid id)
    {
        var projeto = await _repository.GetByIdWithEtapas(id);
        if (projeto is null) return null;

        string? clienteNome = null;
        if (_clienteRepository != null && projeto.ClienteId.HasValue)
        {
            var cliente = await _clienteRepository.GetById(projeto.ClienteId.Value);
            clienteNome = cliente?.Nome;
        }

        return ToDto(projeto, clienteNome);
    }

    public async Task<ProjetoDto> Create(CriarProjetoCommand command)
    {
        var projeto = _mapper.Map<Projeto>(command);
        await _repository.Create(projeto);
        await _unitOfWork.Commit();

        string? clienteNome = null;
        if (_clienteRepository != null && projeto.ClienteId.HasValue)
        {
            var cliente = await _clienteRepository.GetById(projeto.ClienteId.Value);
            clienteNome = cliente?.Nome;
        }

        return ToDto(projeto, clienteNome);
    }

    public async Task<ProjetoDto> Update(AtualizarProjetoCommand command)
    {
        var projeto = await _repository.GetByIdWithEtapas(command.Id)
            ?? throw new KeyNotFoundException($"Projeto {command.Id} não encontrado.");

        _mapper.Map(command, projeto);
        await _repository.Update(projeto);
        await _unitOfWork.Commit();

        string? clienteNome = null;
        if (_clienteRepository != null && projeto.ClienteId.HasValue)
        {
            var cliente = await _clienteRepository.GetById(projeto.ClienteId.Value);
            clienteNome = cliente?.Nome;
        }

        return ToDto(projeto, clienteNome);
    }

    public async Task<ProjetoDto> UpdateStatus(AtualizarStatusProjetoCommand command)
    {
        var projeto = await _repository.GetByIdWithEtapas(command.Id)
            ?? throw new KeyNotFoundException($"Projeto {command.Id} não encontrado.");

        projeto.Status      = command.Status;
        projeto.AtualizadoEm = DateTime.UtcNow;

        await _repository.Update(projeto);
        await _unitOfWork.Commit();

        string? clienteNome = null;
        if (_clienteRepository != null && projeto.ClienteId.HasValue)
        {
            var cliente = await _clienteRepository.GetById(projeto.ClienteId.Value);
            clienteNome = cliente?.Nome;
        }

        return ToDto(projeto, clienteNome);
    }

    public async Task<EtapaProjetoDto> CreateEtapa(CriarEtapaCommand command)
    {
        var etapa = new EtapaProjeto
        {
            Id        = Guid.NewGuid(),
            ProjetoId = command.ProjetoId,
            Nome      = command.Nome,
            Descricao = command.Descricao,
            Ordem     = command.Ordem,
            Status    = StatusEtapa.Pendente
        };

        await _repository.CreateEtapa(etapa);
        await _unitOfWork.Commit();
        return ToEtapaDto(etapa);
    }

    public async Task<EtapaProjetoDto> UpdateStatusEtapa(AtualizarStatusEtapaCommand command)
    {
        var etapa = await _repository.GetEtapaById(command.EtapaId)
            ?? throw new KeyNotFoundException($"Etapa {command.EtapaId} não encontrada.");

        etapa.Status = command.Status;

        if (command.Status == StatusEtapa.Concluida)
            etapa.DataConclusao = DateTime.UtcNow;

        await _repository.UpdateEtapa(etapa);
        await _unitOfWork.Commit();
        return ToEtapaDto(etapa);
    }

    public async Task Delete(Guid id)
    {
        await _repository.Delete(id);
        await _unitOfWork.Commit();
    }

    public async Task<TarefaEtapaDto> AdicionarTarefa(AdicionarTarefaCommand command)
    {
        _ = await _repository.GetEtapaById(command.EtapaId)
            ?? throw new KeyNotFoundException($"Etapa {command.EtapaId} não encontrada.");

        var tarefa = new TarefaEtapa
        {
            Id        = Guid.NewGuid(),
            EtapaId   = command.EtapaId,
            Titulo    = command.Titulo,
            Concluida = false,
            CriadoEm  = DateTime.UtcNow
        };

        await _repository.CreateTarefa(tarefa);
        await _unitOfWork.Commit();

        return new TarefaEtapaDto(tarefa.Id, tarefa.EtapaId, tarefa.Titulo, tarefa.Concluida, tarefa.CriadoEm);
    }

    public async Task<TarefaEtapaDto> AlternarTarefa(Guid tarefaId)
    {
        var tarefa = await _repository.GetTarefaById(tarefaId)
            ?? throw new KeyNotFoundException($"Tarefa {tarefaId} não encontrada.");

        tarefa.Concluida = !tarefa.Concluida;
        await _repository.UpdateTarefa(tarefa);
        await _unitOfWork.Commit();

        return new TarefaEtapaDto(tarefa.Id, tarefa.EtapaId, tarefa.Titulo, tarefa.Concluida, tarefa.CriadoEm);
    }

    public async Task RemoverTarefa(Guid tarefaId)
    {
        var tarefa = await _repository.GetTarefaById(tarefaId)
            ?? throw new KeyNotFoundException($"Tarefa {tarefaId} não encontrada.");

        await _repository.DeleteTarefa(tarefa);
        await _unitOfWork.Commit();
    }

    public async Task<IEnumerable<TemplateProjetoDto>> ObterTemplates()
    {
        if (_templateRepository == null) return Enumerable.Empty<TemplateProjetoDto>();

        var templates = await _templateRepository.GetAllWithEtapas();
        return templates.Select(ToTemplateDto);
    }

    public async Task<TemplateProjetoDto?> ObterTemplatePorId(Guid id)
    {
        if (_templateRepository == null) return null;

        var template = await _templateRepository.GetByIdWithEtapas(id);
        return template is null ? null : ToTemplateDto(template);
    }

    public async Task<TemplateProjetoDto> CriarTemplate(CriarTemplateProjetoCommand command)
    {
        if (_templateRepository == null)
            throw new InvalidOperationException("Repositório de templates não configurado.");

        var templateId = Guid.NewGuid();
        var template = new TemplateProjeto
        {
            Id        = templateId,
            Codigo    = command.Codigo,
            Nome      = command.Nome,
            Descricao = command.Descricao,
            Icone     = command.Icone,
            Ativo     = true,
            CriadoEm  = DateTime.UtcNow,
            Etapas    = command.Etapas?.Select(e => new TemplateEtapa
            {
                Id                = Guid.NewGuid(),
                TemplateProjetoId = templateId,
                Nome              = e.Nome,
                Descricao         = e.Descricao,
                Ordem             = e.Ordem,
                TarefasJson       = e.Tarefas != null ? JsonSerializer.Serialize(e.Tarefas) : null
            }).ToList() ?? new List<TemplateEtapa>()
        };

        await _templateRepository.Create(template);
        if (template.Etapas.Any())
        {
            _templateRepository.AddEtapas(template.Etapas);
        }
        await _unitOfWork.Commit();

        return ToTemplateDto(template);
    }

    public async Task<TemplateProjetoDto> AtualizarTemplate(AtualizarTemplateProjetoCommand command)
    {
        if (_templateRepository == null)
            throw new InvalidOperationException("Repositório de templates não configurado.");

        var template = await _templateRepository.GetByIdWithEtapas(command.Id)
            ?? throw new KeyNotFoundException($"Template {command.Id} não encontrado.");

        template.Nome = command.Nome;
        template.Descricao = command.Descricao;
        template.Icone = command.Icone;

        if (command.Etapas != null)
        {
            var oldEtapas = template.Etapas.ToList();
            if (oldEtapas.Any())
            {
                _templateRepository.RemoveEtapas(oldEtapas);
            }

            var newEtapas = command.Etapas.Select(e => new TemplateEtapa
            {
                Id = Guid.NewGuid(),
                TemplateProjetoId = template.Id,
                Nome = e.Nome,
                Descricao = e.Descricao,
                Ordem = e.Ordem,
                TarefasJson = e.Tarefas != null ? JsonSerializer.Serialize(e.Tarefas) : null
            }).ToList();

            _templateRepository.AddEtapas(newEtapas);
            template.Etapas = newEtapas;
        }

        await _unitOfWork.Commit();

        return ToTemplateDto(template);
    }

    public async Task ExcluirTemplate(Guid id)
    {
        if (_templateRepository == null)
            throw new InvalidOperationException("Repositório de templates não configurado.");

        var template = await _templateRepository.GetByIdWithEtapas(id)
            ?? throw new KeyNotFoundException($"Template {id} não encontrado.");

        await _templateRepository.Delete(template.Id);
        await _unitOfWork.Commit();
    }

    private static ProjetoDto ToDto(Projeto p, string? clienteNome = null)
    {
        var total      = p.Etapas.Count;
        var concluidas = p.Etapas.Count(e => e.Status == StatusEtapa.Concluida);
        var progresso  = total == 0 ? 0 : (int)Math.Round((double)concluidas / total * 100);

        return new ProjetoDto(
            p.Id, 
            p.Nome ?? string.Empty, 
            p.Descricao ?? string.Empty,
            p.Status ?? StatusProjeto.Briefing,  
            (p.Status ?? StatusProjeto.Briefing).ToString(),
            p.Tipo ?? TipoProjeto.Residencial,    
            (p.Tipo ?? TipoProjeto.Residencial).ToString(),
            p.DataInicio ?? DateTime.UtcNow, 
            p.DataPrevistaEntrega,
            p.MetragemTotal ?? 0, 
            p.ClienteId ?? Guid.Empty,
            p.CriadoEm ?? DateTime.UtcNow, 
            p.AtualizadoEm,
            p.Etapas.OrderBy(e => e.Ordem ?? 0).Select(ToEtapaDto),
            progresso,
            clienteNome
        );
    }

    private static EtapaProjetoDto ToEtapaDto(EtapaProjeto e) =>
        new(e.Id, 
            e.ProjetoId ?? Guid.Empty, 
            e.Nome ?? string.Empty, 
            e.Descricao ?? string.Empty,
            e.Status ?? StatusEtapa.Pendente, 
            (e.Status ?? StatusEtapa.Pendente).ToString(), 
            e.Ordem ?? 0, 
            e.DataConclusao,
            e.Tarefas?.OrderBy(t => t.CriadoEm).Select(t => new TarefaEtapaDto(t.Id, t.EtapaId, t.Titulo, t.Concluida, t.CriadoEm)));

    private static TemplateProjetoDto ToTemplateDto(TemplateProjeto t) =>
        new(t.Id,
            t.Codigo,
            t.Nome,
            t.Descricao,
            t.Icone,
            t.Ativo,
            t.Etapas.OrderBy(e => e.Ordem).Select(e => new TemplateEtapaDto(
                e.Id,
                e.TemplateProjetoId,
                e.Nome,
                e.Descricao,
                e.Ordem,
                string.IsNullOrEmpty(e.TarefasJson) 
                    ? Enumerable.Empty<string>() 
                    : JsonSerializer.Deserialize<List<string>>(e.TarefasJson) ?? Enumerable.Empty<string>()
            )));
}
