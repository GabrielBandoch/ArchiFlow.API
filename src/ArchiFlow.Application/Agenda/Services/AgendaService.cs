using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ArchiFlow.Application.Agenda.Commands;
using ArchiFlow.Application.Agenda.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Agenda;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Domain.Usuarios;
using Microsoft.AspNetCore.Http;

namespace ArchiFlow.Application.Agenda.Services;

public class AgendaService : IAgendaService
{
    private const string MensagemSemPermissao = "Você não possui permissão para gerenciar este compromisso.";
    private const string IntegracaoOAuth = "OAuth";

    private readonly ICompromissoRepository _compromissoRepository;
    private readonly IConfiguracaoAgendaRepository _configuracaoAgendaRepository;
    private readonly IProjetoRepository _projetoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ILeadRepository? _leadRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IGoogleCalendarService _googleCalendarService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Microsoft.Extensions.Configuration.IConfiguration? _configuration;

    public AgendaService(
        ICompromissoRepository compromissoRepository,
        IConfiguracaoAgendaRepository configuracaoAgendaRepository,
        IProjetoRepository projetoRepository,
        IClienteRepository clienteRepository,
        IUsuarioRepository usuarioRepository,
        IGoogleCalendarService googleCalendarService,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        ILeadRepository? leadRepository = null,
        Microsoft.Extensions.Configuration.IConfiguration? configuration = null)
    {
        _compromissoRepository = compromissoRepository;
        _configuracaoAgendaRepository = configuracaoAgendaRepository;
        _projetoRepository = projetoRepository;
        _clienteRepository = clienteRepository;
        _usuarioRepository = usuarioRepository;
        _googleCalendarService = googleCalendarService;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _leadRepository = leadRepository;
        _configuration = configuration;
    }

