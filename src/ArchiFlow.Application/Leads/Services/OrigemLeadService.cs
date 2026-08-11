using ArchiFlow.Application.Leads.Commands;
using ArchiFlow.Application.Leads.DTOs;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Leads.Services;

public class OrigemLeadService : IOrigemLeadService
{
    private readonly IOrigemLeadRepository _repository;
    private readonly IUnitOfWork           _unitOfWork;

    public OrigemLeadService(
        IOrigemLeadRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<OrigemLeadDto>> GetAll()
    {
        var origens = await _repository.GetAll();
        return origens.OrderBy(o => o.Descricao).Select(ToDto);
    }

    public async Task<IEnumerable<OrigemLeadDto>> GetAllActive()
    {
        var origens = await _repository.GetAllActive();
        return origens.Select(ToDto);
    }

    public async Task<OrigemLeadDto?> GetById(Guid id)
    {
        var origem = await _repository.GetById(id);
        return origem is null ? null : ToDto(origem);
    }

    public async Task<OrigemLeadDto> Create(CriarOrigemLeadCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Descricao))
        {
            throw new ArgumentException("A descrição da origem do lead é obrigatória.");
        }

        var origem = new OrigemLead
        {
            Id        = Guid.NewGuid(),
            Descricao = command.Descricao.Trim(),
            Ativo     = true,
            CriadoEm  = DateTime.UtcNow
        };

        await _repository.Create(origem);
        await _unitOfWork.Commit();
        return ToDto(origem);
    }

    public async Task<OrigemLeadDto> Update(AtualizarOrigemLeadCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Descricao))
        {
            throw new ArgumentException("A descrição da origem do lead é obrigatória.");
        }

        var origem = await _repository.GetById(command.Id)
            ?? throw new KeyNotFoundException($"Origem de lead {command.Id} não encontrada.");

        origem.Descricao = command.Descricao.Trim();

        await _repository.Update(origem);
        await _unitOfWork.Commit();
        return ToDto(origem);
    }

    public async Task<OrigemLeadDto> Desativar(Guid id)
    {
        var origem = await _repository.GetById(id)
            ?? throw new KeyNotFoundException($"Origem de lead {id} não encontrada.");

        origem.Ativo = false;

        await _repository.Update(origem);
        await _unitOfWork.Commit();
        return ToDto(origem);
    }

    public async Task<OrigemLeadDto> Reativar(Guid id)
    {
        var origem = await _repository.GetById(id)
            ?? throw new KeyNotFoundException($"Origem de lead {id} não encontrada.");

        origem.Ativo = true;

        await _repository.Update(origem);
        await _unitOfWork.Commit();
        return ToDto(origem);
    }

    private static OrigemLeadDto ToDto(OrigemLead o) =>
        new(o.Id, o.Descricao, o.Ativo, o.CriadoEm);
}
