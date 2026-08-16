using AutoMapper;
using ArchiFlow.Application.Leads.Commands;
using ArchiFlow.Application.Leads.DTOs;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Leads.Enum;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Application.Interfaces.Services;

namespace ArchiFlow.Application.Leads.Services;

public class LeadService : ILeadService
{
    private readonly ILeadRepository _repository;
    private readonly IUnitOfWork     _unitOfWork;
    private readonly IMapper         _mapper;

    public LeadService(
        ILeadRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper     = mapper;
    }

    public async Task<IEnumerable<LeadDto>> GetAll()
    {
        var leads = await _repository.GetAllWithHistorico();
        return leads.Select(ToDto);
    }

    public async Task<LeadDto?> GetById(Guid id)
    {
        var lead = await _repository.GetByIdWithHistorico(id);
        return lead is null ? null : ToDto(lead);
    }

    public async Task<LeadDto> Create(CriarLeadCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ArgumentException("E-mail é obrigatório.");
        }

        var existing = await _repository.GetByEmail(command.Email.Trim());
        if (existing != null)
        {
            throw new ArgumentException("Este e-mail já está cadastrado para outro lead.");
        }

        var lead = _mapper.Map<Lead>(command);
        await _repository.Create(lead);
        await _unitOfWork.Commit();
        return ToDto(lead);
    }

    public async Task<LeadDto> Update(AtualizarLeadCommand command)
    {
        var lead = await _repository.GetByIdWithHistorico(command.Id)
            ?? throw new KeyNotFoundException($"Lead {command.Id} não encontrado.");

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ArgumentException("E-mail é obrigatório.");
        }

        var existing = await _repository.GetByEmail(command.Email.Trim());
        if (existing != null && existing.Id != command.Id)
        {
            throw new ArgumentException("Este e-mail já está cadastrado para outro lead.");
        }

        _mapper.Map(command, lead);
        lead.AtualizadoEm = DateTime.UtcNow;

        await _repository.Update(lead);
        await _unitOfWork.Commit();
        return ToDto(lead);
    }

    public async Task<LeadDto> UpdateStatus(AtualizarStatusLeadCommand command)
    {
        var lead = await _repository.GetByIdWithHistorico(command.Id)
            ?? throw new KeyNotFoundException($"Lead {command.Id} não encontrado.");

        if (lead.Status == StatusLead.Convertido && command.Status != StatusLead.Convertido)
        {
            throw new InvalidOperationException("Não é permitido alterar o status de um lead que já foi convertido em cliente.");
        }

        lead.Status = command.Status;
        if (command.Status == StatusLead.Perdido)
        {
            lead.MotivoPerda = command.MotivoPerda;
        }
        else
        {
            lead.MotivoPerda = null;
        }
        lead.AtualizadoEm = DateTime.UtcNow;

        await _repository.Update(lead);
        await _unitOfWork.Commit();
        return ToDto(lead);
    }

    public async Task<HistoricoContatoLeadDto> RegisterContact(RegistrarContatoLeadCommand command)
    {
        var lead = await _repository.GetByIdWithHistorico(command.LeadId)
            ?? throw new KeyNotFoundException($"Lead {command.LeadId} não encontrado.");

        var historico = new HistoricoContatoLead
        {
            Id          = Guid.NewGuid(),
            LeadId      = command.LeadId,
            DataContato = DateTime.UtcNow,
            Canal       = command.Canal,
            Resumo      = command.Resumo
        };

        lead.AtualizadoEm = DateTime.UtcNow;

        await _repository.CreateHistorico(historico);
        await _unitOfWork.Commit();
        return ToHistoricoDto(historico);
    }

    public async Task Delete(Guid id)
    {
        await _repository.Delete(id);
        await _unitOfWork.Commit();
    }

    private static LeadDto ToDto(Lead l)
    {
        return new LeadDto(
            l.Id,
            l.Nome ?? string.Empty,
            l.Email ?? string.Empty,
            l.Telefone,
            l.OrigemId,
            l.Origem?.Descricao,
            l.MotivoPerda,
            l.Status,
            l.Status.ToString(),
            l.CriadoEm,
            l.AtualizadoEm,
            l.HistoricoContatos.OrderByDescending(h => h.DataContato).Select(ToHistoricoDto)
        );
    }

    private static HistoricoContatoLeadDto ToHistoricoDto(HistoricoContatoLead h) =>
        new(h.Id,
            h.LeadId,
            h.DataContato,
            h.Canal ?? string.Empty,
            h.Resumo ?? string.Empty);
}
