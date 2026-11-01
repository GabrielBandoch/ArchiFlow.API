using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Application.Configuracoes.DTOs;

namespace ArchiFlow.Application.Interfaces.Services;

public interface IConfiguracaoSistemaService
{
    Task<IEnumerable<OpcaoConfiguracaoDto>> ObterPorCategoriaAsync(string categoria);
    Task<IDictionary<string, IEnumerable<OpcaoConfiguracaoDto>>> ObterTodasAgrupadasAsync();
    Task<OpcaoConfiguracaoDto> SalvarOpcaoAsync(SalvarOpcaoConfiguracaoCommand command);
}
