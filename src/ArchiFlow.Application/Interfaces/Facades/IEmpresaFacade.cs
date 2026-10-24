using System.IO;
using System.Threading.Tasks;
using ArchiFlow.Application.Empresa.Commands;
using ArchiFlow.Application.Empresa.DTOs;

namespace ArchiFlow.Application.Interfaces.Facades;

public interface IEmpresaFacade
{
    Task<DadosEmpresaDto> ObterDadosEmpresaAsync();
    Task<DadosEmpresaDto> SalvarDadosEmpresaAsync(SalvarDadosEmpresaCommand command);
    Task<DadosEmpresaDto> AtualizarCorPrimariaAsync(string corPrimaria);
    Task<string> AtualizarLogotipoAsync(Stream stream, string fileName, string contentType);
    Task RemoverLogotipoAsync();
}

