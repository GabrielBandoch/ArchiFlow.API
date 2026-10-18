using ArchiFlow.API.Middleware;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Domain.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchiFlow.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMensagemChatFacade? _mensagemFacade;
    private readonly IServiceProvider? _serviceProvider;
    private readonly ITenantContext? _tenantContext;

    public ChatHub(IMensagemChatFacade mensagemFacade)
    {
        _mensagemFacade = mensagemFacade;
    }

    [ActivatorUtilitiesConstructor]
    public ChatHub(IServiceProvider serviceProvider, ITenantContext tenantContext)
    {
        _serviceProvider = serviceProvider;
        _tenantContext = tenantContext;
    }

    private IMensagemChatFacade GetFacade()
    {
        if (_mensagemFacade != null)
        {
            return _mensagemFacade;
        }

        if (_serviceProvider != null)
        {
            return _serviceProvider.GetRequiredService<IMensagemChatFacade>();
        }

        throw new InvalidOperationException("Nenhum serviço de mensagens configurado.");
    }

    private void EnsureTenantContext()
    {
        if (_tenantContext == null) return;

        var user = Context.User;
        var tenant = user?.FindFirst("tenant")?.Value;
        if (string.IsNullOrWhiteSpace(tenant))
        {
            tenant = user?.FindFirst("escritorio_id")?.Value
                  ?? user?.FindFirst("EscritorioId")?.Value;
        }

        var httpContext = Context.GetHttpContext();
        var host = httpContext?.Request?.Host.Host ?? "localhost";

        if (string.IsNullOrWhiteSpace(tenant) && httpContext != null)
        {
            tenant = TenantResolutionMiddleware.ResolveTenantFromHost(host);
        }

        if (!string.IsNullOrWhiteSpace(tenant))
        {
            _tenantContext.SetTenant(tenant, host);
        }
    }

    public async Task EntrarNoProjeto(string projetoIdStr)
    {
        EnsureTenantContext();
        if (Guid.TryParse(projetoIdStr, out var projetoId) && PossuiAcessoAoProjeto(projetoId))
        {
            var grupo = ObterNomeGrupo(projetoId);
            await Groups.AddToGroupAsync(Context.ConnectionId, grupo);

            var usuarioId = ObterUsuarioId();
            if (usuarioId.HasValue)
            {
                var facade = GetFacade();
                await facade.MarcarComoLidas(projetoId, usuarioId.Value);
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
        EnsureTenantContext();
        if (!Guid.TryParse(projetoIdStr, out var projetoId) || string.IsNullOrWhiteSpace(conteudo) || !PossuiAcessoAoProjeto(projetoId))
        {
            return;
        }

        var usuarioId = ObterUsuarioId() ?? Guid.NewGuid();
        var usuarioNome = ObterUsuarioNome() ?? "Usuário";
        var usuarioPerfil = ObterUsuarioPerfil() ?? "Colaborador";

        var facade = GetFacade();
        var mensagemDto = await facade.EnviarMensagem(
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
        if (string.Equals(userType, "staff", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var role = user.FindFirst(ClaimTypes.Role)?.Value
                ?? user.FindFirst("role")?.Value;

        if (!string.IsNullOrEmpty(role))
        {
            var allowedRoles = new[]
            {
                Roles.Administrador,
                Roles.Gerente,
                Roles.Colaborador,
                Roles.AdministradorGeral,
                Roles.ArquitetoAdmin,
                Roles.ArquitetoColaborador,
                Roles.Estagiario,
                Roles.Financeiro,
                "Administrador",
                "Gerente",
                "Colaborador",
                "Arquiteto",
                "AdministradorGeral"
            };

            if (allowedRoles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        if (string.Equals(userType, "client", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Cliente", StringComparison.OrdinalIgnoreCase))
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
