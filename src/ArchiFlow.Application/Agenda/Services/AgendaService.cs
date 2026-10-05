using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiFlow.Application.Agenda.Commands;
using ArchiFlow.Application.Agenda.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Agenda;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Domain.Usuarios;

namespace ArchiFlow.Application.Agenda.Services;

public class AgendaService : IAgendaService
{
    private const string MensagemSemPermissao = "Você não possui permissão para gerenciar este compromisso.";
    private const string IntegracaoOAuth = "OAuth";
    private const string PrimaryCalendarId = "primary";

    private readonly ICompromissoRepository _compromissoRepository;
    private readonly IConfiguracaoAgendaRepository _configuracaoAgendaRepository;
    private readonly IGoogleCalendarService _googleCalendarService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContextService _userContextService;
    private readonly IAgendaValidationService _validationService;
    private readonly IOAuthStateService _oauthStateService;

    public AgendaService(
        ICompromissoRepository compromissoRepository,
        IConfiguracaoAgendaRepository configuracaoAgendaRepository,
        IGoogleCalendarService googleCalendarService,
        IUnitOfWork unitOfWork,
        IUserContextService userContextService,
        IAgendaValidationService validationService,
        IOAuthStateService oauthStateService)
    {
        _compromissoRepository = compromissoRepository;
        _configuracaoAgendaRepository = configuracaoAgendaRepository;
        _googleCalendarService = googleCalendarService;
        _unitOfWork = unitOfWork;
        _userContextService = userContextService;
        _validationService = validationService;
        _oauthStateService = oauthStateService;
    }

