using System.Threading.Tasks;

namespace ArchiFlow.Domain.Usuarios;

public interface ITokenRecuperacaoSenhaRepository
{
    Task AdicionarAsync(TokenRecuperacaoSenha token);
    Task<TokenRecuperacaoSenha?> ObterPorTokenHashAsync(string tokenHash);
    Task InvalidarParaEmailAsync(string email);
}
