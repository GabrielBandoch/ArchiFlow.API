using System.IO;
using System.Threading.Tasks;
using ArchiFlow.Application.Empresa.Commands;
using ArchiFlow.Application.Empresa.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;

namespace ArchiFlow.Application.Empresa.Facades;

public class EmpresaFacade : IEmpresaFacade
{
    private readonly IEmpresaService _empresaService;

    public EmpresaFacade(IEmpresaService empresaService)
    {
        _empresaService = empresaService;
    }

    public Task<DadosEmpresaDto> ObterDadosEmpresaAsync() =>
        _empresaService.ObterDadosEmpresaAsync();

    public Task<DadosEmpresaDto> SalvarDadosEmpresaAsync(SalvarDadosEmpresaCommand command) =>
        _empresaService.SalvarDadosEmpresaAsync(command);

    public Task<DadosEmpresaDto> AtualizarCorPrimariaAsync(string corPrimaria) =>
        _empresaService.AtualizarCorPrimariaAsync(corPrimaria);

    public Task<string> AtualizarLogotipoAsync(Stream stream, string fileName, string contentType) =>
        _empresaService.AtualizarLogotipoAsync(stream, fileName, contentType);

    public Task RemoverLogotipoAsync() =>
        _empresaService.RemoverLogotipoAsync();
}
