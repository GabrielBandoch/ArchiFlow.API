using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Projetos.DTOs;

namespace ArchiFlow.Application.Interfaces.Facades;

public interface IProjetoFacade
{
    Task<IEnumerable<ProjetoDto>> GetAll();
    Task<ProjetoDto?> GetById(Guid id);
    Task<ProjetoDto> Create(CriarProjetoCommand command);
    Task<ProjetoDto> Update(AtualizarProjetoCommand command);
    Task<ProjetoDto> UpdateStatus(AtualizarStatusProjetoCommand command);
    Task<EtapaProjetoDto> CreateEtapa(CriarEtapaCommand command);
    Task<EtapaProjetoDto> UpdateStatusEtapa(AtualizarStatusEtapaCommand command);
    Task Delete(Guid id);
}
