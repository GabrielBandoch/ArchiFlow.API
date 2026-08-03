using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Usuarios;
using ArchiFlow.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class TokenServiceTests
{
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET", "SuperSecretKeyForArchiFlowAPIProject123456!");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "ArchiFlow.API");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "ArchiFlow.App");
        Environment.SetEnvironmentVariable("JWT_EXPIRATION_MINUTES", "60");

        _sut = new TokenService();
    }

    [Fact]
    public void GenerateToken_ParaUsuario_DeveRetornarTokenValidoComClaims()
    {
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Arquiteto Teste",
            Email = "arquiteto@test.com",
            Role = "Administrador"
        };

        var token = _sut.GenerateToken(usuario);

        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be("ArchiFlow.API");
        jwtToken.Claims.Should().Contain(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" && c.Value == usuario.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" && c.Value == usuario.Nome);
        jwtToken.Claims.Should().Contain(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" && c.Value == usuario.Email);
        jwtToken.Claims.Should().Contain(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && c.Value == usuario.Role);
        jwtToken.Claims.Should().Contain(c => c.Type == "user_type" && c.Value == "staff");
    }

    [Fact]
    public void GenerateToken_ParaClienteComProjeto_DeveRetornarTokenComProjetoClaim()
    {
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente Teste",
            Email = "cliente@test.com"
        };
        var projetoId = Guid.NewGuid();

        var token = _sut.GenerateToken(cliente, projetoId);

        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && c.Value == "Cliente");
        jwtToken.Claims.Should().Contain(c => c.Type == "user_type" && c.Value == "client");
        jwtToken.Claims.Should().Contain(c => c.Type == "projeto_id" && c.Value == projetoId.ToString());
    }

    [Fact]
    public void GenerateToken_ParaClienteSemProjeto_DeveRetornarTokenSemProjetoIdClaim()
    {
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente Teste",
            Email = "cliente@test.com"
        };

        var token = _sut.GenerateToken(cliente, null);

        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && c.Value == "Cliente");
        jwtToken.Claims.Should().Contain(c => c.Type == "user_type" && c.Value == "client");
        jwtToken.Claims.Should().NotContain(c => c.Type == "projeto_id");
    }
}
