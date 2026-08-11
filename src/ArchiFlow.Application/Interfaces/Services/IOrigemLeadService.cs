using ArchiFlow.Application.Leads.Commands;
using ArchiFlow.Application.Leads.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Interfaces.Services;

public interface IOrigemLeadService
{
    Task<IEnumerable<OrigemLeadDto>> GetAll();
    Task<IEnumerable<OrigemLeadDto>> GetAllActive();
    Task<OrigemLeadDto?> GetById(Guid id);
    Task<OrigemLeadDto> Create(CriarOrigemLeadCommand command);
    Task<OrigemLeadDto> Update(AtualizarOrigemLeadCommand command);
    Task<OrigemLeadDto> Desativar(Guid id);
    Task<OrigemLeadDto> Reativar(Guid id);
}
