using System;
using System.Linq;
using System.Threading.Tasks;
using ArchiFlow.Domain.Agenda;
using ArchiFlow.Infrastructure.Repositories.Agenda;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using Xunit;

namespace ArchiFlow.Tests.Agenda;

public class AgendaRepositoryTests
{
    [Fact]
    public async Task CompromissoRepository_ObterPorPeriodoAsync_DeveFiltrarPorPeriodoEIds()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new CompromissoRepository(context);

        var escritorioId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var projetoId = Guid.NewGuid();

        var c1 = new Compromisso
        {
            Id = Guid.NewGuid(),
            EscritorioId = escritorioId,
            UsuarioId = usuarioId,
            ProjetoId = projetoId,
            Titulo = "Compromisso 1",
            DataHoraInicio = DateTime.UtcNow.Date.AddHours(10),
            DataHoraFim = DateTime.UtcNow.Date.AddHours(11),
            Status = StatusCompromisso.Agendado
        };

        var c2 = new Compromisso
        {
            Id = Guid.NewGuid(),
            EscritorioId = escritorioId,
            UsuarioId = Guid.NewGuid(), // Outro usuario
            ProjetoId = projetoId,
            Titulo = "Compromisso 2",
            DataHoraInicio = DateTime.UtcNow.Date.AddHours(14),
            DataHoraFim = DateTime.UtcNow.Date.AddHours(15),
            Status = StatusCompromisso.Agendado
        };

        var c3 = new Compromisso
        {
            Id = Guid.NewGuid(),
            EscritorioId = Guid.NewGuid(), // Outro escritorio
            Titulo = "Compromisso Outro Escritorio",
            DataHoraInicio = DateTime.UtcNow.Date.AddHours(10),
            DataHoraFim = DateTime.UtcNow.Date.AddHours(11),
            Status = StatusCompromisso.Agendado
        };

        context.Compromissos.AddRange(c1, c2, c3);
        await context.SaveChangesAsync();

        var inicio = DateTime.UtcNow.Date;
        var fim = DateTime.UtcNow.Date.AddDays(1);

        // Sem filtros opcionais
        var todos = await repo.ObterPorPeriodoAsync(escritorioId, inicio, fim);
        todos.Should().HaveCount(2);

        // Filtrado por usuario
        var porUsuario = await repo.ObterPorPeriodoAsync(escritorioId, inicio, fim, usuarioId: usuarioId);
        porUsuario.Should().ContainSingle().Which.Titulo.Should().Be("Compromisso 1");

        // Filtrado por projeto
        var porProjeto = await repo.ObterPorPeriodoAsync(escritorioId, inicio, fim, projetoId: projetoId);
        porProjeto.Should().HaveCount(2);
    }

    [Fact]
    public async Task CompromissoRepository_ObterProximosAsync_DeveRetornarApenasFuturosNaoCancelados()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new CompromissoRepository(context);

        var escritorioId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        var cFuturo = new Compromisso
        {
            Id = Guid.NewGuid(),
            EscritorioId = escritorioId,
            UsuarioId = usuarioId,
            Titulo = "Reuniao Futura",
            DataHoraInicio = DateTime.UtcNow.AddHours(2),
            DataHoraFim = DateTime.UtcNow.AddHours(3),
            Status = StatusCompromisso.Agendado
        };

        var cCancelado = new Compromisso
        {
            Id = Guid.NewGuid(),
            EscritorioId = escritorioId,
            UsuarioId = usuarioId,
            Titulo = "Cancelado",
            DataHoraInicio = DateTime.UtcNow.AddHours(4),
            DataHoraFim = DateTime.UtcNow.AddHours(5),
            Status = StatusCompromisso.Cancelado
        };

        var cPassado = new Compromisso
        {
            Id = Guid.NewGuid(),
            EscritorioId = escritorioId,
            UsuarioId = usuarioId,
            Titulo = "Passado",
            DataHoraInicio = DateTime.UtcNow.AddDays(-2),
            DataHoraFim = DateTime.UtcNow.AddDays(-2).AddHours(1),
            Status = StatusCompromisso.Concluido
        };

        context.Compromissos.AddRange(cFuturo, cCancelado, cPassado);
        await context.SaveChangesAsync();

        var proximos = (await repo.ObterProximosAsync(escritorioId, 10, usuarioId)).ToList();
        proximos.Should().ContainSingle();
        proximos[0].Titulo.Should().Be("Reuniao Futura");
    }

    [Fact]
    public async Task ConfiguracaoAgendaRepository_ObterPorEscritorioIdAsync_DeveRetornarConfiguracaoCorreta()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new ConfiguracaoAgendaRepository(context);

        var escritorioId = Guid.NewGuid();
        var config = new ConfiguracaoAgendaEscritorio
        {
            Id = Guid.NewGuid(),
            EscritorioId = escritorioId,
            EmailAgendaEmpresa = "contato@arquiteto.com",
            GoogleCalendarId = "primary",
            GoogleOAuthEmail = "oauth@gmail.com",
            GoogleOAuthRefreshToken = "rt-secret",
            ChaveGoogleServiceAccountJson = "json-secret"
        };

        context.ConfiguracoesAgendaEscritorio.Add(config);
        await context.SaveChangesAsync();

        var obtido = await repo.ObterPorEscritorioIdAsync(escritorioId);
        obtido.Should().NotBeNull();
        obtido!.EscritorioId.Should().Be(escritorioId);
        obtido.EmailAgendaEmpresa.Should().Be("contato@arquiteto.com");

        var inexistente = await repo.ObterPorEscritorioIdAsync(Guid.NewGuid());
        inexistente.Should().BeNull();
    }
}
