using ArchiFlow.Application.Chat.Commands;
using ArchiFlow.Application.Chat.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Chat;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Shared;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Chat.Services;

public class MensagemChatService : IMensagemChatService
{
    private readonly IMensagemChatRepository _mensagemRepository;
    private readonly IProjetoRepository _projetoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public MensagemChatService(
        IMensagemChatRepository mensagemRepository,
        IProjetoRepository projetoRepository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _mensagemRepository = mensagemRepository;
        _projetoRepository = projetoRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<MensagemChatDto>> GetByProjetoId(Guid projetoId, int take = 50)
    {
        var mensagens = await _mensagemRepository.GetByProjetoId(projetoId, take);

        var user = _httpContextAccessor?.HttpContext?.User;
        var usuarioIdStr = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? user?.FindFirst("nameid")?.Value
                        ?? user?.FindFirst("sub")?.Value;

        if (Guid.TryParse(usuarioIdStr, out var usuarioId))
        {
            await MarcarComoLidas(projetoId, usuarioId);
        }

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

    public async Task<MensagemChatDto> EnviarMensagem(Guid projetoId, EnviarMensagemCommand command)
    {
        var user = _httpContextAccessor?.HttpContext?.User;
        var usuarioIdStr = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? user?.FindFirst("nameid")?.Value
                        ?? user?.FindFirst("sub")?.Value;

        var usuarioId = Guid.TryParse(usuarioIdStr, out var id) ? id : Guid.NewGuid();

        var usuarioNome = user?.FindFirst(ClaimTypes.Name)?.Value
                       ?? user?.FindFirst("name")?.Value
                       ?? user?.FindFirst("unique_name")?.Value
                       ?? user?.FindFirst(ClaimTypes.Email)?.Value
                       ?? "Usuário";

        var role = user?.FindFirst(ClaimTypes.Role)?.Value
                ?? user?.FindFirst("role")?.Value;

        var userType = user?.FindFirst("user_type")?.Value;
        var perfil = (userType == "client" || role == "Cliente") ? "Cliente" : (role ?? "Arquiteto");

        return await EnviarMensagem(projetoId, usuarioId, usuarioNome, perfil, command.Conteudo);
    }

    public async Task<MensagemChatDto> EnviarMensagem(Guid projetoId, Guid remetenteId, string remetenteNome, string remetentePerfil, string conteudo)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
        {
            throw new ArgumentException("O conteúdo da mensagem não pode ser vazio.");
        }

        if (conteudo.Trim().Length > 2000)
        {
            throw new ArgumentException("O conteúdo da mensagem não pode exceder 2000 caracteres.");
        }

        _ = await _projetoRepository.GetById(projetoId)
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
