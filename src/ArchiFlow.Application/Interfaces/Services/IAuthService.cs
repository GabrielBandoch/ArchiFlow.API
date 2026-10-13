using ArchiFlow.Application.Usuarios.DTOs;

namespace ArchiFlow.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponseDto> Login(LoginRequestDto request);
    Task<Guid> Registrar(RegisterRequestDto request);
    Task SolicitarRecuperacaoSenhaAsync(SolicitarRecuperacaoSenhaDto request);
    Task RedefinirSenhaAsync(RedefinirSenhaDto request);
}
