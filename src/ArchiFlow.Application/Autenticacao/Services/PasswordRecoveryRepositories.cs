using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Usuarios;

namespace ArchiFlow.Application.Autenticacao.Services;

public record PasswordRecoveryRepositories(
    IUsuarioRepository UsuarioRepository,
    IRepresentanteClienteRepository RepresentanteRepository,
    IClienteRepository ClienteRepository,
    ITokenRecuperacaoSenhaRepository TokenRepository);
