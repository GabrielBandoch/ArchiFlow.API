using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ArchiFlow.Application.Common;
using ArchiFlow.Application.Interfaces.Services;
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

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UsuarioService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<IEnumerable<MembroEquipeDto>> ObterEquipeAsync()
    {
        var (_, escritorioId) = await ObterUsuarioContextoAsync();
        var membros = await _usuarioRepository.ObterPorEscritorioIdAsync(escritorioId);
        return membros
            .Where(m => !string.Equals(m.Email, "suporte.cotu@cotu.com.br", StringComparison.OrdinalIgnoreCase))
            .Select(MapearParaDto);
    }

    public async Task<MembroEquipeDto?> ObterMembroPorIdAsync(Guid id)
    {
        var (_, escritorioId) = await ObterUsuarioContextoAsync();
        var membro = await _usuarioRepository.GetById(id);
        if (membro is null) return null;

        var membroEscritorioId = membro.EscritorioId ?? membro.Id;
        if (membroEscritorioId != escritorioId) return null;

        return MapearParaDto(membro);
    }

    public async Task<MembroEquipeDto> ConvidarMembroAsync(ConvidarMembroEquipeCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new ArgumentException("Nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(command.Email) || !new EmailAddressAttribute().IsValid(command.Email.Trim()))
            throw new ArgumentException("E-mail em formato inválido.");

        if (string.IsNullOrWhiteSpace(command.Role) || !Roles.RolesValidasEquipe.Contains(command.Role))
            throw new ArgumentException($"Função '{command.Role}' inválida.");

        var (usuarioAtual, escritorioId) = await ObterUsuarioContextoAsync();

        var emailNormalizado = command.Email.Trim().ToLower();
        var existente = await _usuarioRepository.GetByEmail(emailNormalizado);
        if (existente is not null)
            throw new InvalidOperationException("Já existe um usuário cadastrado com este e-mail.");

        var senha = !string.IsNullOrWhiteSpace(command.SenhaTemporaria) && command.SenhaTemporaria.Length >= 6
            ? command.SenhaTemporaria
            : GerarSenhaTemporaria();

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
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        await _usuarioRepository.Create(novoUsuario);
        await _unitOfWork.Commit();

        try
        {
            var req = _httpContextAccessor.HttpContext?.Request;
            var loginUrl = req != null ? $"{req.Scheme}://{req.Host}/login" : null;
            var corpoHtml = EmailTemplates.GerarConviteEquipe(
                novoUsuario.Nome,
                usuarioAtual.Nome,
                novoUsuario.Email,
                senha,
                loginUrl
            );

            await _emailService.SendEmailAsync(
                novoUsuario.Email,
                "Convite para a equipe Cotu",
                corpoHtml
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao enviar e-mail de convite para {Email}", novoUsuario.Email);
        }

        return MapearParaDto(novoUsuario);
    }

    public async Task<MembroEquipeDto> AtualizarMembroAsync(Guid id, AtualizarMembroEquipeCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new ArgumentException("Nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(command.Role) || !Roles.RolesValidasEquipe.Contains(command.Role))
            throw new ArgumentException($"Função '{command.Role}' inválida.");

        var (_, escritorioId) = await ObterUsuarioContextoAsync();

        var membro = await _usuarioRepository.GetById(id);
        if (membro is null)
            throw new KeyNotFoundException($"Membro com Id {id} não encontrado.");

        var membroEscritorioId = membro.EscritorioId ?? membro.Id;
        if (membroEscritorioId != escritorioId)
            throw new UnauthorizedAccessException(MensagemSemPermissaoGerenciarMembro);

        if (!string.IsNullOrWhiteSpace(command.Email))
        {
            var emailNormalizado = command.Email.Trim().ToLower();
            if (!new EmailAddressAttribute().IsValid(emailNormalizado))
                throw new ArgumentException("E-mail em formato inválido.");

            if (!string.Equals(emailNormalizado, membro.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existente = await _usuarioRepository.GetByEmail(emailNormalizado);
                if (existente is not null && existente.Id != membro.Id)
                    throw new InvalidOperationException("Já existe outro usuário cadastrado com este e-mail.");

                membro.Email = emailNormalizado;
            }
        }

        membro.Nome = command.Nome.Trim();
        membro.Role = command.Role;
        membro.Cargo = command.Cargo?.Trim();
        membro.Telefone = command.Telefone?.Trim();
        membro.AtualizadoEm = DateTime.UtcNow;

        await _usuarioRepository.Update(membro);
        await _unitOfWork.Commit();

        return MapearParaDto(membro);
    }

    public async Task<MembroEquipeDto> AlterarStatusMembroAsync(Guid id, AlterarStatusMembroCommand command)
    {
        var (usuarioAtual, escritorioId) = await ObterUsuarioContextoAsync();

        if (id == usuarioAtual.Id && !command.Ativo)
            throw new InvalidOperationException("Não é permitido desativar a própria conta.");

        var membro = await _usuarioRepository.GetById(id);
        if (membro is null)
            throw new KeyNotFoundException($"Membro com Id {id} não encontrado.");

        var membroEscritorioId = membro.EscritorioId ?? membro.Id;
        if (membroEscritorioId != escritorioId)
            throw new UnauthorizedAccessException(MensagemSemPermissaoGerenciarMembro);

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
        if (membro is null)
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
            var req = _httpContextAccessor.HttpContext?.Request;
            var loginUrl = req != null ? $"{req.Scheme}://{req.Host}/login" : null;
            var corpoHtml = EmailTemplates.GerarRedefinicaoSenha(
                membro.Nome,
                novaSenha,
                loginUrl
            );

            await _emailService.SendEmailAsync(
                membro.Email,
                "Sua senha no Cotu foi redefinida",
                corpoHtml
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
        if (membro is null)
            throw new KeyNotFoundException($"Membro com Id {id} não encontrado.");

        var membroEscritorioId = membro.EscritorioId ?? membro.Id;
        if (membroEscritorioId != escritorioId)
            throw new UnauthorizedAccessException(MensagemSemPermissaoGerenciarMembro);

        await _usuarioRepository.Delete(id);
        await _unitOfWork.Commit();
    }

    private async Task<(Usuario Usuario, Guid EscritorioId)> ObterUsuarioContextoAsync()
    {
        var usuarioId = ObterUsuarioIdContexto();
        var usuario = await _usuarioRepository.GetById(usuarioId);
        if (usuario is null)
            throw new UnauthorizedAccessException("Usuário autenticado não encontrado.");

        if (!usuario.EscritorioId.HasValue)
        {
            usuario.EscritorioId = usuario.Id;
            await _usuarioRepository.Update(usuario);
            await _unitOfWork.Commit();
        }

        return (usuario, usuario.EscritorioId.Value);
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
        return "Af@" + new string(result);
    }

    public async Task<PerfilUsuarioDto> ObterMeuPerfilAsync()
    {
        var (usuario, _) = await ObterUsuarioContextoAsync();
        return new PerfilUsuarioDto(
            usuario.Id,
            usuario.EscritorioId,
            usuario.Nome,
            usuario.Email,
            usuario.Role,
            usuario.Cargo,
            usuario.Telefone,
            usuario.CriadoEm,
            usuario.AtualizadoEm
        );
    }

    public async Task<PerfilUsuarioDto> AtualizarMeuPerfilAsync(AtualizarMeuPerfilCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new ArgumentException("Nome é obrigatório.");

        var (usuario, _) = await ObterUsuarioContextoAsync();

        usuario.Nome = command.Nome.Trim();
        usuario.Telefone = command.Telefone?.Trim();
        usuario.Cargo = command.Cargo?.Trim();
        usuario.AtualizadoEm = DateTime.UtcNow;

        await _usuarioRepository.Update(usuario);
        await _unitOfWork.Commit();

        return new PerfilUsuarioDto(
            usuario.Id,
            usuario.EscritorioId,
            usuario.Nome,
            usuario.Email,
            usuario.Role,
            usuario.Cargo,
            usuario.Telefone,
            usuario.CriadoEm,
            usuario.AtualizadoEm
        );
    }

    public async Task AlterarMinhaSenhaAsync(AlterarMinhaSenhaCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.SenhaAtual))
            throw new ArgumentException("Senha atual é obrigatória.");

        if (string.IsNullOrWhiteSpace(command.NovaSenha) || command.NovaSenha.Length < 6)
            throw new ArgumentException("A nova senha deve possuir no mínimo 6 caracteres.");

        var (usuario, _) = await ObterUsuarioContextoAsync();

        if (!BCrypt.Net.BCrypt.Verify(command.SenhaAtual, usuario.SenhaHash))
            throw new UnauthorizedAccessException("A senha atual informada está incorreta.");

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(command.NovaSenha, workFactor: 12);
        usuario.AtualizadoEm = DateTime.UtcNow;

        await _usuarioRepository.Update(usuario);
        await _unitOfWork.Commit();
    }

    private static MembroEquipeDto MapearParaDto(Usuario u) =>
        new(
            u.Id,
            u.EscritorioId,
            u.Nome,
            u.Email,
            u.Role,
            u.Cargo,
            u.Telefone,
            u.Ativo,
            u.CriadoEm,
            u.AtualizadoEm
        );
}
