using ArchiFlow.Application.Usuarios.DTOs;
using FluentAssertions;
using System;
using Xunit;

namespace ArchiFlow.Tests.DTOs;

public class DtoTests
{
    [Fact]
    public void Dtos_DevePermitirObterPropriedades()
    {
        var id = Guid.NewGuid();
        var projId = Guid.NewGuid();
        
        var loginReq = new LoginRequestDto("email@test.com", "senha");
        loginReq.Email.Should().Be("email@test.com");
        loginReq.Senha.Should().Be("senha");

        var loginResp = new LoginResponseDto("token", "Admin", "Nome", "email@test.com", id, projId);
        loginResp.Token.Should().Be("token");
        loginResp.Perfil.Should().Be("Admin");
        loginResp.Nome.Should().Be("Nome");
        loginResp.Email.Should().Be("email@test.com");
        loginResp.Id.Should().Be(id);
        loginResp.ProjetoId.Should().Be(projId);

        var regReq = new RegisterRequestDto("Nome", "email@test.com", "senha", "Admin");
        regReq.Nome.Should().Be("Nome");
        regReq.Email.Should().Be("email@test.com");
        regReq.Senha.Should().Be("senha");
        regReq.Role.Should().Be("Admin");
    }
}
