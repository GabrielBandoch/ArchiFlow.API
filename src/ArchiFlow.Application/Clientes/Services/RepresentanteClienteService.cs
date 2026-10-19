using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Clientes.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Empresa;
using ArchiFlow.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace ArchiFlow.Application.Clientes.Services;

public class RepresentanteClienteService : IRepresentanteClienteService
{
    private readonly IRepresentanteClienteRepository _representanteRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IDadosEmpresaRepository _empresaRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppUrlService? _appUrlService;
    private readonly ILogger<RepresentanteClienteService> _logger;

    public RepresentanteClienteService(
        IRepresentanteClienteRepository representanteRepository,
        IClienteRepository clienteRepository,
        IDadosEmpresaRepository empresaRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<RepresentanteClienteService> logger,
        IAppUrlService? appUrlService = null)
    {
        _representanteRepository = representanteRepository;
        _clienteRepository = clienteRepository;
        _empresaRepository = empresaRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _appUrlService = appUrlService;
    }

    public async Task<IEnumerable<RepresentanteClienteDto>> ObterPorClienteIdAsync(Guid clienteId)
    {
        var reps = await _representanteRepository.ObterPorClienteIdAsync(clienteId);
        return reps.Select(MapearParaDto);
    }

    public async Task<RepresentanteClienteDto?> ObterPorIdAsync(Guid id)
    {
        var rep = await _representanteRepository.ObterPorIdAsync(id);
        return rep == null ? null : MapearParaDto(rep);
    }

    public async Task<RepresentanteClienteDto> ConvidarRepresentanteAsync(Guid clienteId, ConvidarRepresentanteCommand command, string? baseUrl = null)
    {
        _ = await _clienteRepository.GetById(clienteId)
            ?? throw new KeyNotFoundException("Cliente não encontrado.");

        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new ArgumentException("Nome do representante é obrigatório.");

        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ArgumentException("E-mail do representante é obrigatório.");

        var emailNormalizado = command.Email.Trim().ToLower();

        var existente = await _representanteRepository.ObterPorEmailAsync(emailNormalizado);
        if (existente != null && existente.ClienteId == clienteId)
        {
            if (command.EnviarEmailConvite)
            {
                await ReenviarConviteAsync(existente.Id, baseUrl);
            }
            return MapearParaDto(existente);
        }

        string? rawToken = null;
        string? tokenHash = null;
        DateTime? expiraEm = null;

        if (command.EnviarEmailConvite)
        {
            rawToken = GerarTokenCriptografico();
            tokenHash = ComputeSha256(rawToken);
            expiraEm = DateTime.UtcNow.AddDays(7);
        }

        var representante = new RepresentanteCliente
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            Nome = command.Nome.Trim(),
            Email = emailNormalizado,
            Telefone = command.Telefone?.Trim(),
            Cargo = command.Cargo?.Trim(),
            Ativo = true,
            ConviteTokenHash = tokenHash,
            ConviteExpiraEm = expiraEm,
            ConviteUsadoEm = null,
            CriadoEm = DateTime.UtcNow
        };

        await _representanteRepository.AdicionarAsync(representante);

        if (command.EnviarEmailConvite && !string.IsNullOrEmpty(rawToken))
        {
            await EnviarEmailConviteAsync(representante, rawToken, baseUrl);
        }

