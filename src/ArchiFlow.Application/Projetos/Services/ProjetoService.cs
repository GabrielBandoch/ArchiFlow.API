using AutoMapper;
using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Projetos.DTOs;
using ArchiFlow.Infrastructure.Data;
using ArchiFlow.Application.Projetos.Services.Interface;

namespace ArchiFlow.Application.Projetos.Services;

public class ProjetoService : IProjetoService
{
    public ProjetoService(ArchiFlowDbContext context, IMapper mapper) { }

    public Task<IEnumerable<ProjetoDto>> ObterTodos() =>
        throw new NotImplementedException("Pendente de Implementação");

    public Task<ProjetoDto?> ObterPorId(Guid id) =>
        throw new NotImplementedException("Pendente de Implementação");

    public Task<ProjetoDto> Criar(CriarProjetoCommand command) =>
        throw new NotImplementedException("Pendente de Implementação");

    public Task<ProjetoDto> Atualizar(AtualizarProjetoCommand command) =>
        throw new NotImplementedException("Pendente de Implementação");

    public Task<ProjetoDto> AtualizarStatus(AtualizarStatusProjetoCommand command) =>
        throw new NotImplementedException("Pendente de Implementação");

    public Task<EtapaProjetoDto> CriarEtapa(CriarEtapaCommand command) =>
        throw new NotImplementedException("Pendente de Implementação");

    public Task<EtapaProjetoDto> AtualizarStatusEtapa(AtualizarStatusEtapaCommand command) =>
        throw new NotImplementedException("Pendente de Implementação");

    public Task Excluir(Guid id) =>
        throw new NotImplementedException("Pendente de Implementação");
}
