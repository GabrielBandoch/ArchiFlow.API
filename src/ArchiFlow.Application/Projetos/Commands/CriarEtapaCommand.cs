namespace ArchiFlow.Application.Projetos.Commands;

public record CriarEtapaCommand(
    Guid ProjetoId,
    string Nome,
    string Descricao,
    int Ordem
);
