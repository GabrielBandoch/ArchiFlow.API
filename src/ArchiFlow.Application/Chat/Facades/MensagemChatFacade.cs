using ArchiFlow.Application.Chat.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Chat.Facades;

public class MensagemChatFacade : IMensagemChatFacade
{
    private readonly IMensagemChatService _mensagemService;

    public MensagemChatFacade(IMensagemChatService mensagemService)
    {
        _mensagemService = mensagemService;
    }

    public Task<IEnumerable<MensagemChatDto>> GetByProjetoId(Guid projetoId, int take = 50)
    {
        return _mensagemService.GetByProjetoId(projetoId, take);
    }

    public Task<MensagemChatDto> EnviarMensagem(Guid projetoId, Guid remetenteId, string remetenteNome, string remetentePerfil, string conteudo)
    {
        return _mensagemService.EnviarMensagem(projetoId, remetenteId, remetenteNome, remetentePerfil, conteudo);
    }

    public Task MarcarComoLidas(Guid projetoId, Guid usuarioId)
    {
        return _mensagemService.MarcarComoLidas(projetoId, usuarioId);
    }
}
