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

        var etapaTemplate = new Application.Projetos.DTOs.TemplateEtapaDto(id, id, "Briefing", "Desc", 1, new List<string> { "Reuniao" });
        etapaTemplate.Id.Should().Be(id);
        etapaTemplate.TemplateProjetoId.Should().Be(id);
        etapaTemplate.Nome.Should().Be("Briefing");
        etapaTemplate.Descricao.Should().Be("Desc");
        etapaTemplate.Ordem.Should().Be(1);
        etapaTemplate.Tarefas.Should().Contain("Reuniao");

        var templateDto = new Application.Projetos.DTOs.TemplateProjetoDto(id, "res", "Residencial", "Desc", "home", true, new List<Application.Projetos.DTOs.TemplateEtapaDto> { etapaTemplate });
        templateDto.Id.Should().Be(id);
        templateDto.Codigo.Should().Be("res");
        templateDto.Nome.Should().Be("Residencial");
        templateDto.Descricao.Should().Be("Desc");
        templateDto.Icone.Should().Be("home");
        templateDto.Ativo.Should().BeTrue();
        templateDto.Etapas.Should().HaveCount(1);

        var tarefaDto = new Application.Projetos.DTOs.TarefaEtapaDto(id, projId, "Tarefa 1", true, DateTime.UtcNow);
        tarefaDto.Id.Should().Be(id);
        tarefaDto.EtapaId.Should().Be(projId);
        tarefaDto.Titulo.Should().Be("Tarefa 1");
        tarefaDto.Concluida.Should().BeTrue();
        tarefaDto.CriadoEm.Should().NotBeNull();

        var etapaDto = new Application.Projetos.DTOs.EtapaProjetoDto(id, projId, "Etapa", "Desc", Domain.Projetos.Enum.StatusEtapa.EmAndamento, "EmAndamento", 1, null, new List<Application.Projetos.DTOs.TarefaEtapaDto> { tarefaDto });
        etapaDto.Id.Should().Be(id);
        etapaDto.ProjetoId.Should().Be(projId);
        etapaDto.Nome.Should().Be("Etapa");
        etapaDto.Status.Should().Be(Domain.Projetos.Enum.StatusEtapa.EmAndamento);
        etapaDto.StatusLabel.Should().Be("EmAndamento");
        etapaDto.Ordem.Should().Be(1);
        etapaDto.Tarefas.Should().HaveCount(1);

        var projetoDto = new Application.Projetos.DTOs.ProjetoDto(id, "Projeto", "Desc", Domain.Projetos.Enum.StatusProjeto.Desenvolvimento, "Desenvolvimento", Domain.Projetos.Enum.TipoProjeto.Residencial, "Residencial", DateTime.UtcNow, null, 200, projId, DateTime.UtcNow, null, new List<Application.Projetos.DTOs.EtapaProjetoDto> { etapaDto }, 50, "Cliente Teste");
        projetoDto.Id.Should().Be(id);
        projetoDto.Nome.Should().Be("Projeto");
        projetoDto.ClienteNome.Should().Be("Cliente Teste");
        projetoDto.ProgressoPercentual.Should().Be(50);
        projetoDto.MetragemTotal.Should().Be(200);

        var altCmd = new Application.Projetos.Commands.AlternarTarefaCommand(id);
        altCmd.TarefaId.Should().Be(id);
    }
}
