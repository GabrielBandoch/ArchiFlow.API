using ArchiFlow.Application.Chat.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Chat;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Chat.Services;

public class MensagemChatService : IMensagemChatService
{
    private readonly IMensagemChatRepository _mensagemRepository;
    private readonly IProjetoRepository _projetoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MensagemChatService(
        IMensagemChatRepository mensagemRepository,
        IProjetoRepository projetoRepository,
        IUnitOfWork unitOfWork)
    {
        _mensagemRepository = mensagemRepository;
        _projetoRepository = projetoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<MensagemChatDto>> GetByProjetoId(Guid projetoId, int take = 50)
    {
        var mensagens = await _mensagemRepository.GetByProjetoId(projetoId, take);
        return mensagens.Select(m => new MensagemChatDto(
            m.Id,
            m.ProjetoId,
            m.RemetenteId,
            m.RemetenteNome,
            m.RemetentePerfil,
            m.Conteudo,
            m.CriadoEm,
            m.Lida
        ));
    }

    public async Task<MensagemChatDto> EnviarMensagem(Guid projetoId, Guid remetenteId, string remetenteNome, string remetentePerfil, string conteudo)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
        {
            throw new ArgumentException("O conteúdo da mensagem não pode ser vazio.");
        }

        var projeto = await _projetoRepository.GetById(projetoId)
            ?? throw new KeyNotFoundException($"Projeto {projetoId} não encontrado.");

        var mensagem = new MensagemChat
        {
            Id = Guid.NewGuid(),
            ProjetoId = projetoId,
            RemetenteId = remetenteId,
            RemetenteNome = remetenteNome,
            RemetentePerfil = remetentePerfil,
            Conteudo = conteudo.Trim(),
            CriadoEm = DateTime.UtcNow,
            Lida = false
        };

        await _mensagemRepository.Create(mensagem);
        await _unitOfWork.Commit();

        return new MensagemChatDto(
            mensagem.Id,
            mensagem.ProjetoId,
            mensagem.RemetenteId,
            mensagem.RemetenteNome,
            mensagem.RemetentePerfil,
            mensagem.Conteudo,
            mensagem.CriadoEm,
            mensagem.Lida
        );
    }

    public async Task MarcarComoLidas(Guid projetoId, Guid usuarioId)
    {
        await _mensagemRepository.MarcarComoLidas(projetoId, usuarioId);
        await _unitOfWork.Commit();
    }
}
