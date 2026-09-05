using ArchiFlow.Application.Chat.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Interfaces.Facades;

public interface IMensagemChatFacade
{
    Task<IEnumerable<MensagemChatDto>> GetByProjetoId(Guid projetoId, int take = 50);
    Task<MensagemChatDto> EnviarMensagem(Guid projetoId, Guid remetenteId, string remetenteNome, string remetentePerfil, string conteudo);
    Task MarcarComoLidas(Guid projetoId, Guid usuarioId);
}
