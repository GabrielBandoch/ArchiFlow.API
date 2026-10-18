using System.Threading.Tasks;
using ArchiFlow.Application.Autenticacao.DTOs;

namespace ArchiFlow.Application.Interfaces.Services;

public interface IPasswordRecoveryService
{
    Task SolicitarRecuperacaoAsync(string email, string? baseUrl = null);
    Task<ResultadoValidacaoTokenDto> ValidarTokenAsync(string token);
    Task RedefinirSenhaAsync(string token, string novaSenha);
}
