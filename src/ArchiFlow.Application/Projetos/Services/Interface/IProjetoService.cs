using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Projetos.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Projetos.Services.Interface;
public interface IProjetoService
{
    Task<IEnumerable<ProjetoDto>> ObterTodos();
    Task<ProjetoDto?> ObterPorId(Guid id);
    Task<ProjetoDto> Criar(CriarProjetoCommand command);
    Task<ProjetoDto> Atualizar(AtualizarProjetoCommand command);
    Task<ProjetoDto> AtualizarStatus(AtualizarStatusProjetoCommand command);
    Task<EtapaProjetoDto> CriarEtapa(CriarEtapaCommand command);
    Task<EtapaProjetoDto> AtualizarStatusEtapa(AtualizarStatusEtapaCommand command);
    Task Excluir(Guid id);
}