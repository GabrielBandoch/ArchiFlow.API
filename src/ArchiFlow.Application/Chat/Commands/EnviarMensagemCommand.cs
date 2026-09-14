using System;

namespace ArchiFlow.Application.Chat.Commands;

public record EnviarMensagemCommand(
    Guid ProjetoId,
    string Conteudo
);
