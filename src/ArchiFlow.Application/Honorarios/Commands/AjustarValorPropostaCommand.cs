namespace ArchiFlow.Application.Honorarios.Commands;

public record AjustarValorPropostaCommand(
    decimal ValorFinalAjustado,
    string? Observacoes
);
