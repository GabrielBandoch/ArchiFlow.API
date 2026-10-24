using System;
using System.IO;
using System.Threading.Tasks;
using ArchiFlow.Application.Empresa.Commands;
using ArchiFlow.Application.Empresa.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Empresa;
using ArchiFlow.Application.Security;
using Microsoft.Extensions.Logging;

namespace ArchiFlow.Application.Empresa.Services;

public class EmpresaService : IEmpresaService
{
    private readonly IDadosEmpresaRepository _empresaRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<EmpresaService> _logger;

    public EmpresaService(
        IDadosEmpresaRepository empresaRepository,
        IStorageService storageService,
        ILogger<EmpresaService> logger)
    {
        _empresaRepository = empresaRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<DadosEmpresaDto> ObterDadosEmpresaAsync()
    {
        var empresa = await _empresaRepository.ObterAsync();
        if (empresa == null)
        {
            empresa = new DadosEmpresa
            {
                Id = Guid.NewGuid(),
                NomeEmpresa = "COTU Studio",
                CorPrimaria = "#765538",
                AtualizadoEm = DateTime.UtcNow
            };
            await _empresaRepository.SalvarAsync(empresa);
        }

        return MapearParaDto(empresa);
    }

    public async Task<DadosEmpresaDto> SalvarDadosEmpresaAsync(SalvarDadosEmpresaCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.NomeEmpresa))
            throw new ArgumentException("Nome da empresa é obrigatório.");

        var empresa = await _empresaRepository.ObterAsync() ?? new DadosEmpresa { Id = Guid.NewGuid() };

        empresa.NomeEmpresa = command.NomeEmpresa.Trim();
        empresa.RazaoSocial = command.RazaoSocial?.Trim();
        empresa.Cnpj = command.Cnpj?.Trim();
        empresa.RegistroProfissional = command.RegistroProfissional?.Trim();
        empresa.Slogan = command.Slogan?.Trim();
        empresa.Email = command.Email?.Trim();
        empresa.Telefone = command.Telefone?.Trim();
        empresa.Endereco = command.Endereco?.Trim();
        empresa.Cidade = command.Cidade?.Trim();
        empresa.Estado = command.Estado?.Trim();
        empresa.Cep = command.Cep?.Trim();
        empresa.Website = command.Website?.Trim();
        if (!string.IsNullOrWhiteSpace(command.LogoUrl))
        {
            empresa.LogoUrl = command.LogoUrl;
        }
        if (!string.IsNullOrWhiteSpace(command.CorPrimaria))
        {
            empresa.CorPrimaria = command.CorPrimaria.Trim();
        }

        empresa.AtualizadoEm = DateTime.UtcNow;

        await _empresaRepository.SalvarAsync(empresa);

        return MapearParaDto(empresa);
    }

    public async Task<DadosEmpresaDto> AtualizarCorPrimariaAsync(string corPrimaria)
    {
        if (string.IsNullOrWhiteSpace(corPrimaria))
            throw new ArgumentException("Cor primária é obrigatória.");

        var empresa = await _empresaRepository.ObterAsync() ?? new DadosEmpresa
        {
            Id = Guid.NewGuid(),
            NomeEmpresa = "COTU Studio"
        };

        empresa.CorPrimaria = corPrimaria.Trim();
        empresa.AtualizadoEm = DateTime.UtcNow;

        await _empresaRepository.SalvarAsync(empresa);

        return MapearParaDto(empresa);
    }

    public async Task<string> AtualizarLogotipoAsync(Stream stream, string fileName, string contentType)
    {
        FileSecurityValidator.ValidarImagem(stream, fileName, contentType);

        var empresa = await _empresaRepository.ObterAsync() ?? new DadosEmpresa { Id = Guid.NewGuid() };

        if (!string.IsNullOrEmpty(empresa.LogoUrl))
        {
            try
            {
                await _storageService.DeleteAsync(empresa.LogoUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao remover logotipo anterior do storage.");
            }
        }

        var url = await _storageService.UploadAsync(stream, fileName, contentType, "logos", null);
        empresa.LogoUrl = url;
        empresa.AtualizadoEm = DateTime.UtcNow;

        await _empresaRepository.SalvarAsync(empresa);

        return url;
    }

    public async Task RemoverLogotipoAsync()
    {
        var empresa = await _empresaRepository.ObterAsync();
        if (empresa != null && !string.IsNullOrEmpty(empresa.LogoUrl))
        {
            try
            {
                await _storageService.DeleteAsync(empresa.LogoUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao remover logotipo do storage.");
            }

            empresa.LogoUrl = null;
            empresa.AtualizadoEm = DateTime.UtcNow;
            await _empresaRepository.SalvarAsync(empresa);
        }
    }

    private static DadosEmpresaDto MapearParaDto(DadosEmpresa e) =>
        new(
            e.Id,
            e.EscritorioId,
            e.NomeEmpresa,
            e.RazaoSocial,
            e.Cnpj,
            e.RegistroProfissional,
            e.Slogan,
            e.Email,
            e.Telefone,
            e.Endereco,
            e.Cidade,
            e.Estado,
            e.Cep,
            e.Website,
            e.LogoUrl,
            e.CorPrimaria,
            e.AtualizadoEm
        );
}