        return MapearParaDto(representante);
    }

    public async Task ReenviarConviteAsync(Guid representanteId, string? baseUrl = null)
    {
        var rep = await _representanteRepository.ObterPorIdAsync(representanteId)
            ?? throw new KeyNotFoundException("Representante não encontrado.");

        _ = await _clienteRepository.GetById(rep.ClienteId)
            ?? throw new KeyNotFoundException("Cliente associado não encontrado.");

        var rawToken = GerarTokenCriptografico();
        var tokenHash = ComputeSha256(rawToken);

        rep.ConviteTokenHash = tokenHash;
        rep.ConviteExpiraEm = DateTime.UtcNow.AddDays(7);
        rep.ConviteUsadoEm = null;
        rep.AtualizadoEm = DateTime.UtcNow;

        await _representanteRepository.AtualizarAsync(rep);

        await EnviarEmailConviteAsync(rep, rawToken, baseUrl);
    }

    public async Task<RepresentanteClienteDto> AlterarStatusAsync(Guid representanteId, bool ativo)
    {
        var rep = await _representanteRepository.ObterPorIdAsync(representanteId)
            ?? throw new KeyNotFoundException("Representante não encontrado.");

        rep.Ativo = ativo;
        rep.AtualizadoEm = DateTime.UtcNow;

        await _representanteRepository.AtualizarAsync(rep);
        return MapearParaDto(rep);
    }

    public async Task RemoverRepresentanteAsync(Guid representanteId)
    {
        await _representanteRepository.RemoverAsync(representanteId);
    }

    public async Task<ResultadoValidacaoConviteDto> ValidarTokenConviteAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return new ResultadoValidacaoConviteDto(false, null, null, "Token de convite não informado.");

        var tokenHash = ComputeSha256(token.Trim());
        var rep = await _representanteRepository.ObterPorTokenConviteHashAsync(tokenHash);

        if (rep == null)
            return new ResultadoValidacaoConviteDto(false, null, null, "Convite inválido ou inexistente.");

        if (rep.ConviteUsadoEm.HasValue)
            return new ResultadoValidacaoConviteDto(false, rep.Nome, rep.Email, "Este convite já foi utilizado para ativar sua conta.");

        if (!rep.ConviteExpiraEm.HasValue || rep.ConviteExpiraEm.Value <= DateTime.UtcNow)
            return new ResultadoValidacaoConviteDto(false, rep.Nome, rep.Email, "Este convite expirou. Solicite um novo envio ao escritório responsável.");

        return new ResultadoValidacaoConviteDto(true, rep.Nome, rep.Email, "Convite válido.");
    }

    public async Task AtivarContaAsync(AtivarContaRepresentanteCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            throw new ArgumentException("Token é obrigatório.");

        if (string.IsNullOrWhiteSpace(command.Senha) || command.Senha.Length < 6)
            throw new ArgumentException("A senha deve possuir no mínimo 6 caracteres.");

        var tokenHash = ComputeSha256(command.Token.Trim());
        var rep = await _representanteRepository.ObterPorTokenConviteHashAsync(tokenHash)
            ?? throw new InvalidOperationException("Convite inválido ou inexistente.");

        if (rep.ConviteUsadoEm.HasValue)
            throw new InvalidOperationException("Este convite já foi utilizado.");

        if (!rep.ConviteExpiraEm.HasValue || rep.ConviteExpiraEm.Value <= DateTime.UtcNow)
            throw new InvalidOperationException("Este convite expirou. Solicite um novo envio ao escritório.");

        var hash = BCrypt.Net.BCrypt.HashPassword(command.Senha, workFactor: 12);
        rep.SenhaHash = hash;
        rep.ConviteUsadoEm = DateTime.UtcNow;
        rep.ConviteTokenHash = null;
        rep.Ativo = true;
        rep.AtualizadoEm = DateTime.UtcNow;

        await _representanteRepository.AtualizarAsync(rep);

        var cliente = await _clienteRepository.GetById(rep.ClienteId);
        if (cliente != null && string.Equals(cliente.Email, rep.Email, StringComparison.OrdinalIgnoreCase))
        {
            cliente.SenhaPortal = hash;
            await _clienteRepository.Update(cliente);
            await _unitOfWork.Commit();
        }

        _logger.LogInformation("Conta de representante ativada com sucesso: {Email}", rep.Email);
    }

    public async Task DispararConviteProjetoCriadoAsync(Guid clienteId, string? baseUrl = null)
    {
        var cliente = await _clienteRepository.GetById(clienteId);
        if (cliente == null || string.IsNullOrWhiteSpace(cliente.Email))
            return;

        var reps = (await _representanteRepository.ObterPorClienteIdAsync(clienteId)).ToList();

        if (reps.Any(r => !string.IsNullOrEmpty(r.SenhaHash) && r.Ativo))
        {
            _logger.LogInformation("Cliente {ClienteId} já possui representante com conta ativa. Convite não reenviado.", clienteId);
            return;
        }

        var conviteRecente = reps.FirstOrDefault(r => r.IsConvitePendente());
        if (conviteRecente != null && conviteRecente.ConviteExpiraEm.HasValue && conviteRecente.ConviteExpiraEm.Value > DateTime.UtcNow.AddDays(2))
        {
            _logger.LogInformation("Cliente {ClienteId} já possui convite recente pendente. Evitando duplicidade.", clienteId);
            return;
        }

        var repPrincipal = reps.FirstOrDefault(r => string.Equals(r.Email, cliente.Email.Trim(), StringComparison.OrdinalIgnoreCase));
        if (repPrincipal == null)
        {
            await ConvidarRepresentanteAsync(clienteId, new ConvidarRepresentanteCommand(
                Nome: cliente.Nome,
                Email: cliente.Email,
                Telefone: cliente.Telefone,
                Cargo: "Contato Principal"
            ), baseUrl);
        }
        else
        {
            await ReenviarConviteAsync(repPrincipal.Id, baseUrl);
        }
    }

    private async Task EnviarEmailConviteAsync(RepresentanteCliente rep, string rawToken, string? baseUrl)
    {
        var empresa = await _empresaRepository.ObterAsync();
        var nomeEmpresa = !string.IsNullOrWhiteSpace(empresa?.NomeEmpresa) ? empresa.NomeEmpresa : "COTU Studio";

        var domain = _appUrlService != null
            ? _appUrlService.ObterUrlBaseConfiavel(baseUrl)
            : ResolverDominioFallback(baseUrl);

        var inviteUrl = $"{domain}/ativar-conta?token={rawToken}";

        var repNomeHtml = WebUtility.HtmlEncode(rep.Nome);
        var nomeEmpresaHtml = WebUtility.HtmlEncode(nomeEmpresa);
        var repEmailHtml = WebUtility.HtmlEncode(rep.Email);

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
        <h2 style=""margin-top: 0; color: #0f172a; font-size: 18px;"">Bem-vindo ao Portal do Cliente</h2>
        <p>Olá, <strong>{repNomeHtml}</strong>,</p>
        <p>Você foi convidado pela equipe do escritório <strong>{nomeEmpresaHtml}</strong> para acompanhar seus projetos em andamento com total transparência e organização.</p>
        <p>No Portal do Cliente, você poderá visualizar o cronograma de etapas, acessar documentos e plantas técnicas, acompanhar o status em tempo real e interagir diretamente com a equipe técnica.</p>
        <p>Para ativar seu acesso exclusivo e definir sua senha pessoal, clique no botão abaixo:</p>
        <div style=""text-align: center; margin: 32px 0;"">
          <a href=""{inviteUrl}"" style=""background-color: #765538; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: 600; display: inline-block; font-size: 14px;"">Ativar Meu Acesso</a>
        </div>
        <p style=""font-size: 13px; color: #64748b;"">Este link é exclusivo para <strong>{repEmailHtml}</strong> e expira em 7 dias.</p>
      </td>
    </tr>
    <tr>
      <td style=""background-color: #f1f5f9; padding: 16px 32px; text-align: center; color: #64748b; font-size: 12px;"">
        © {DateTime.UtcNow.Year} COTU Technologies — Gestão Transparente de Projetos
      </td>
    </tr>
  </table>
</body>
</html>";

        try
        {
            await _emailService.SendEmailAsync(
                rep.Email,
                $"Convite para o Portal do Cliente — {nomeEmpresa}",
                htmlBody
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao enviar e-mail de convite para {Email}", rep.Email);
        }
    }

    private static string ResolverDominioFallback(string? baseUrl)
    {
        var domain = baseUrl?.TrimEnd('/');
        if (string.IsNullOrEmpty(domain))
        {
            var envDomain = Environment.GetEnvironmentVariable("DOMAIN_NAME") ?? "cotu.com.br";
            var scheme = envDomain.Contains("localhost") ? "http" : "https";
            domain = $"{scheme}://{envDomain}";
        }
        return domain;
    }

    private static string GerarTokenCriptografico()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static RepresentanteClienteDto MapearParaDto(RepresentanteCliente r)
    {
        string statusConvite;
        if (!string.IsNullOrEmpty(r.SenhaHash))
        {
            statusConvite = "Ativo";
        }
        else if (r.ConviteExpiraEm.HasValue && r.ConviteExpiraEm.Value < DateTime.UtcNow)
        {
            statusConvite = "Expirado";
        }
        else if (!string.IsNullOrEmpty(r.ConviteTokenHash))
        {
            statusConvite = "Convite Enviado";
        }
        else
        {
            statusConvite = "Pendente";
        }

        return new RepresentanteClienteDto(
            r.Id,
            r.ClienteId,
            r.Nome,
            r.Email,
            r.Telefone,
            r.Cargo,
            r.Ativo,
            statusConvite,
            r.CriadoEm,
            r.AtualizadoEm
        );
    }
}
