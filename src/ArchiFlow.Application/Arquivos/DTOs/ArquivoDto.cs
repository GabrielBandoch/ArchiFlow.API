using System;

namespace ArchiFlow.Application.Arquivos.DTOs;

public record ArquivoDto(
    Guid Id,
    Guid ProjetoId,
    string Nome,
    string UrlStorage,
    string? Tipo,
    bool VisivelCliente,
    DateTime CriadoEm
);
