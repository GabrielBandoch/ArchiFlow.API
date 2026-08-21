using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Projetos.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    public Task<EtapaProjetoDto> UpdateStatusEtapa(AtualizarStatusEtapaCommand command) => _service.UpdateStatusEtapa(command);
    public Task Delete(Guid id)                                                     => _service.Delete(id);

    public Task<TarefaEtapaDto> AdicionarTarefa(AdicionarTarefaCommand command) => _service.AdicionarTarefa(command);
    public Task<TarefaEtapaDto> AlternarTarefa(Guid tarefaId)                   => _service.AlternarTarefa(tarefaId);
    public Task RemoverTarefa(Guid tarefaId)                                    => _service.RemoverTarefa(tarefaId);

    public Task<IEnumerable<TemplateProjetoDto>> ObterTemplates()               => _service.ObterTemplates();
    public Task<TemplateProjetoDto?> ObterTemplatePorId(Guid id)                => _service.ObterTemplatePorId(id);
    public Task<TemplateProjetoDto> CriarTemplate(CriarTemplateProjetoCommand command) => _service.CriarTemplate(command);
}
