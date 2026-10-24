using ArchiFlow.Domain.Empresa;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Repositories.Empresa;

public class DadosEmpresaRepository : IDadosEmpresaRepository
{
    private readonly ArchiFlowDbContext _context;

    public DadosEmpresaRepository(ArchiFlowDbContext context)
    {
        _context = context;
    }

    public async Task<DadosEmpresa?> ObterAsync()
    {
        return await _context.DadosEmpresa.FirstOrDefaultAsync();
    }

    public async Task SalvarAsync(DadosEmpresa dadosEmpresa)
    {
        var existente = await _context.DadosEmpresa.FirstOrDefaultAsync();
        if (existente == null)
        {
            dadosEmpresa.AtualizadoEm = DateTime.UtcNow;
            await _context.DadosEmpresa.AddAsync(dadosEmpresa);
        }
        else
        {
            existente.NomeEmpresa = dadosEmpresa.NomeEmpresa;
            existente.RazaoSocial = dadosEmpresa.RazaoSocial;
            existente.Cnpj = dadosEmpresa.Cnpj;
            existente.RegistroProfissional = dadosEmpresa.RegistroProfissional;
            existente.Slogan = dadosEmpresa.Slogan;
            existente.Email = dadosEmpresa.Email;
            existente.Telefone = dadosEmpresa.Telefone;
            existente.Endereco = dadosEmpresa.Endereco;
            existente.Cidade = dadosEmpresa.Cidade;
            existente.Estado = dadosEmpresa.Estado;
            existente.Cep = dadosEmpresa.Cep;
            existente.Website = dadosEmpresa.Website;
            if (!string.IsNullOrWhiteSpace(dadosEmpresa.LogoUrl))
            {
                existente.LogoUrl = dadosEmpresa.LogoUrl;
            }
            if (!string.IsNullOrWhiteSpace(dadosEmpresa.CorPrimaria))
            {
                existente.CorPrimaria = dadosEmpresa.CorPrimaria;
            }
            existente.AtualizadoEm = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}

