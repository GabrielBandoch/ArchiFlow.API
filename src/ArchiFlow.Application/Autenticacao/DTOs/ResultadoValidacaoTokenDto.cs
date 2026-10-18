namespace ArchiFlow.Application.Autenticacao.DTOs;

public record ResultadoValidacaoTokenDto(
    bool Valido,
    string? Email = null,
    string? Mensagem = null
);
