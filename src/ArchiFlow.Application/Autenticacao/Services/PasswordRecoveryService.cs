using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ArchiFlow.Application.Autenticacao.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Domain.Usuarios;
using Microsoft.Extensions.Logging;

namespace ArchiFlow.Application.Autenticacao.Services;

public class PasswordRecoveryService : IPasswordRecoveryService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRepresentanteClienteRepository _representanteRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ITokenRecuperacaoSenhaRepository _tokenRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PasswordRecoveryService> _logger;

    public PasswordRecoveryService(
        IUsuarioRepository usuarioRepository,
        IRepresentanteClienteRepository representanteRepository,
        IClienteRepository clienteRepository,
        ITokenRecuperacaoSenhaRepository tokenRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<PasswordRecoveryService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _representanteRepository = representanteRepository;
        _clienteRepository = clienteRepository;
        _tokenRepository = tokenRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SolicitarRecuperacaoAsync(string email, string? baseUrl = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return;

        var emailNormalizado = email.Trim().ToLowerInvariant();

        // 1. Identify user type without exposing existence
        var usuario = await _usuarioRepository.GetByEmail(emailNormalizado);
        var representante = usuario == null ? await _representanteRepository.ObterPorEmailAsync(emailNormalizado) : null;
        var cliente = (usuario == null && representante == null) ? await _clienteRepository.GetByEmail(emailNormalizado) : null;

        if (usuario == null && representante == null && cliente == null)
        {
            // Account enumeration prevention: silently return
            _logger.LogInformation("Solicitação de recuperação de senha para e-mail não cadastrado: {Email}", emailNormalizado);
            return;
        }

        var tipoUsuario = usuario != null ? "staff" : "client";
        var nomeDestinatario = usuario?.Nome ?? representante?.Nome ?? cliente?.Nome ?? "Usuário";

        // 2. Invalidate previous tokens
        await _tokenRepository.InvalidarParaEmailAsync(emailNormalizado);

        // 3. Generate secure token & hash
        var rawTokenBytes = RandomNumberGenerator.GetBytes(32);
        var rawToken = Convert.ToHexString(rawTokenBytes).ToLowerInvariant();
        var tokenHash = ComputeSha256(rawToken);

        var tokenEntity = new TokenRecuperacaoSenha
        {
            Id = Guid.NewGuid(),
            Email = emailNormalizado,
            TokenHash = tokenHash,
            TipoUsuario = tipoUsuario,
            CriadoEm = DateTime.UtcNow,
            ExpiraEm = DateTime.UtcNow.AddHours(2),
            Tentativas = 0
        };

        await _tokenRepository.AdicionarAsync(tokenEntity);

        // 4. Construct recovery URL
        var domain = baseUrl?.TrimEnd('/');
        if (string.IsNullOrEmpty(domain))
        {
            var envDomain = Environment.GetEnvironmentVariable("DOMAIN_NAME") ?? "cotu.com.br";
            var scheme = envDomain.Contains("localhost") ? "http" : "https";
            domain = $"{scheme}://{envDomain}";
        }
        var recoveryUrl = $"{domain}/redefinir-senha?token={rawToken}";

        // 5. Send COTU-branded email
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head><meta charset=""UTF-8""></head>
<body style=""margin: 0; padding: 20px; background-color: #f8fafc; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;"">
  <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);"">
    <tr>
      <td style=""background-color: #1f1f1f; padding: 24px 32px; text-align: left;"">
        <h1 style=""margin: 0; color: #ffffff; font-size: 22px; font-weight: 700; letter-spacing: -0.5px;""><span style=""color: #d8894d;"">■</span> COTU</h1>
      </td>
    </tr>
    <tr>
      <td style=""padding: 32px; color: #334155; font-size: 15px; line-height: 1.6;"">
        <h2 style=""margin-top: 0; color: #0f172a; font-size: 18px;"">Recuperação de Senha</h2>
        <p>Olá, <strong>{nomeDestinatario}</strong>,</p>
        <p>Recebemos uma solicitação para redefinir a senha da sua conta no COTU.</p>
        <p>Clique no botão abaixo para definir sua nova senha. Este link é válido por <strong>2 horas</strong> e pode ser usado apenas uma vez.</p>
        <div style=""text-align: center; margin: 32px 0;"">
          <a href=""{recoveryUrl}"" style=""background-color: #765538; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: 600; display: inline-block; font-size: 14px;"">Redefinir Minha Senha</a>
        </div>
        <p style=""font-size: 13px; color: #64748b;"">Se você não realizou esta solicitação, nenhuma ação é necessária; sua conta permanece em segurança.</p>
      </td>
    </tr>
    <tr>
      <td style=""background-color: #f1f5f9; padding: 16px 32px; text-align: center; color: #64748b; font-size: 12px;"">
        © {DateTime.UtcNow.Year} COTU Technologies — Segurança e Gestão Integrada
      </td>
    </tr>
  </table>
</body>
</html>";

        try
        {
            await _emailService.SendEmailAsync(
                emailNormalizado,
                "Recuperação de Senha — COTU",
                htmlBody
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao enviar e-mail de recuperação para {Email}", emailNormalizado);
        }
    }

    public async Task<ResultadoValidacaoTokenDto> ValidarTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new ResultadoValidacaoTokenDto(false, null, "Token não informado.");
        }

        var tokenHash = ComputeSha256(token.Trim());
        var tokenEntity = await _tokenRepository.ObterPorTokenHashAsync(tokenHash);

        if (tokenEntity == null)
        {
            return new ResultadoValidacaoTokenDto(false, null, "Link de recuperação inválido.");
        }

        if (tokenEntity.UsadoEm.HasValue)
        {
            return new ResultadoValidacaoTokenDto(false, null, "Este link de recuperação já foi utilizado.");
        }

        if (tokenEntity.ExpiraEm <= DateTime.UtcNow)
        {
            return new ResultadoValidacaoTokenDto(false, null, "Este link de recuperação expirou. Por favor, solicite um novo.");
        }

        return new ResultadoValidacaoTokenDto(true, tokenEntity.Email, "Token válido.");
    }

    public async Task RedefinirSenhaAsync(string token, string novaSenha)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token é obrigatório.");

        if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Length < 6)
            throw new ArgumentException("A nova senha deve possuir no mínimo 6 caracteres.");

        var tokenHash = ComputeSha256(token.Trim());
        var tokenEntity = await _tokenRepository.ObterPorTokenHashAsync(tokenHash)
            ?? throw new InvalidOperationException("Link de recuperação inválido ou inexistente.");

        if (!tokenEntity.IsValido())
        {
            throw new InvalidOperationException("Este link de recuperação já foi utilizado ou expirou.");
        }

        var novaSenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha, workFactor: 12);

        if (string.Equals(tokenEntity.TipoUsuario, "staff", StringComparison.OrdinalIgnoreCase))
        {
            var usuario = await _usuarioRepository.GetByEmail(tokenEntity.Email)
                ?? throw new InvalidOperationException("Usuário associado ao token não encontrado.");

            usuario.SenhaHash = novaSenhaHash;
            usuario.AtualizadoEm = DateTime.UtcNow;
            await _usuarioRepository.Update(usuario);
        }
        else
        {
            var representante = await _representanteRepository.ObterPorEmailAsync(tokenEntity.Email);
            if (representante != null)
            {
                representante.SenhaHash = novaSenhaHash;
                representante.AtualizadoEm = DateTime.UtcNow;
                await _representanteRepository.AtualizarAsync(representante);
            }

            var cliente = await _clienteRepository.GetByEmail(tokenEntity.Email);
            if (cliente != null)
            {
                cliente.SenhaPortal = novaSenhaHash;
                await _clienteRepository.Update(cliente);
            }
        }

        tokenEntity.UsadoEm = DateTime.UtcNow;
        await _unitOfWork.Commit();

        _logger.LogInformation("Senha redefinida com sucesso para o e-mail: {Email}", tokenEntity.Email);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