    public async Task<IEnumerable<CompromissoDto>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim, Guid? usuarioId = null, Guid? projetoId = null)
    {
        if (inicio > fim)
            throw new ArgumentException("A data inicial não pode ser superior à data final.");

        var (_, escritorioId) = await _userContextService.ObterUsuarioContextoAsync();
        var compromissos = await _compromissoRepository.ObterPorPeriodoAsync(escritorioId, inicio, fim, usuarioId, projetoId);

        return await MapearListaAsync(compromissos, escritorioId);
    }

    public async Task<IEnumerable<CompromissoDto>> ObterProximosAsync(int quantidade = 10, Guid? usuarioId = null)
    {
        var count = Math.Clamp(quantidade, 1, 50);
        var (_, escritorioId) = await _userContextService.ObterUsuarioContextoAsync();
        var compromissos = await _compromissoRepository.ObterProximosAsync(escritorioId, count, usuarioId);

        return await MapearListaAsync(compromissos, escritorioId);
    }

    public async Task<CompromissoDto?> ObterPorIdAsync(Guid id)
    {
        var (_, escritorioId) = await _userContextService.ObterUsuarioContextoAsync();
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

        var (usuarioLogado, escritorioId) = await _userContextService.ObterUsuarioContextoAsync();

        await _validationService.ValidarEntidadesRelacionadasAsync(
            escritorioId,
            command.UsuarioId,
            command.ProjetoId,
            command.ClienteId,
            command.LeadId);

        var tipo = !string.IsNullOrWhiteSpace(command.Tipo) && TiposCompromisso.Todos.Contains(command.Tipo)
            ? command.Tipo
            : TiposCompromisso.Geral;

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
            LinkGoogleMeet = null,
            CriadoEm = DateTime.UtcNow
        };

        // Persistência local ocorre primeiro para prevenir eventos órfãos no Google se o banco falhar
        await _compromissoRepository.Create(compromisso);
        await _unitOfWork.Commit();

        // Sincronização externa somente após confirmação do commit local
        var syncResult = await SincronizarCriacaoComGoogleCalendarAsync(compromisso, escritorioId, command.GerarGoogleMeet);
        if (syncResult is not null && (!string.IsNullOrWhiteSpace(syncResult.GoogleEventId) || !string.IsNullOrWhiteSpace(syncResult.LinkGoogleMeet)))
        {
            compromisso.GoogleEventId = syncResult.GoogleEventId;
            compromisso.LinkGoogleMeet = syncResult.LinkGoogleMeet;
            await _compromissoRepository.Update(compromisso);
            await _unitOfWork.Commit();
        }

        return await MapearAsync(compromisso);
    }

    public async Task<CompromissoDto> AtualizarCompromissoAsync(Guid id, AtualizarCompromissoCommand command)
    {
        var dataInicioUtc = GarantirUtc(command.DataHoraInicio);
        var dataFimUtc = GarantirUtc(command.DataHoraFim);
        ValidarHorarios(dataInicioUtc, dataFimUtc);

        var (_, escritorioId) = await _userContextService.ObterUsuarioContextoAsync();
        var compromisso = await _compromissoRepository.GetById(id);
        if (compromisso is null)
            throw new KeyNotFoundException($"Compromisso com ID {id} não encontrado.");

        if (compromisso.EscritorioId != escritorioId)
            throw new UnauthorizedAccessException(MensagemSemPermissao);

        await _validationService.ValidarEntidadesRelacionadasAsync(
            escritorioId,
            command.UsuarioId,
            command.ProjetoId,
            command.ClienteId,
            command.LeadId);

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
        if (command.LinkGoogleMeet != null)
        {
            compromisso.LinkGoogleMeet = string.IsNullOrWhiteSpace(command.LinkGoogleMeet) ? null : command.LinkGoogleMeet.Trim();
        }
        compromisso.ProjetoId = command.ProjetoId;
        compromisso.ClienteId = command.ClienteId;
        compromisso.LeadId = command.LeadId;
        compromisso.UsuarioId = command.UsuarioId ?? compromisso.UsuarioId;
        compromisso.AtualizadoEm = DateTime.UtcNow;

        await _compromissoRepository.Update(compromisso);
        await _unitOfWork.Commit();

        if (!string.IsNullOrWhiteSpace(compromisso.GoogleEventId))
        {
            await SincronizarAtualizacaoComGoogleCalendarAsync(compromisso, escritorioId);
        }

        return await MapearAsync(compromisso);
    }

    public async Task<CompromissoDto> AlterarStatusCompromissoAsync(Guid id, AlterarStatusCompromissoCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Status) || !StatusCompromisso.Todos.Contains(command.Status))
            throw new ArgumentException($"Status '{command.Status}' inválido.");

        var (_, escritorioId) = await _userContextService.ObterUsuarioContextoAsync();
        var compromisso = await _compromissoRepository.GetById(id);
        if (compromisso is null)
            throw new KeyNotFoundException($"Compromisso com ID {id} não encontrado.");

        if (compromisso.EscritorioId != escritorioId)
            throw new UnauthorizedAccessException(MensagemSemPermissao);

        compromisso.Status = command.Status;
        compromisso.AtualizadoEm = DateTime.UtcNow;

        await _compromissoRepository.Update(compromisso);
        await _unitOfWork.Commit();

        if (!string.IsNullOrWhiteSpace(compromisso.GoogleEventId))
        {
            await SincronizarAtualizacaoComGoogleCalendarAsync(compromisso, escritorioId);
        }

        return await MapearAsync(compromisso);
    }

    public async Task ExcluirCompromissoAsync(Guid id)
    {
        var (_, escritorioId) = await _userContextService.ObterUsuarioContextoAsync();
        var compromisso = await _compromissoRepository.GetById(id);
        if (compromisso is null)
            throw new KeyNotFoundException($"Compromisso com ID {id} não encontrado.");

        if (compromisso.EscritorioId != escritorioId)
            throw new UnauthorizedAccessException(MensagemSemPermissao);

        if (!string.IsNullOrWhiteSpace(compromisso.GoogleEventId))
        {
            await SincronizarExclusaoComGoogleCalendarAsync(compromisso, escritorioId);
        }

        await _compromissoRepository.Delete(id);
        await _unitOfWork.Commit();
    }

    public async Task<string> ExportarIcsAsync(DateTime? inicio = null, DateTime? fim = null)
    {
        var (_, escritorioId) = await _userContextService.ObterUsuarioContextoAsync();
        var dataInicio = inicio ?? DateTime.UtcNow.AddMonths(-1);
        var dataFim = fim ?? DateTime.UtcNow.AddMonths(3);

        var compromissos = await _compromissoRepository.ObterPorPeriodoAsync(escritorioId, dataInicio, dataFim);
        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        var nomeCalendario = config?.NomeAgenda ?? "ArchiFlow - Agenda Corporativa";

        return _googleCalendarService.ExportarIcs(compromissos, nomeCalendario);
    }

    public async Task<ConfiguracaoAgendaEscritorioDto?> ObterConfiguracaoAgendaEscritorioAsync()
    {
        var (_, escritorioId) = await _userContextService.ObterUsuarioContextoAsync();
        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        if (config is null)
            return null;

        return MapearConfiguracao(config);
    }

    public async Task<ConfiguracaoAgendaEscritorioDto> SalvarConfiguracaoAgendaEscritorioAsync(SalvarConfiguracaoAgendaEscritorioCommand command)
    {
        var (usuario, escritorioId) = await _userContextService.ObterUsuarioContextoAsync();
        if (usuario.Role == Roles.Cliente)
            throw new UnauthorizedAccessException("Clientes não possuem permissão para configurar a agenda.");

        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        var calId = !string.IsNullOrWhiteSpace(command.GoogleCalendarId)
            ? command.GoogleCalendarId.Trim()
            : command.EmailAgendaEmpresa?.Trim();

        if (config is null)
        {
            config = new ConfiguracaoAgendaEscritorio
            {
                Id = Guid.NewGuid(),
                EscritorioId = escritorioId,
                EmailAgendaEmpresa = command.EmailAgendaEmpresa?.Trim() ?? string.Empty,
                GoogleCalendarId = calId,
                ChaveGoogleServiceAccountJson = command.ChaveGoogleServiceAccountJson?.Trim(),
                GoogleClientId = command.GoogleClientId?.Trim(),
                GoogleClientSecret = command.GoogleClientSecret?.Trim(),
                TipoIntegracao = !string.IsNullOrWhiteSpace(command.TipoIntegracao) ? command.TipoIntegracao.Trim() : "ServiceAccount",
                NomeAgenda = string.IsNullOrWhiteSpace(command.NomeAgenda) ? "Agenda Oficial do Escritório" : command.NomeAgenda.Trim(),
                SincronizacaoAutomaticaAtiva = command.SincronizacaoAutomaticaAtiva,
                ConectadoEm = DateTime.UtcNow
            };
            await _configuracaoAgendaRepository.Create(config);
        }
        else
        {
            AplicarAtualizacaoConfiguracao(config, command, calId);
            await _configuracaoAgendaRepository.Update(config);
        }

        await _unitOfWork.Commit();
        return MapearConfiguracao(config);
    }

    public async Task<string> ObterUrlGoogleOAuthAsync(string redirectUri)
    {
        var (usuario, escritorioId) = await _userContextService.ObterUsuarioContextoAsync();
        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        var clientId = ObterGoogleClientId(config);
        if (string.IsNullOrWhiteSpace(clientId))
            throw new InvalidOperationException("A integração OAuth não foi configurada no servidor (defina GOOGLE_CALENDAR_CLIENT_ID no arquivo .env).");

        var state = _oauthStateService.GerarState(usuario.Id, escritorioId);
        return _googleCalendarService.GerarUrlAutorizacaoOAuth(clientId, redirectUri, state);
    }

    public async Task<ConfiguracaoAgendaEscritorioDto> ConectarGoogleOAuthAsync(ConectarGoogleOAuthCommand command)
    {
        var (usuario, escritorioId) = await _userContextService.ObterUsuarioContextoAsync();
        if (usuario.Role == Roles.Cliente)
            throw new UnauthorizedAccessException("Clientes não possuem permissão para configurar a agenda corporativa do escritório.");

        if (!_oauthStateService.ValidarEConsumirState(command.State, usuario.Id, escritorioId))
            throw new InvalidOperationException("Estado OAuth inválido, expirado ou já utilizado. Por favor, reinicie a autorização com o Google.");

        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        var clientId = ObterGoogleClientId(config);
        var clientSecret = ObterGoogleClientSecret(config);

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("As credenciais do Google OAuth (Client ID e Secret) não estão configuradas no servidor.");

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
                GoogleCalendarId = PrimaryCalendarId,
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
                config.GoogleCalendarId = PrimaryCalendarId;
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
        var (usuario, escritorioId) = await _userContextService.ObterUsuarioContextoAsync();
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

    public async Task<string> ObterLinkCompartilhadoGoogleAgendaAsync()
    {
        var (_, escritorioId) = await _userContextService.ObterUsuarioContextoAsync();
        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        if (config is null || (string.IsNullOrWhiteSpace(config.EmailAgendaEmpresa) && string.IsNullOrWhiteSpace(config.GoogleCalendarId)))
            return string.Empty;

        var calId = !string.IsNullOrWhiteSpace(config.GoogleCalendarId) ? config.GoogleCalendarId : config.EmailAgendaEmpresa;
        return GerarLinkEmbedGoogleCalendar(calId);
    }

    private async Task<GoogleCalendarSyncResult?> SincronizarCriacaoComGoogleCalendarAsync(Compromisso compromisso, Guid escritorioId, bool gerarMeet)
    {
        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        if (config == null || !config.SincronizacaoAutomaticaAtiva) return null;

        var targetCalId = ObterCalendarIdAlvo(config);

        var (nomeProjeto, nomeCliente, nomeLead, _) = await _validationService.ObterNomesRelacionadosAsync(
            compromisso.ProjetoId, compromisso.ClienteId, compromisso.LeadId, compromisso.UsuarioId);

        var request = new GoogleCalendarEventRequest
        {
            Compromisso = compromisso,
            CalendarId = targetCalId,
            NomeProjeto = nomeProjeto,
            NomeCliente = nomeCliente,
            NomeLead = nomeLead,
            SolicitarGoogleMeet = gerarMeet
        };

        if (config.TipoIntegracao == IntegracaoOAuth && !string.IsNullOrWhiteSpace(config.GoogleOAuthRefreshToken))
        {
            var clientId = ObterGoogleClientId(config) ?? string.Empty;
            var clientSecret = ObterGoogleClientSecret(config) ?? string.Empty;

            return await _googleCalendarService.CriarEventoViaOAuthAsync(
                request,
                config.GoogleOAuthRefreshToken,
                clientId,
                clientSecret);
        }
        else if (!string.IsNullOrWhiteSpace(config.ChaveGoogleServiceAccountJson))
        {
            return await _googleCalendarService.CriarEventoDiretoNoGoogleCalendarAsync(
                request,
                config.ChaveGoogleServiceAccountJson);
        }

        return null;
    }

    private async Task SincronizarAtualizacaoComGoogleCalendarAsync(Compromisso compromisso, Guid escritorioId)
    {
        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        if (config == null || string.IsNullOrWhiteSpace(compromisso.GoogleEventId)) return;

        var targetCalId = ObterCalendarIdAlvo(config);

        var (nomeProjeto, nomeCliente, nomeLead, _) = await _validationService.ObterNomesRelacionadosAsync(
            compromisso.ProjetoId, compromisso.ClienteId, compromisso.LeadId, compromisso.UsuarioId);

        var request = new GoogleCalendarEventRequest
        {
            Compromisso = compromisso,
            CalendarId = targetCalId,
            NomeProjeto = nomeProjeto,
            NomeCliente = nomeCliente,
            NomeLead = nomeLead,
            SolicitarGoogleMeet = false
        };

        if (config.TipoIntegracao == IntegracaoOAuth && !string.IsNullOrWhiteSpace(config.GoogleOAuthRefreshToken))
        {
            var clientId = ObterGoogleClientId(config) ?? string.Empty;
            var clientSecret = ObterGoogleClientSecret(config) ?? string.Empty;
            await _googleCalendarService.AtualizarEventoViaOAuthAsync(request, config.GoogleOAuthRefreshToken, clientId, clientSecret);
        }
        else if (!string.IsNullOrWhiteSpace(config.ChaveGoogleServiceAccountJson))
        {
            await _googleCalendarService.AtualizarEventoDiretoAsync(request, config.ChaveGoogleServiceAccountJson);
        }
    }

    private async Task SincronizarExclusaoComGoogleCalendarAsync(Compromisso compromisso, Guid escritorioId)
    {
        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        if (config == null || string.IsNullOrWhiteSpace(compromisso.GoogleEventId)) return;

        var targetCalId = ObterCalendarIdAlvo(config);

        if (config.TipoIntegracao == IntegracaoOAuth && !string.IsNullOrWhiteSpace(config.GoogleOAuthRefreshToken))
        {
            var clientId = ObterGoogleClientId(config) ?? string.Empty;
            var clientSecret = ObterGoogleClientSecret(config) ?? string.Empty;
            await _googleCalendarService.ExcluirEventoViaOAuthAsync(
                compromisso.GoogleEventId,
                targetCalId,
                config.GoogleOAuthRefreshToken,
                clientId,
                clientSecret);
        }
        else if (!string.IsNullOrWhiteSpace(config.ChaveGoogleServiceAccountJson))
        {
            await _googleCalendarService.ExcluirEventoDiretoAsync(
                compromisso.GoogleEventId,
                targetCalId,
                config.ChaveGoogleServiceAccountJson);
        }
    }

    private static string ObterCalendarIdAlvo(ConfiguracaoAgendaEscritorio config)
    {
        if (!string.IsNullOrWhiteSpace(config.GoogleCalendarId))
            return config.GoogleCalendarId;

        if (!string.IsNullOrWhiteSpace(config.EmailAgendaEmpresa))
            return config.EmailAgendaEmpresa;

        return PrimaryCalendarId;
    }

    private static string? ObterGoogleClientId(ConfiguracaoAgendaEscritorio? config) =>
        config?.GoogleClientId ??
        Environment.GetEnvironmentVariable("GOOGLE_CALENDAR_CLIENT_ID");

    private static string? ObterGoogleClientSecret(ConfiguracaoAgendaEscritorio? config) =>
        config?.GoogleClientSecret ??
        Environment.GetEnvironmentVariable("GOOGLE_CALENDAR_CLIENT_SECRET");

    private static void AplicarAtualizacaoConfiguracao(ConfiguracaoAgendaEscritorio config, SalvarConfiguracaoAgendaEscritorioCommand command, string? calId)
    {
        if (!string.IsNullOrWhiteSpace(command.EmailAgendaEmpresa))
        {
            config.EmailAgendaEmpresa = command.EmailAgendaEmpresa.Trim();
        }
        if (!string.IsNullOrWhiteSpace(calId))
        {
            config.GoogleCalendarId = calId;
        }
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

    private static string GerarLinkEmbedGoogleCalendar(string? calId)
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

    private async Task<CompromissoDto> MapearAsync(Compromisso c)
    {
        var (nomeProjeto, nomeCliente, nomeLead, nomeUsuario) = await _validationService.ObterNomesRelacionadosAsync(
            c.ProjetoId, c.ClienteId, c.LeadId, c.UsuarioId);

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

    private async Task<IEnumerable<CompromissoDto>> MapearListaAsync(IEnumerable<Compromisso> lista, Guid escritorioId)
    {
        var items = lista.ToList();
        if (items.Count == 0) return Enumerable.Empty<CompromissoDto>();

        var batch = await _validationService.ObterNomesEmLoteAsync(items);
        var config = await _configuracaoAgendaRepository.ObterPorEscritorioIdAsync(escritorioId);
        var emailEmpresa = config?.EmailAgendaEmpresa;

        var dtos = new List<CompromissoDto>(items.Count);
        foreach (var c in items)
        {
            var nomeProjeto = c.ProjetoId.HasValue && batch.Projetos.TryGetValue(c.ProjetoId.Value, out var np) ? np : null;
            var nomeCliente = c.ClienteId.HasValue && batch.Clientes.TryGetValue(c.ClienteId.Value, out var nc) ? nc : null;
            var nomeLead = c.LeadId.HasValue && batch.Leads.TryGetValue(c.LeadId.Value, out var nl) ? nl : null;
            var nomeUsuario = c.UsuarioId.HasValue && batch.Usuarios.TryGetValue(c.UsuarioId.Value, out var nu) ? nu : null;

            var linkGoogle = _googleCalendarService.GerarLinkWebAdicionarEvento(c, nomeProjeto, nomeCliente, nomeLead, emailEmpresa);

            dtos.Add(new CompromissoDto(
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
            ));
        }

        return dtos;
    }

    private static DateTime GarantirUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    private static void ValidarHorarios(DateTime inicio, DateTime fim)
    {
        if (inicio >= fim)
            throw new ArgumentException("O horário de término deve ser posterior ao horário de início.");
    }
}