    public async Task<IEnumerable<CompromissoDto>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim, Guid? usuarioId = null, Guid? projetoId = null)
    {
        if (inicio > fim)
            throw new ArgumentException("A data inicial não pode ser superior à data final.");

        var (_, escritorioId) = await ObterUsuarioContextoAsync();
        var compromissos = await _compromissoRepository.ObterPorPeriodoAsync(escritorioId, inicio, fim, usuarioId, projetoId);

        return await MapearListaAsync(compromissos);
    }

    public async Task<IEnumerable<CompromissoDto>> ObterProximosAsync(int quantidade = 10, Guid? usuarioId = null)
    {
        var count = Math.Clamp(quantidade, 1, 50);
        var (_, escritorioId) = await ObterUsuarioContextoAsync();
        var compromissos = await _compromissoRepository.ObterProximosAsync(escritorioId, count, usuarioId);

        return await MapearListaAsync(compromissos);
    }

    public async Task<CompromissoDto?> ObterPorIdAsync(Guid id)
    {
        var (_, escritorioId) = await ObterUsuarioContextoAsync();
        var compromisso = await _compromissoRepository.GetById(id);
        if (compromisso is null || compromisso.EscritorioId != escritorioId)
            return null;

        return await MapearAsync(compromisso);
    }

    public async Task<CompromissoDto> CriarCompromissoAsync(CriarCompromissoCommand command)
    {
        var dataInicioUtc = GarantirUtc(command.DataHoraInicio);
        var dataFimUtc = GarantirUtc(command.DataHoraFim);
        ValidarHorarios(dataInicioUtc, dataFimUtc);

        var (usuarioLogado, escritorioId) = await ObterUsuarioContextoAsync();

        var tipo = !string.IsNullOrWhiteSpace(command.Tipo) && TiposCompromisso.Todos.Contains(command.Tipo)
            ? command.Tipo
            : TiposCompromisso.Geral;

        var meetLink = command.GerarGoogleMeet
            ? _googleCalendarService.GerarLinkGoogleMeet(Guid.NewGuid().ToString())
            : null;

        var compromisso = new Compromisso
        {
            Id = Guid.NewGuid(),
            EscritorioId = escritorioId,
            UsuarioId = command.UsuarioId ?? usuarioLogado.Id,
            ProjetoId = command.ProjetoId,
            ClienteId = command.ClienteId,
            LeadId = command.LeadId,
            Titulo = command.Titulo.Trim(),
            Descricao = command.Descricao?.Trim(),
            Tipo = tipo,
            Status = StatusCompromisso.Agendado,
            DataHoraInicio = dataInicioUtc,
            DataHoraFim = dataFimUtc,
            Local = command.Local?.Trim(),
            LinkGoogleMeet = meetLink,
            CriadoEm = DateTime.UtcNow
        };

        await SincronizarCriacaoComGoogleCalendarAsync(compromisso, escritorioId);

        await _compromissoRepository.Create(compromisso);
        await _unitOfWork.Commit();

        return await MapearAsync(compromisso);
    }

    private async Task SincronizarCriacaoComGoogleCalendarAsync(Compromisso compromisso, Guid escritorioId)
    {
        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        if (config == null) return;

        var targetCalId = !string.IsNullOrWhiteSpace(config.GoogleCalendarId)
            ? config.GoogleCalendarId
            : config.EmailAgendaEmpresa;

        if (config.TipoIntegracao == IntegracaoOAuth && !string.IsNullOrWhiteSpace(config.GoogleOAuthRefreshToken))
        {
            var clientId = config.GoogleClientId ?? _configuration?["GoogleCalendar:ClientId"] ?? string.Empty;
            var clientSecret = config.GoogleClientSecret ?? _configuration?["GoogleCalendar:ClientSecret"] ?? string.Empty;

            var googleEvtId = await _googleCalendarService.CriarEventoViaOAuthAsync(
                compromisso,
                targetCalId,
                config.GoogleOAuthRefreshToken,
                clientId,
                clientSecret);

            if (!string.IsNullOrWhiteSpace(googleEvtId))
            {
                compromisso.GoogleEventId = googleEvtId;
            }
        }
        else if (!string.IsNullOrWhiteSpace(config.ChaveGoogleServiceAccountJson))
        {
            var googleEvtId = await _googleCalendarService.CriarEventoDiretoNoGoogleCalendarAsync(
                compromisso,
                targetCalId,
                config.ChaveGoogleServiceAccountJson);

            if (!string.IsNullOrWhiteSpace(googleEvtId))
            {
                compromisso.GoogleEventId = googleEvtId;
            }
        }
    }

    public async Task<CompromissoDto> AtualizarCompromissoAsync(Guid id, AtualizarCompromissoCommand command)
    {
        var dataInicioUtc = GarantirUtc(command.DataHoraInicio);
        var dataFimUtc = GarantirUtc(command.DataHoraFim);
        ValidarHorarios(dataInicioUtc, dataFimUtc);

        var (_, escritorioId) = await ObterUsuarioContextoAsync();
        var compromisso = await _compromissoRepository.GetById(id);
        if (compromisso is null)
            throw new KeyNotFoundException($"Compromisso com ID {id} não encontrado.");

        if (compromisso.EscritorioId != escritorioId)
            throw new UnauthorizedAccessException(MensagemSemPermissao);

        var tipo = !string.IsNullOrWhiteSpace(command.Tipo) && TiposCompromisso.Todos.Contains(command.Tipo)
            ? command.Tipo
            : compromisso.Tipo;

        var status = !string.IsNullOrWhiteSpace(command.Status) && StatusCompromisso.Todos.Contains(command.Status)
            ? command.Status
            : compromisso.Status;

        compromisso.Titulo = command.Titulo.Trim();
        compromisso.Descricao = command.Descricao?.Trim();
        compromisso.Tipo = tipo;
        compromisso.Status = status;
        compromisso.DataHoraInicio = dataInicioUtc;
        compromisso.DataHoraFim = dataFimUtc;
        compromisso.Local = command.Local?.Trim();
        compromisso.LinkGoogleMeet = command.LinkGoogleMeet?.Trim() ?? compromisso.LinkGoogleMeet;
        compromisso.ProjetoId = command.ProjetoId;
        compromisso.ClienteId = command.ClienteId;
        compromisso.LeadId = command.LeadId;
        compromisso.UsuarioId = command.UsuarioId ?? compromisso.UsuarioId;
        compromisso.AtualizadoEm = DateTime.UtcNow;

        await _compromissoRepository.Update(compromisso);
        await _unitOfWork.Commit();

        return await MapearAsync(compromisso);
    }

    public async Task<CompromissoDto> AlterarStatusCompromissoAsync(Guid id, AlterarStatusCompromissoCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Status) || !StatusCompromisso.Todos.Contains(command.Status))
            throw new ArgumentException($"Status '{command.Status}' inválido.");

        var (_, escritorioId) = await ObterUsuarioContextoAsync();
        var compromisso = await _compromissoRepository.GetById(id);
        if (compromisso is null)
            throw new KeyNotFoundException($"Compromisso com ID {id} não encontrado.");

        if (compromisso.EscritorioId != escritorioId)
            throw new UnauthorizedAccessException(MensagemSemPermissao);

        compromisso.Status = command.Status;
        compromisso.AtualizadoEm = DateTime.UtcNow;

        await _compromissoRepository.Update(compromisso);
        await _unitOfWork.Commit();

        return await MapearAsync(compromisso);
    }

    public async Task ExcluirCompromissoAsync(Guid id)
    {
        var (_, escritorioId) = await ObterUsuarioContextoAsync();
        var compromisso = await _compromissoRepository.GetById(id);
        if (compromisso is null)
            throw new KeyNotFoundException($"Compromisso com ID {id} não encontrado.");

        if (compromisso.EscritorioId != escritorioId)
            throw new UnauthorizedAccessException(MensagemSemPermissao);

        await _compromissoRepository.Delete(id);
        await _unitOfWork.Commit();
    }

    public async Task<string> ExportarIcsAsync(DateTime? inicio = null, DateTime? fim = null)
    {
        var (_, escritorioId) = await ObterUsuarioContextoAsync();
        var dataInicio = inicio ?? DateTime.UtcNow.AddMonths(-1);
        var dataFim = fim ?? DateTime.UtcNow.AddMonths(6);

        var compromissos = await _compromissoRepository.ObterPorPeriodoAsync(escritorioId, dataInicio, dataFim);
        return _googleCalendarService.ExportarIcs(compromissos);
    }

    private static void ValidarHorarios(DateTime inicio, DateTime fim)
    {
        if (fim <= inicio)
            throw new ArgumentException("A data/hora de término deve ser posterior à data/hora de início.");
    }

    private static DateTime GarantirUtc(DateTime data)
    {
        return data.Kind switch
        {
            DateTimeKind.Utc => data,
            DateTimeKind.Local => data.ToUniversalTime(),
            _ => DateTime.SpecifyKind(data, DateTimeKind.Utc)
        };
    }

    private async Task<CompromissoDto> MapearAsync(Compromisso c)
    {
        string? nomeProjeto = null;
        if (c.ProjetoId.HasValue)
        {
            var p = await _projetoRepository.GetById(c.ProjetoId.Value);
            nomeProjeto = p?.Nome;
        }

        string? nomeCliente = null;
        if (c.ClienteId.HasValue)
        {
            var cli = await _clienteRepository.GetById(c.ClienteId.Value);
            nomeCliente = cli?.Nome;
        }

        string? nomeLead = null;
        if (c.LeadId.HasValue && _leadRepository != null)
        {
            var l = await _leadRepository.GetById(c.LeadId.Value);
            nomeLead = l?.Nome;
        }

        string? nomeUsuario = null;
        if (c.UsuarioId.HasValue)
        {
            var u = await _usuarioRepository.GetById(c.UsuarioId.Value);
            nomeUsuario = u?.Nome;
        }

        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(c.EscritorioId);
        var emailEmpresa = config?.EmailAgendaEmpresa;

        var linkGoogle = _googleCalendarService.GerarLinkWebAdicionarEvento(c, nomeProjeto, nomeCliente, nomeLead, emailEmpresa);

        return new CompromissoDto(
            c.Id,
            c.EscritorioId,
            c.UsuarioId,
            nomeUsuario,
            c.ProjetoId,
            nomeProjeto,
            c.ClienteId,
            nomeCliente,
            c.Titulo,
            c.Descricao,
            c.Tipo,
            c.Status,
            c.DataHoraInicio,
            c.DataHoraFim,
            c.Local,
            c.LinkGoogleMeet,
            c.GoogleEventId,
            linkGoogle,
            c.CriadoEm,
            c.AtualizadoEm,
            c.LeadId,
            nomeLead
        );
    }

    private async Task<IEnumerable<CompromissoDto>> MapearListaAsync(IEnumerable<Compromisso> lista)
    {
        var dtos = new List<CompromissoDto>();
        foreach (var item in lista)
        {
            dtos.Add(await MapearAsync(item));
        }
        return dtos;
    }

    private async Task<(Usuario Usuario, Guid EscritorioId)> ObterUsuarioContextoAsync()
    {
        var user = _httpContextAccessor?.HttpContext?.User;
        var claim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? user?.FindFirst("nameid")?.Value
                 ?? user?.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(claim) || !Guid.TryParse(claim, out var id) || id == Guid.Empty)
            throw new UnauthorizedAccessException("Usuário não autenticado ou identidade inválida.");

        var usuario = await _usuarioRepository.GetById(id);
        if (usuario is null)
            throw new UnauthorizedAccessException("Usuário autenticado não encontrado.");

        var escritorioId = usuario.EscritorioId ?? usuario.Id;
        return (usuario, escritorioId);
    }

    public async Task<ConfiguracaoAgendaEscritorioDto?> ObterConfiguracaoAgendaEscritorioAsync()
    {
        var (_, escritorioId) = await ObterUsuarioContextoAsync();
        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        if (config is null)
            return null;

        return MapearConfiguracao(config);
    }

    public async Task<ConfiguracaoAgendaEscritorioDto> SalvarConfiguracaoAgendaEscritorioAsync(SalvarConfiguracaoAgendaEscritorioCommand command)
    {
        var (usuario, escritorioId) = await ObterUsuarioContextoAsync();
        if (usuario.Role == Roles.Cliente)
            throw new UnauthorizedAccessException("Clientes não possuem permissão para configurar a agenda corporativa do escritório.");

        var calId = !string.IsNullOrWhiteSpace(command.GoogleCalendarId)
            ? command.GoogleCalendarId.Trim()
            : (command.EmailAgendaEmpresa?.Trim() ?? string.Empty);

        if (string.IsNullOrWhiteSpace(calId))
            throw new ArgumentException("O ID da Agenda do Google é obrigatório.");

        var emailEmpresa = !string.IsNullOrWhiteSpace(command.EmailAgendaEmpresa)
            ? command.EmailAgendaEmpresa.Trim()
            : calId;

        var tipoIntegracao = !string.IsNullOrWhiteSpace(command.TipoIntegracao)
            ? command.TipoIntegracao.Trim()
            : "ServiceAccount";

        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        if (config is null)
        {
            config = new ConfiguracaoAgendaEscritorio
            {
                Id = Guid.NewGuid(),
                EscritorioId = escritorioId,
                EmailAgendaEmpresa = emailEmpresa,
                GoogleCalendarId = calId,
                ChaveGoogleServiceAccountJson = command.ChaveGoogleServiceAccountJson?.Trim(),
                GoogleClientId = command.GoogleClientId?.Trim(),
                GoogleClientSecret = command.GoogleClientSecret?.Trim(),
                TipoIntegracao = tipoIntegracao,
                NomeAgenda = string.IsNullOrWhiteSpace(command.NomeAgenda) ? "Agenda Oficial do Escritório" : command.NomeAgenda.Trim(),
                SincronizacaoAutomaticaAtiva = command.SincronizacaoAutomaticaAtiva,
                ConectadoEm = DateTime.UtcNow
            };
            await _configuracaoAgendaRepository.Create(config);
        }
        else
        {
            AtualizarConfiguracaoExistente(config, command, emailEmpresa, calId);
            await _configuracaoAgendaRepository.Update(config);
        }

        await _unitOfWork.Commit();
        return MapearConfiguracao(config);
    }

    private static void AtualizarConfiguracaoExistente(
        ConfiguracaoAgendaEscritorio config,
        SalvarConfiguracaoAgendaEscritorioCommand command,
        string emailEmpresa,
        string calId)
    {
        config.EmailAgendaEmpresa = emailEmpresa;
        config.GoogleCalendarId = calId;
        if (!string.IsNullOrWhiteSpace(command.ChaveGoogleServiceAccountJson))
        {
            config.ChaveGoogleServiceAccountJson = command.ChaveGoogleServiceAccountJson.Trim();
        }
        if (!string.IsNullOrWhiteSpace(command.GoogleClientId))
        {
            config.GoogleClientId = command.GoogleClientId.Trim();
        }
        if (!string.IsNullOrWhiteSpace(command.GoogleClientSecret))
        {
            config.GoogleClientSecret = command.GoogleClientSecret.Trim();
        }
        if (!string.IsNullOrWhiteSpace(command.TipoIntegracao))
        {
            config.TipoIntegracao = command.TipoIntegracao.Trim();
        }
        config.NomeAgenda = string.IsNullOrWhiteSpace(command.NomeAgenda) ? "Agenda Oficial do Escritório" : command.NomeAgenda.Trim();
        config.SincronizacaoAutomaticaAtiva = command.SincronizacaoAutomaticaAtiva;
        config.AtualizadoEm = DateTime.UtcNow;
    }

    public async Task<string> ObterUrlGoogleOAuthAsync(string redirectUri)
    {
        var (_, escritorioId) = await ObterUsuarioContextoAsync();
        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        var clientId = ObterGoogleClientId(config);
        if (string.IsNullOrWhiteSpace(clientId))
            throw new InvalidOperationException("A integração OAuth não foi configurada no servidor (defina GOOGLE_CALENDAR_CLIENT_ID no arquivo .env).");

        var state = escritorioId.ToString();
        return _googleCalendarService.GerarUrlAutorizacaoOAuth(clientId, redirectUri, state);
    }

    public async Task<ConfiguracaoAgendaEscritorioDto> ConectarGoogleOAuthAsync(ConectarGoogleOAuthCommand command)
    {
        var (usuario, escritorioId) = await ObterUsuarioContextoAsync();
        if (usuario.Role == Roles.Cliente)
            throw new UnauthorizedAccessException("Clientes não possuem permissão para configurar a agenda corporativa do escritório.");

        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        var clientId = ObterGoogleClientId(config);
        var clientSecret = ObterGoogleClientSecret(config);

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("As credenciais do Google OAuth (Client ID e Secret) não estão configuradas no arquivo .env do servidor.");

        var (refreshToken, email) = await _googleCalendarService.TrocarCodigoPorRefreshTokenAsync(
            command.Code,
            clientId,
            clientSecret,
            command.RedirectUri);

        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new InvalidOperationException("Não foi possível obter o token de autorização do Google. Verifique o código e credenciais.");

        var emailFinal = !string.IsNullOrWhiteSpace(email) ? email : "agenda-oauth@google.com";

        if (config is null)
        {
            config = new ConfiguracaoAgendaEscritorio
            {
                Id = Guid.NewGuid(),
                EscritorioId = escritorioId,
                EmailAgendaEmpresa = emailFinal,
                GoogleCalendarId = "primary",
                GoogleOAuthRefreshToken = refreshToken,
                GoogleOAuthEmail = emailFinal,
                GoogleClientId = clientId,
                GoogleClientSecret = clientSecret,
                TipoIntegracao = IntegracaoOAuth,
                NomeAgenda = "Agenda Google Corporativa (OAuth)",
                SincronizacaoAutomaticaAtiva = true,
                ConectadoEm = DateTime.UtcNow
            };
            await _configuracaoAgendaRepository.Create(config);
        }
        else
        {
            config.GoogleOAuthRefreshToken = refreshToken;
            config.GoogleOAuthEmail = emailFinal;
            config.GoogleClientId = clientId;
            config.GoogleClientSecret = clientSecret;
            config.TipoIntegracao = IntegracaoOAuth;
            if (string.IsNullOrWhiteSpace(config.GoogleCalendarId))
            {
                config.GoogleCalendarId = "primary";
            }
            if (string.IsNullOrWhiteSpace(config.EmailAgendaEmpresa))
            {
                config.EmailAgendaEmpresa = emailFinal;
            }
            config.AtualizadoEm = DateTime.UtcNow;
            await _configuracaoAgendaRepository.Update(config);
        }

        await _unitOfWork.Commit();
        return MapearConfiguracao(config);
    }

    public async Task DesconectarGoogleOAuthAsync()
    {
        var (usuario, escritorioId) = await ObterUsuarioContextoAsync();
        if (usuario.Role == Roles.Cliente)
            throw new UnauthorizedAccessException("Clientes não possuem permissão para configurar a agenda.");

        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        if (config is null) return;

        config.GoogleOAuthRefreshToken = null;
        config.GoogleOAuthEmail = null;
        if (config.TipoIntegracao == IntegracaoOAuth)
        {
            config.TipoIntegracao = !string.IsNullOrWhiteSpace(config.ChaveGoogleServiceAccountJson) ? "ServiceAccount" : "Nenhum";
        }
        config.AtualizadoEm = DateTime.UtcNow;

        await _configuracaoAgendaRepository.Update(config);
        await _unitOfWork.Commit();
    }

    private string? ObterGoogleClientId(ConfiguracaoAgendaEscritorio? config) =>
        config?.GoogleClientId ??
        _configuration?["GOOGLE_CALENDAR_CLIENT_ID"] ??
        _configuration?["GoogleCalendar:ClientId"];

    private string? ObterGoogleClientSecret(ConfiguracaoAgendaEscritorio? config) =>
        config?.GoogleClientSecret ??
        _configuration?["GOOGLE_CALENDAR_CLIENT_SECRET"] ??
        _configuration?["GoogleCalendar:ClientSecret"];

    private static ConfiguracaoAgendaEscritorioDto MapearConfiguracao(ConfiguracaoAgendaEscritorio config)
    {
        var calId = !string.IsNullOrWhiteSpace(config.GoogleCalendarId) ? config.GoogleCalendarId : config.EmailAgendaEmpresa;
        var linkEmbed = GerarLinkEmbedGoogleCalendar(calId);

        return new ConfiguracaoAgendaEscritorioDto
        {
            Id = config.Id,
            EscritorioId = config.EscritorioId,
            EmailAgendaEmpresa = config.EmailAgendaEmpresa,
            GoogleCalendarId = config.GoogleCalendarId,
            ChaveGoogleServiceAccountJson = config.ChaveGoogleServiceAccountJson,
            PossuiChaveServiceAccount = !string.IsNullOrWhiteSpace(config.ChaveGoogleServiceAccountJson),
            GoogleOAuthEmail = config.GoogleOAuthEmail,
            PossuiOAuthConectado = !string.IsNullOrWhiteSpace(config.GoogleOAuthRefreshToken),
            GoogleClientId = config.GoogleClientId,
            TipoIntegracao = string.IsNullOrWhiteSpace(config.TipoIntegracao) ? "ServiceAccount" : config.TipoIntegracao,
            NomeAgenda = config.NomeAgenda,
            SincronizacaoAutomaticaAtiva = config.SincronizacaoAutomaticaAtiva,
            LinkEmbedGoogleCalendar = linkEmbed,
            ConectadoEm = config.ConectadoEm,
            AtualizadoEm = config.AtualizadoEm
        };
    }

    private static string GerarLinkEmbedGoogleCalendar(string calId)
    {
        if (string.IsNullOrWhiteSpace(calId))
            return string.Empty;

        if (calId.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            calId.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return calId;
        }

        return $"https://calendar.google.com/calendar/embed?src={Uri.EscapeDataString(calId)}&ctz=America%2FSao_Paulo";
    }

    public async Task<string> ObterLinkCompartilhadoGoogleAgendaAsync()
    {
        var (_, escritorioId) = await ObterUsuarioContextoAsync();
        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        if (config is null || (string.IsNullOrWhiteSpace(config.EmailAgendaEmpresa) && string.IsNullOrWhiteSpace(config.GoogleCalendarId)))
            return string.Empty;

        var calId = !string.IsNullOrWhiteSpace(config.GoogleCalendarId) ? config.GoogleCalendarId : config.EmailAgendaEmpresa;
        return GerarLinkEmbedGoogleCalendar(calId);
    }
}
