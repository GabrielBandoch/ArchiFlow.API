using ArchiFlow.Application.Interfaces.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchiFlow.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMensagemChatFacade _mensagemFacade;

    public ChatHub(IMensagemChatFacade mensagemFacade)
    {
        _mensagemFacade = mensagemFacade;
    }

    public async Task EntrarNoProjeto(string projetoIdStr)
    {
        if (Guid.TryParse(projetoIdStr, out var projetoId) && PossuiAcessoAoProjeto(projetoId))
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
        if (!Guid.TryParse(projetoIdStr, out var projetoId) || string.IsNullOrWhiteSpace(conteudo) || !PossuiAcessoAoProjeto(projetoId))
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

    private bool PossuiAcessoAoProjeto(Guid projetoId)
    {
        var user = Context.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var userType = user.FindFirst("user_type")?.Value;
        if (userType == "staff")
        {
            return true;
        }

        var role = user.FindFirst(ClaimTypes.Role)?.Value
                ?? user.FindFirst("role")?.Value;

        if (role == "Administrador" || role == "Gerente" || role == "Colaborador" || role == "Arquiteto")
        {
            return true;
        }

        if (userType == "client" || role == "Cliente")
        {
            var claimProjetoId = user.FindFirst("projeto_id")?.Value
                              ?? user.FindFirst("projetoId")?.Value;

            return Guid.TryParse(claimProjetoId, out var userProjetoId) && userProjetoId == projetoId;
        }

        return false;
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
