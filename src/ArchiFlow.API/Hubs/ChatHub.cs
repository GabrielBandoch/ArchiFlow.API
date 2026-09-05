using ArchiFlow.Application.Interfaces.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchiFlow.API.Hubs;

[Authorize]
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class ChatHub : Hub
{
    private readonly IMensagemChatFacade _mensagemFacade;

    public ChatHub(IMensagemChatFacade mensagemFacade)
    {
        _mensagemFacade = mensagemFacade;
    }

    public async Task EntrarNoProjeto(string projetoIdStr)
    {
        if (Guid.TryParse(projetoIdStr, out var projetoId))
        {
            var grupo = ObterNomeGrupo(projetoId);
            await Groups.AddToGroupAsync(Context.ConnectionId, grupo);

            var usuarioId = ObterUsuarioId();
            if (usuarioId.HasValue)
            {
                await _mensagemFacade.MarcarComoLidas(projetoId, usuarioId.Value);
            }
        }
    }

    public async Task SairDoProjeto(string projetoIdStr)
    {
        if (Guid.TryParse(projetoIdStr, out var projetoId))
        {
            var grupo = ObterNomeGrupo(projetoId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, grupo);
        }
    }

    public async Task EnviarMensagem(string projetoIdStr, string conteudo)
    {
        if (!Guid.TryParse(projetoIdStr, out var projetoId) || string.IsNullOrWhiteSpace(conteudo))
        {
            return;
        }

        var usuarioId = ObterUsuarioId() ?? Guid.NewGuid();
        var usuarioNome = ObterUsuarioNome() ?? "Usuário";
        var usuarioPerfil = ObterUsuarioPerfil() ?? "Colaborador";

        var mensagemDto = await _mensagemFacade.EnviarMensagem(
            projetoId,
            usuarioId,
            usuarioNome,
            usuarioPerfil,
            conteudo
        );

        var grupo = ObterNomeGrupo(projetoId);
        await Clients.Group(grupo).SendAsync("ReceiveMessage", mensagemDto);
    }

    private static string ObterNomeGrupo(Guid projetoId) => $"projeto_{projetoId}";

    private Guid? ObterUsuarioId()
    {
        var idClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? Context.User?.FindFirst("nameid")?.Value
                   ?? Context.User?.FindFirst("sub")?.Value;

        return Guid.TryParse(idClaim, out var id) ? id : null;
    }

    private string? ObterUsuarioNome()
    {
        return Context.User?.FindFirst(ClaimTypes.Name)?.Value
            ?? Context.User?.FindFirst("name")?.Value
            ?? Context.User?.FindFirst("unique_name")?.Value
            ?? Context.User?.FindFirst(ClaimTypes.Email)?.Value
            ?? "Usuário";
    }

    private string? ObterUsuarioPerfil()
    {
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value
                ?? Context.User?.FindFirst("role")?.Value;

        var userType = Context.User?.FindFirst("user_type")?.Value;
        if (userType == "client" || role == "Cliente")
        {
            return "Cliente";
        }

        return role ?? "Arquiteto";
    }
}
