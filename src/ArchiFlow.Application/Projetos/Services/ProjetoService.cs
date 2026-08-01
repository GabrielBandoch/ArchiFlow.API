using AutoMapper;
using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Projetos.DTOs;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Projetos.Enum;

namespace ArchiFlow.Application.Projetos.Services;

public class ProjetoService : IProjetoService
{
    private readonly IProjetoRepository _repository;
    private readonly IUnitOfWork        _unitOfWork;
    private readonly IMapper            _mapper;

    public ProjetoService(
        IProjetoRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper     = mapper;
    }

    public async Task<IEnumerable<ProjetoDto>> GetAll()
    {
        var projetos = await _repository.GetAllWithEtapas();
        return projetos.Select(ToDto);
    }

    public async Task<ProjetoDto?> GetById(Guid id)
    {
        var projeto = await _repository.GetByIdWithEtapas(id);
        return projeto is null ? null : ToDto(projeto);
    }

    public async Task<ProjetoDto> Create(CriarProjetoCommand command)
    {
        var projeto = _mapper.Map<Projeto>(command);
        await _repository.Create(projeto);
        await _unitOfWork.Commit();
        return ToDto(projeto);
    }

    public async Task<ProjetoDto> Update(AtualizarProjetoCommand command)
    {
        var projeto = await _repository.GetByIdWithEtapas(command.Id)
            ?? throw new KeyNotFoundException($"Projeto {command.Id} não encontrado.");

        _mapper.Map(command, projeto);
        await _repository.Update(projeto);
        await _unitOfWork.Commit();
        return ToDto(projeto);
    }

    public async Task<ProjetoDto> UpdateStatus(AtualizarStatusProjetoCommand command)
    {
        var projeto = await _repository.GetByIdWithEtapas(command.Id)
            ?? throw new KeyNotFoundException($"Projeto {command.Id} não encontrado.");

        projeto.Status      = command.Status;
        projeto.AtualizadoEm = DateTime.UtcNow;

        await _repository.Update(projeto);
        await _unitOfWork.Commit();
        return ToDto(projeto);
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

    private static ProjetoDto ToDto(Projeto p)
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
            progresso
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
            e.DataConclusao);
}
