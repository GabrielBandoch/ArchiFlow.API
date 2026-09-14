using System;

namespace ArchiFlow.Application.Chat.DTOs;

public record MensagemChatDto(
    Guid Id,
    Guid ProjetoId,
    Guid RemetenteId,
    string RemetenteNome,
    string RemetentePerfil,
    string Conteudo,
    DateTime CriadoEm,
    bool Lida
);
