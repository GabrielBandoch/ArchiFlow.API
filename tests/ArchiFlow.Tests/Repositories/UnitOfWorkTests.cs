using ArchiFlow.Infrastructure.Repositories;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using Xunit;

namespace ArchiFlow.Tests.Repositories;

public class UnitOfWorkTests
{
    [Fact]
    public async Task Commit_DeveSalvarAlteracoes()
    {
        using var context = TestDbContextFactory.Create();
        using var uow = new UnitOfWork(context);

        var projeto = new Domain.Projetos.Projeto
        {
            Id = Guid.NewGuid(),
            Nome = "P1",
            Descricao = "Desc",
            Tipo = Domain.Projetos.Enum.TipoProjeto.Residencial,
            DataInicio = DateTime.UtcNow,
            MetragemTotal = 100,
            ClienteId = Guid.NewGuid()
        };
        context.Projetos.Add(projeto);

        var result = await uow.Commit();

        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Dispose_DeveSerExecutadoSemErros()
    {
        var context = TestDbContextFactory.Create();
        var uow = new UnitOfWork(context);

        uow.Dispose();

        // Verificar que dispose pode ser chamado multiplas vezes sem estourar erro
        var act = () => uow.Dispose();
        act.Should().NotThrow();
    }
}
