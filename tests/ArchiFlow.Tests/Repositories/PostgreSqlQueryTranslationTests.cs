using System.Linq;
using ArchiFlow.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArchiFlow.Tests.Repositories;

public class PostgreSqlQueryTranslationTests
{
    [Fact]
    public void Repositories_ConsultasComCaseInsensitive_DevemSerTradutiveisParaPostgreSql()
    {
        var options = new DbContextOptionsBuilder<ArchiFlowDbContext>()
            .UseNpgsql("Host=localhost;Database=dummy;Username=postgres;Password=postgres")
            .Options;

        using var context = new ArchiFlowDbContext(options);

        // UsuarioRepository.GetByEmail
        var sqlUsuario = context.Usuarios
            .Where(u => u.Email.ToLower() == "teste@archiflow.com")
            .ToQueryString();
        sqlUsuario.Should().Contain("lower");

        // ClienteRepository.GetByEmail
        var sqlCliente = context.Clientes
            .Where(c => c.Email.ToLower() == "teste@archiflow.com")
            .ToQueryString();
        sqlCliente.Should().Contain("lower");

        // RepresentanteClienteRepository.ObterPorEmailAsync
        var sqlRep = context.RepresentantesClientes
            .Where(r => r.Email.ToLower() == "teste@archiflow.com")
            .ToQueryString();
        sqlRep.Should().Contain("lower");

        // FornecedorRepository.ObterPorEspecialidadeAsync
        var sqlFornecedor = context.Fornecedores
            .Where(f => f.Especialidade.ToLower() == "pintura" || f.Especialidade.ToLower().Contains("pintura"))
            .ToQueryString();
        sqlFornecedor.Should().Contain("lower");

        // OpcaoConfiguracaoRepository.ObterPorCategoriaAsync
        var sqlOpcao = context.OpcoesConfiguracao
            .Where(o => o.Ativo && o.Categoria.ToLower() == "categoria")
            .ToQueryString();
        sqlOpcao.Should().Contain("lower");

        // TokenRecuperacaoSenhaRepository.InvalidarParaEmailAsync
        var sqlToken = context.TokensRecuperacaoSenha
            .Where(t => t.Email.ToLower() == "teste@archiflow.com" && t.UsadoEm == null)
            .ToQueryString();
        sqlToken.Should().Contain("lower");
    }
}
