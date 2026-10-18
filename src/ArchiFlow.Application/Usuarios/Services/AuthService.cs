using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.Usuarios.DTOs;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Domain.Usuarios;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Usuarios.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IRepresentanteClienteRepository _representanteClienteRepository;
    private readonly IProjetoRepository _projetoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IClienteRepository clienteRepository,
        IProjetoRepository projetoRepository,
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
        : this(usuarioRepository, clienteRepository, null!, projetoRepository, unitOfWork, tokenService)
    {
    }

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IClienteRepository clienteRepository,
        IRepresentanteClienteRepository? representanteClienteRepository,
        IProjetoRepository projetoRepository,
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _clienteRepository = clienteRepository;
        _representanteClienteRepository = representanteClienteRepository!;
        _projetoRepository = projetoRepository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> Login(LoginRequestDto request)
    {
        var usuario = await _usuarioRepository.GetByEmail(request.Email);
        if (usuario != null)
        {
            if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash) || !usuario.Ativo)
            {
                throw new UnauthorizedAccessException("Credenciais inválidas ou usuário inativo.");
            }

            var token = _tokenService.GenerateToken(usuario);
            var modulos = string.IsNullOrWhiteSpace(usuario.ModulosPermitidos)
                ? (usuario.IsAdmin() ? ModulosApp.Todos.ToList() : new System.Collections.Generic.List<string>())
                : usuario.ModulosPermitidos.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            return new LoginResponseDto(
                token, 
                usuario.Role, 
                usuario.Nome, 
                usuario.Email, 
                usuario.Id, 
                null, 
                modulos, 
                usuario.FotoUrl);
        }

        var representante = _representanteClienteRepository != null 
            ? await _representanteClienteRepository.ObterPorEmailAsync(request.Email) 
            : null;
        if (representante != null)
        {
            if (!BCrypt.Net.BCrypt.Verify(request.Senha, representante.SenhaHash) || !representante.Ativo)
            {
                throw new UnauthorizedAccessException("Credenciais inválidas ou conta inativa.");
            }

            var projetos = await _projetoRepository.GetByClienteId(representante.ClienteId);
            var projetoId = projetos.FirstOrDefault()?.Id;

            var token = _tokenService.GenerateToken(representante, projetoId);
            return new LoginResponseDto(token, "Cliente", representante.Nome, representante.Email, representante.Id, projetoId);
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
}
