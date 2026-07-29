using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Projetos.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;

namespace ArchiFlow.Application.Projetos.Facades;

public class ProjetoFacade : IProjetoFacade
{
    private readonly IProjetoService _service;

    public ProjetoFacade(IProjetoService service) => _service = service;

    public Task<IEnumerable<ProjetoDto>> GetAll()                               => _service.GetAll();
    public Task<ProjetoDto?> GetById(Guid id)                                    => _service.GetById(id);
    public Task<ProjetoDto> Create(CriarProjetoCommand command)                      => _service.Create(command);
    public Task<ProjetoDto> Update(AtualizarProjetoCommand command)               => _service.Update(command);
    public Task<ProjetoDto> UpdateStatus(AtualizarStatusProjetoCommand command)   => _service.UpdateStatus(command);
    public Task<EtapaProjetoDto> CreateEtapa(CriarEtapaCommand command)              => _service.CreateEtapa(command);
    public Task<EtapaProjetoDto> UpdateStatusEtapa(AtualizarStatusEtapaCommand c) => _service.UpdateStatusEtapa(c);
    public Task Delete(Guid id)                                                     => _service.Delete(id);
}
