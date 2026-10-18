using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.Security;
using ArchiFlow.Application.Usuarios.Commands;
using ArchiFlow.Application.Usuarios.DTOs;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Domain.Usuarios;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ArchiFlow.Application.Usuarios.Services;

public class UsuarioService : IUsuarioService
{
    private const string MensagemSemPermissaoGerenciarMembro = "Você não possui permissão para gerenciar este membro.";
    private const string MensagemUsuarioNaoEncontrado = "Usuário não encontrado.";
    private static readonly char[] ModuleSeparators = [',', ';'];

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IStorageService _storageService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UsuarioService> logger)
        : this(usuarioRepository, unitOfWork, emailService, null, httpContextAccessor, logger)
    {
    }

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IStorageService? storageService = null,
        IHttpContextAccessor? httpContextAccessor = null,
        ILogger<UsuarioService>? logger = null)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _storageService = storageService!;
        _httpContextAccessor = httpContextAccessor!;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<UsuarioService>.Instance;
    }

    public async Task<IEnumerable<MembroEquipeDto>> ObterEquipeAsync()
    {
        var (_, escritorioId) = await ObterUsuarioContextoAsync();
        var membros = await _usuarioRepository.ObterPorEscritorioIdAsync(escritorioId);
        return membros.Where(m => !m.IsContaSuporte()).Select(MapearParaDto);
    }

    public async Task<MembroEquipeDto?> ObterMembroPorIdAsync(Guid id)
    {
        var (_, escritorioId) = await ObterUsuarioContextoAsync();
        var membro = await _usuarioRepository.GetById(id);
        if (membro is null || membro.IsContaSuporte()) return null;

        var membroEscritorioId = membro.EscritorioId ?? membro.Id;
        if (membroEscritorioId != escritorioId) return null;

        return MapearParaDto(membro);
    }

    public async Task<MembroEquipeDto> ConvidarMembroAsync(ConvidarMembroEquipeCommand command)
    {
        ValidarComandoConvite(command);

        var emailNormalizado = command.Email.Trim().ToLowerInvariant();
        var (usuarioAtual, escritorioId) = await ObterUsuarioContextoAsync();

        var existente = await _usuarioRepository.GetByEmail(emailNormalizado);
        if (existente is not null)
            throw new InvalidOperationException("Já existe um usuário cadastrado com este e-mail.");

        var senha = !string.IsNullOrWhiteSpace(command.SenhaTemporaria) && command.SenhaTemporaria.Length >= 6
            ? command.SenhaTemporaria
            : GerarSenhaTemporaria();

        var modulos = DeterminarModulosIniciais(command.Role, command.ModulosPermitidos);

        var novoUsuario = new Usuario
        {
            Id = Guid.NewGuid(),
            EscritorioId = escritorioId,
            Nome = command.Nome.Trim(),
            Email = emailNormalizado,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 12),
            Role = command.Role,
            Cargo = command.Cargo?.Trim(),
            Telefone = command.Telefone?.Trim(),
            ModulosPermitidos = modulos,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        if (novoUsuario.IsContaSuporte())
            throw new InvalidOperationException("Não é permitido convidar a conta interna de suporte.");

        await _usuarioRepository.Create(novoUsuario);
        await _unitOfWork.Commit();

        await EnviarEmailConviteAsync(novoUsuario, usuarioAtual, senha);

        return MapearParaDto(novoUsuario);
    }

    private static void ValidarComandoConvite(ConvidarMembroEquipeCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new ArgumentException("Nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(command.Email) || !new EmailAddressAttribute().IsValid(command.Email.Trim()))
            throw new ArgumentException("E-mail em formato inválido.");

        if (string.IsNullOrWhiteSpace(command.Role) || !Roles.RolesValidasEquipe.Contains(command.Role))
            throw new ArgumentException($"Função '{command.Role}' inválida.");

        var emailNormalizado = command.Email.Trim().ToLowerInvariant();
        if (string.Equals(emailNormalizado, "suporte.cotu@cotu.com.br", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(emailNormalizado, "admin@cotu.com", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Não é permitido convidar a conta interna de suporte.");
        }
    }

    private static string? DeterminarModulosIniciais(string role, IReadOnlyCollection<string>? modulosPermitidos)
    {
        if (modulosPermitidos != null && modulosPermitidos.Count > 0)
        {
            return string.Join(",", modulosPermitidos.Distinct(StringComparer.OrdinalIgnoreCase));
        }

        var isAdminRole = string.Equals(role, Roles.Administrador, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(role, Roles.ArquitetoAdmin, StringComparison.OrdinalIgnoreCase);

        return isAdminRole ? string.Join(",", ModulosApp.Todos) : null;
    }

    private async Task EnviarEmailConviteAsync(Usuario novoUsuario, Usuario usuarioAtual, string senha)
    {
        try
        {
            await _emailService.SendEmailAsync(
                novoUsuario.Email,
                "Convite para a equipe COTU",
                $"Olá {novoUsuario.Nome},\n\nVocê foi convidado para a equipe no COTU por {usuarioAtual.Nome}.\n\nSeu acesso:\nE-mail: {novoUsuario.Email}\nSenha temporária: {senha}\n\nRecomendamos alterar sua senha após o primeiro acesso."
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao enviar e-mail de convite para {Email}", novoUsuario.Email);
        }
    }

    public async Task<MembroEquipeDto> AtualizarMembroAsync(Guid id, AtualizarMembroEquipeCommand command)
    {
        ValidarComandoAtualizacao(command);

        var (usuarioAtual, escritorioId) = await ObterUsuarioContextoAsync();
        var membro = await ObterMembroParaGerenciamentoAsync(id, escritorioId);

        ValidarNaoRemocaoProprioAdmin(membro, usuarioAtual, command.Role);

        if (!string.IsNullOrWhiteSpace(command.Email))
        {
            await ValidarEAtualizarEmailMembroAsync(membro, command.Email);
        }

        AplicarDadosAtualizadosMembro(membro, command);

        await _usuarioRepository.Update(membro);
        await _unitOfWork.Commit();

        return MapearParaDto(membro);
    }

    private static void ValidarComandoAtualizacao(AtualizarMembroEquipeCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new ArgumentException("Nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(command.Role) || !Roles.RolesValidasEquipe.Contains(command.Role))
            throw new ArgumentException($"Função '{command.Role}' inválida.");
    }

    private async Task<Usuario> ObterMembroParaGerenciamentoAsync(Guid id, Guid escritorioId)
    {
        var membro = await _usuarioRepository.GetById(id);
        if (membro is null || membro.IsContaSuporte())
            throw new KeyNotFoundException($"Membro com Id {id} não encontrado.");

        var membroEscritorioId = membro.EscritorioId ?? membro.Id;
        if (membroEscritorioId != escritorioId)
            throw new UnauthorizedAccessException(MensagemSemPermissaoGerenciarMembro);

        return membro;
    }

    private static void ValidarNaoRemocaoProprioAdmin(Usuario membro, Usuario usuarioAtual, string novaRole)
    {
        var isProprioUsuario = membro.Id == usuarioAtual.Id;
        var isRoleAdmin = string.Equals(novaRole, Roles.Administrador, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(novaRole, Roles.ArquitetoAdmin, StringComparison.OrdinalIgnoreCase);

        if (isProprioUsuario && membro.IsAdmin() && !isRoleAdmin)
        {
            throw new InvalidOperationException("Não é permitido remover a sua própria função de administrador.");
        }
    }

    private async Task ValidarEAtualizarEmailMembroAsync(Usuario membro, string novoEmail)
    {
        var emailNormalizado = novoEmail.Trim().ToLowerInvariant();
        if (!new EmailAddressAttribute().IsValid(emailNormalizado))
            throw new ArgumentException("E-mail em formato inválido.");

        if (string.Equals(emailNormalizado, "suporte.cotu@cotu.com.br", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(emailNormalizado, "admin@cotu.com", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Não é permitido atribuir o e-mail da conta interna de suporte.");
        }

        if (!string.Equals(emailNormalizado, membro.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existente = await _usuarioRepository.GetByEmail(emailNormalizado);
            if (existente is not null && existente.Id != membro.Id)
                throw new InvalidOperationException("Já existe outro usuário cadastrado com este e-mail.");

            membro.Email = emailNormalizado;
        }
    }

    private static void AplicarDadosAtualizadosMembro(Usuario membro, AtualizarMembroEquipeCommand command)
    {
        membro.Nome = command.Nome.Trim();
        membro.Role = command.Role;
        membro.Cargo = command.Cargo?.Trim();
        membro.Telefone = command.Telefone?.Trim();

        if (command.ModulosPermitidos != null)
        {
            membro.ModulosPermitidos = string.Join(",", command.ModulosPermitidos.Distinct(StringComparer.OrdinalIgnoreCase));
        }

        membro.AtualizadoEm = DateTime.UtcNow;
    }

    public async Task<MembroEquipeDto> AlterarStatusMembroAsync(Guid id, AlterarStatusMembroCommand command)
    {
        var (usuarioAtual, escritorioId) = await ObterUsuarioContextoAsync();

        if (id == usuarioAtual.Id && !command.Ativo)
            throw new InvalidOperationException("Não é permitido desativar a própria conta.");

        var membro = await _usuarioRepository.GetById(id);
        if (membro is null || membro.IsContaSuporte())
            throw new KeyNotFoundException($"Membro com Id {id} não encontrado.");

        var membroEscritorioId = membro.EscritorioId ?? membro.Id;
        if (membroEscritorioId != escritorioId)
            throw new UnauthorizedAccessException(MensagemSemPermissaoGerenciarMembro);

        if (!command.Ativo && membro.IsAdmin())
        {
            var membros = await _usuarioRepository.ObterPorEscritorioIdAsync(escritorioId);
            var outrosAdminsAtivos = membros.Count(m => m.Id != id && m.Ativo && m.IsAdmin());
            if (outrosAdminsAtivos == 0)
            {
                throw new InvalidOperationException("Não é permitido desativar o único administrador ativo do escritório.");
            }
        }

        membro.Ativo = command.Ativo;
        membro.AtualizadoEm = DateTime.UtcNow;

        await _usuarioRepository.Update(membro);
        await _unitOfWork.Commit();

        return MapearParaDto(membro);
    }

    public async Task RedefinirSenhaMembroAsync(Guid id, RedefinirSenhaMembroCommand command)
    {
        var (_, escritorioId) = await ObterUsuarioContextoAsync();

        var membro = await _usuarioRepository.GetById(id);
        if (membro is null || membro.IsContaSuporte())
            throw new KeyNotFoundException($"Membro com Id {id} não encontrado.");

        var membroEscritorioId = membro.EscritorioId ?? membro.Id;
        if (membroEscritorioId != escritorioId)
            throw new UnauthorizedAccessException(MensagemSemPermissaoGerenciarMembro);

        var novaSenha = !string.IsNullOrWhiteSpace(command.NovaSenha) && command.NovaSenha.Length >= 6
            ? command.NovaSenha
            : GerarSenhaTemporaria();

        membro.SenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha, workFactor: 12);
        membro.AtualizadoEm = DateTime.UtcNow;

        await _usuarioRepository.Update(membro);
        await _unitOfWork.Commit();

        try
        {
            await _emailService.SendEmailAsync(
                membro.Email,
                "Sua senha no COTU foi redefinida",
                $"Olá {membro.Nome},\n\nSua senha no COTU foi redefinida pelo administrador.\n\nSua nova senha de acesso é: {novaSenha}\n\nRecomendamos alterá-la logo após o login."
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao enviar e-mail de redefinição de senha para {Email}", membro.Email);
        }
    }

    public async Task ExcluirMembroAsync(Guid id)
    {
        var (usuarioAtual, escritorioId) = await ObterUsuarioContextoAsync();

        if (id == usuarioAtual.Id)
            throw new InvalidOperationException("Não é permitido excluir a própria conta.");

        var membro = await _usuarioRepository.GetById(id);
        if (membro is null || membro.IsContaSuporte())
            throw new KeyNotFoundException($"Membro com Id {id} não encontrado.");

        var membroEscritorioId = membro.EscritorioId ?? membro.Id;
        if (membroEscritorioId != escritorioId)
            throw new UnauthorizedAccessException(MensagemSemPermissaoGerenciarMembro);

        if (membro.IsAdmin())
        {
            var membros = await _usuarioRepository.ObterPorEscritorioIdAsync(escritorioId);
            var outrosAdmins = membros.Count(m => m.Id != id && m.Ativo && m.IsAdmin());
            if (outrosAdmins == 0)
            {
                throw new InvalidOperationException("Não é permitido excluir o único administrador do escritório.");
            }
        }

        await _usuarioRepository.Delete(id);
        await _unitOfWork.Commit();
    }

    public async Task<PerfilUsuarioDto> ObterPerfilAsync(Guid usuarioId)
    {
        var usuario = await _usuarioRepository.GetById(usuarioId)
            ?? throw new KeyNotFoundException("Perfil de usuário não encontrado.");

        return MapearParaPerfilDto(usuario);
    }

    public async Task<PerfilUsuarioDto> AtualizarPerfilAsync(Guid usuarioId, AtualizarPerfilCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new ArgumentException("Nome é obrigatório.");

        var usuario = await _usuarioRepository.GetById(usuarioId)
            ?? throw new KeyNotFoundException(MensagemUsuarioNaoEncontrado);

        usuario.Nome = command.Nome.Trim();
        usuario.Cargo = command.Cargo?.Trim();
        usuario.Telefone = command.Telefone?.Trim();
        usuario.AtualizadoEm = DateTime.UtcNow;

        await _usuarioRepository.Update(usuario);
        await _unitOfWork.Commit();

        return MapearParaPerfilDto(usuario);
    }

    public async Task AlterarSenhaAsync(Guid usuarioId, AlterarSenhaCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.SenhaAtual))
            throw new ArgumentException("A senha atual é obrigatória.");

        if (string.IsNullOrWhiteSpace(command.NovaSenha) || command.NovaSenha.Length < 6)
            throw new ArgumentException("A nova senha deve possuir no mínimo 6 caracteres.");

        var usuario = await _usuarioRepository.GetById(usuarioId)
            ?? throw new KeyNotFoundException(MensagemUsuarioNaoEncontrado);

        if (!BCrypt.Net.BCrypt.Verify(command.SenhaAtual, usuario.SenhaHash))
        {
            throw new InvalidOperationException("A senha atual informada está incorreta.");
        }

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(command.NovaSenha, workFactor: 12);
        usuario.AtualizadoEm = DateTime.UtcNow;

        await _usuarioRepository.Update(usuario);
        await _unitOfWork.Commit();
    }

    public async Task<string> AtualizarFotoPerfilAsync(Guid usuarioId, Stream stream, string fileName, string contentType)
    {
        var usuario = await _usuarioRepository.GetById(usuarioId)
            ?? throw new KeyNotFoundException(MensagemUsuarioNaoEncontrado);

        FileSecurityValidator.ValidarImagem(stream, fileName, contentType);

        if (!string.IsNullOrEmpty(usuario.FotoUrl))
        {
            try
            {
                await _storageService.DeleteAsync(usuario.FotoUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao remover foto de perfil anterior para {UsuarioId}", usuarioId);
            }
        }

        var url = await _storageService.UploadAsync(stream, fileName, contentType, "perfil", usuarioId.ToString());
        usuario.FotoUrl = url;
        usuario.AtualizadoEm = DateTime.UtcNow;

        await _usuarioRepository.Update(usuario);
        await _unitOfWork.Commit();

        return url;
    }

    public async Task RemoverFotoPerfilAsync(Guid usuarioId)
    {
        var usuario = await _usuarioRepository.GetById(usuarioId)
            ?? throw new KeyNotFoundException(MensagemUsuarioNaoEncontrado);

        if (!string.IsNullOrEmpty(usuario.FotoUrl))
        {
            try
            {
                await _storageService.DeleteAsync(usuario.FotoUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao remover foto de perfil do storage para {UsuarioId}", usuarioId);
            }

            usuario.FotoUrl = null;
            usuario.AtualizadoEm = DateTime.UtcNow;

            await _usuarioRepository.Update(usuario);
            await _unitOfWork.Commit();
        }
    }

    private async Task<(Usuario Usuario, Guid EscritorioId)> ObterUsuarioContextoAsync()
    {
        var usuarioId = ObterUsuarioIdContexto();
        var usuario = await _usuarioRepository.GetById(usuarioId);
        if (usuario is null)
            throw new UnauthorizedAccessException("Usuário autenticado não encontrado.");

        if (!usuario.EscritorioId.HasValue && !usuario.IsContaSuporte())
        {
            usuario.EscritorioId = usuario.Id;
            await _usuarioRepository.Update(usuario);
            await _unitOfWork.Commit();
        }

        var escritorioId = usuario.EscritorioId ?? usuario.Id;
        return (usuario, escritorioId);
    }

    private Guid ObterUsuarioIdContexto()
    {
        var user = _httpContextAccessor?.HttpContext?.User;
        var claim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? user?.FindFirst("nameid")?.Value
                 ?? user?.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(claim) || !Guid.TryParse(claim, out var id) || id == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado ou identidade inválida.");
        }

        return id;
    }

    private static string GerarSenhaTemporaria()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$";
        var bytes = RandomNumberGenerator.GetBytes(10);
        var result = new char[10];
        for (var i = 0; i < 10; i++)
        {
            result[i] = chars[bytes[i] % chars.Length];
        }
        return "Cot@" + new string(result);
    }

    private static MembroEquipeDto MapearParaDto(Usuario u)
    {
        List<string> modulos;
        if (u.IsAdmin() || u.IsContaSuporte())
        {
            modulos = ModulosApp.Todos.ToList();
        }
        else if (string.IsNullOrWhiteSpace(u.ModulosPermitidos))
        {
            modulos = new List<string>();
        }
        else
        {
            modulos = u.ModulosPermitidos
                .Split(ModuleSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return new MembroEquipeDto(
            u.Id,
            u.EscritorioId,
            u.Nome,
            u.Email,
            u.Role,
            u.Cargo,
            u.Telefone,
            u.FotoUrl,
            modulos,
            u.Ativo,
            u.CriadoEm,
            u.AtualizadoEm
        );
    }

    private static PerfilUsuarioDto MapearParaPerfilDto(Usuario u)
    {
        List<string> modulos;
        if (u.IsAdmin() || u.IsContaSuporte())
        {
            modulos = ModulosApp.Todos.ToList();
        }
        else if (string.IsNullOrWhiteSpace(u.ModulosPermitidos))
        {
            modulos = new List<string>();
        }
        else
        {
            modulos = u.ModulosPermitidos
                .Split(ModuleSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return new PerfilUsuarioDto(
            u.Id,
            u.EscritorioId,
            u.Nome,
            u.Email,
            u.Role,
            u.Cargo,
            u.Telefone,
            u.FotoUrl,
            modulos,
            u.CriadoEm
        );
    }
}
