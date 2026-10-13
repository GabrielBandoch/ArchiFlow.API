using ArchiFlow.Application.Common;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.Usuarios.DTOs;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Domain.Usuarios;
using ArchiFlow.Domain.Assinaturas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Usuarios.Services;

public class AuthService : IAuthService
{
    private static readonly ConcurrentDictionary<string, (string Code, DateTime Expiry)> _recoveryCodes = new();

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IProjetoRepository _projetoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;
    private readonly ITenantContext? _tenantContext;
    private readonly ITenantAssinaturaRepository? _tenantAssinaturaRepository;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IClienteRepository clienteRepository,
        IProjetoRepository projetoRepository,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IEmailService? emailService = null,
        IHttpContextAccessor? httpContextAccessor = null,
        ILogger<AuthService>? logger = null,
        ITenantContext? tenantContext = null,
        ITenantAssinaturaRepository? tenantAssinaturaRepository = null)
    {
        _usuarioRepository = usuarioRepository;
        _clienteRepository = clienteRepository;
        _projetoRepository = projetoRepository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _emailService = emailService!;
        _httpContextAccessor = httpContextAccessor!;
        _logger = logger!;
        _tenantContext = tenantContext;
        _tenantAssinaturaRepository = tenantAssinaturaRepository;
    }

    public async Task<LoginResponseDto> Login(LoginRequestDto request)
    {
        // Se o tenant estiver suspenso por inadimplência, bloqueia o acesso
        if (_tenantContext != null && _tenantAssinaturaRepository != null &&
            !string.IsNullOrWhiteSpace(_tenantContext.TenantId) &&
            !_tenantContext.TenantId.Equals("default", StringComparison.OrdinalIgnoreCase) &&
            !_tenantContext.TenantId.Equals("staging", StringComparison.OrdinalIgnoreCase))
        {
            var assinatura = await _tenantAssinaturaRepository.ObterPorSubdominioAsync(_tenantContext.TenantId);
            if (assinatura != null && assinatura.BloqueadoPorInadimplencia)
            {
                throw new UnauthorizedAccessException(
                    assinatura.MotivoBloqueio ?? "O acesso a este escritório está temporariamente suspenso por pendência financeira na assinatura COTU. Entre em contato com a equipe COTU para regularização.");
            }
        }
        var usuario = await _usuarioRepository.GetByEmail(request.Email);
        if (usuario != null)
        {
            if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash) || !usuario.Ativo)
            {
                throw new UnauthorizedAccessException("Credenciais inválidas ou usuário inativo.");
            }

            var token = _tokenService.GenerateToken(usuario);
            return new LoginResponseDto(token, usuario.Role, usuario.Nome, usuario.Email, usuario.Id);
        }

        var cliente = await _clienteRepository.GetByEmail(request.Email);
        if (cliente != null)
        {
            if (string.IsNullOrEmpty(cliente.SenhaPortal) || !BCrypt.Net.BCrypt.Verify(request.Senha, cliente.SenhaPortal) || !cliente.Ativo)
            {
                throw new UnauthorizedAccessException("Credenciais inválidas ou cliente inativo.");
            }

            var projetos = await _projetoRepository.GetByClienteId(cliente.Id);
            var projetoId = projetos.FirstOrDefault()?.Id;

            var token = _tokenService.GenerateToken(cliente, projetoId);
            return new LoginResponseDto(token, "Cliente", cliente.Nome, cliente.Email, cliente.Id, projetoId);
        }

        throw new UnauthorizedAccessException("Credenciais inválidas.");
    }

    public async Task<Guid> Registrar(RegisterRequestDto request)
    {
        var existente = await _usuarioRepository.GetByEmail(request.Email);
        if (existente != null)
        {
            throw new InvalidOperationException("E-mail já cadastrado.");
        }

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Email = request.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha, workFactor: 12),
            Role = request.Role,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        await _usuarioRepository.Create(usuario);
        await _unitOfWork.Commit();
        return usuario.Id;
    }

    public async Task SolicitarRecuperacaoSenhaAsync(SolicitarRecuperacaoSenhaDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email)) return;

        var email = request.Email.Trim().ToLowerInvariant();
        string nomeDestinatario = string.Empty;
        bool encontrado = false;

        var usuario = await _usuarioRepository.GetByEmail(email);
        if (usuario != null && usuario.Ativo)
        {
            encontrado = true;
            nomeDestinatario = usuario.Nome;
        }
        else
        {
            var cliente = await _clienteRepository.GetByEmail(email);
            if (cliente != null && cliente.Ativo)
            {
                encontrado = true;
                nomeDestinatario = cliente.Nome;
            }
        }

        if (!encontrado)
        {
            // Para segurança contra enumeração de e-mails, não lançamos exceção se o e-mail não existir
            return;
        }

        var codigo = Random.Shared.Next(100000, 999999).ToString();
        var expiracao = DateTime.UtcNow.AddMinutes(15);
        _recoveryCodes[email] = (codigo, expiracao);

        try
        {
            var req = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = req != null ? $"{req.Scheme}://{req.Host}" : "https://staging.cotu.com.br";
            var resetUrl = $"{baseUrl}/login?recuperar=1&email={Uri.EscapeDataString(email)}&token={codigo}";

            var html = EmailTemplates.GerarEmailRecuperacaoSenha(nomeDestinatario, codigo, resetUrl);
            await _emailService.SendEmailAsync(
                email,
                "Recuperação de Senha - COTU",
                html
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao enviar e-mail de recuperação de senha para {Email}", email);
        }
    }

    public async Task RedefinirSenhaAsync(RedefinirSenhaDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("E-mail é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Token))
            throw new ArgumentException("Código de verificação é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.NovaSenha) || request.NovaSenha.Length < 6)
            throw new ArgumentException("A nova senha deve ter no mínimo 6 caracteres.");

        var email = request.Email.Trim().ToLowerInvariant();
        var tokenInformado = request.Token.Trim();

        if (!_recoveryCodes.TryGetValue(email, out var entry) ||
            entry.Expiry < DateTime.UtcNow ||
            !string.Equals(entry.Code, tokenInformado, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Código de recuperação inválido ou expirado. Por favor, solicite um novo código.");
        }

        var novaSenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha, workFactor: 12);
        bool atualizado = false;

        var usuario = await _usuarioRepository.GetByEmail(email);
        if (usuario != null)
        {
            usuario.SenhaHash = novaSenhaHash;
            usuario.AtualizadoEm = DateTime.UtcNow;
            await _usuarioRepository.Update(usuario);
            atualizado = true;
        }
        else
        {
            var cliente = await _clienteRepository.GetByEmail(email);
            if (cliente != null)
            {
                cliente.SenhaPortal = novaSenhaHash;
                await _clienteRepository.Update(cliente);
                atualizado = true;
            }
        }

        if (!atualizado)
        {
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        _recoveryCodes.TryRemove(email, out _);
        await _unitOfWork.Commit();
    }
}
