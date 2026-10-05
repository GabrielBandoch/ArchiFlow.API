using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Usuarios;
using Microsoft.AspNetCore.Http;

namespace ArchiFlow.Infrastructure.Services;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUsuarioRepository _usuarioRepository;

    public UserContextService(
        IHttpContextAccessor httpContextAccessor,
        IUsuarioRepository usuarioRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<(Usuario Usuario, Guid EscritorioId)> ObterUsuarioContextoAsync()
    {
        var user = _httpContextAccessor?.HttpContext?.User;
        var claim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? user?.FindFirst("nameid")?.Value
                 ?? user?.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(claim) || !Guid.TryParse(claim, out var id) || id == Guid.Empty)
            throw new UnauthorizedAccessException("Usuário não autenticado ou identidade inválida.");

        var usuario = await _usuarioRepository.GetById(id);
        if (usuario is null)
            throw new UnauthorizedAccessException("Usuário autenticado não encontrado.");

        var escritorioId = usuario.EscritorioId ?? usuario.Id;
        return (usuario, escritorioId);
    }
}
