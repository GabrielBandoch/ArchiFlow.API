namespace ArchiFlow.Application.Clientes.DTOs;

public record ResultadoValidacaoConviteDto(
    bool Valido,
    string? Nome = null,
    string? Email = null,
    string? Mensagem = null
);

