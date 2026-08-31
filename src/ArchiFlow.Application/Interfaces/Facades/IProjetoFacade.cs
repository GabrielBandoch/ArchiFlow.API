using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Projetos.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    Task<TarefaEtapaDto> AdicionarTarefa(AdicionarTarefaCommand command);
    Task<TarefaEtapaDto> AlternarTarefa(Guid tarefaId);
    Task RemoverTarefa(Guid tarefaId);

    Task<IEnumerable<TemplateProjetoDto>> ObterTemplates();
    Task<TemplateProjetoDto?> ObterTemplatePorId(Guid id);
    Task<TemplateProjetoDto> CriarTemplate(CriarTemplateProjetoCommand command);
    Task<TemplateProjetoDto> AtualizarTemplate(AtualizarTemplateProjetoCommand command);
    Task ExcluirTemplate(Guid id);
}
