using ArchiFlow.Application.Dashboard.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Dashboard;
using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Leads.Enum;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Domain.Shared;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Dashboard.Services;

public class DashboardService : IDashboardService
{
    private static readonly Dictionary<StatusProjeto, string> StatusProjetoNomes = new()
    {
        { StatusProjeto.Briefing, "Briefing" },
        { StatusProjeto.Desenvolvimento, "Desenvolvimento" },
        { StatusProjeto.Revisao, "Revisão" },
        { StatusProjeto.Aprovacao, "Aprovação" },
        { StatusProjeto.Execucao, "Execução" },
        { StatusProjeto.Concluido, "Concluído" },
        { StatusProjeto.Cancelado, "Cancelado" }
    };

    private static readonly Dictionary<TipoProjeto, string> TipoProjetoNomes = new()
    {
        { TipoProjeto.Residencial, "Residencial" },
        { TipoProjeto.Comercial, "Comercial" },
        { TipoProjeto.Corporativo, "Corporativo" },
        { TipoProjeto.Interiores, "Design de Interiores" }
    };

    private static readonly Dictionary<StatusLead, string> StatusLeadNomes = new()
    {
        { StatusLead.Novo, "Novo Lead" },
        { StatusLead.EmContato, "Em Contato" },
        { StatusLead.PropostaEnviada, "Proposta Enviada" },
        { StatusLead.Negociando, "Em Negociação" },
        { StatusLead.Convertido, "Convertido em Cliente" },
        { StatusLead.Perdido, "Oportunidade Perdida" }
    };

    private static readonly Dictionary<StatusProposta, string> StatusPropostaNomes = new()
    {
        { StatusProposta.Rascunho, "Rascunho" },
        { StatusProposta.Enviada, "Enviada" },
        { StatusProposta.Aprovada, "Aprovada" },
        { StatusProposta.Recusada, "Recusada" }
    };

    private readonly IProjetoRepository _projetoRepo;
    private readonly IClienteRepository _clienteRepo;
    private readonly ILeadRepository _leadRepo;
    private readonly IPropostaHonorarioRepository _propostaRepo;
    private readonly IPreferenciaDashboardRepository _preferenciaRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public DashboardService(
        IProjetoRepository projetoRepo,
        IClienteRepository clienteRepo,
        ILeadRepository leadRepo,
        IPropostaHonorarioRepository propostaRepo,
        IPreferenciaDashboardRepository preferenciaRepo,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _projetoRepo = projetoRepo;
        _clienteRepo = clienteRepo;
        _leadRepo = leadRepo;
        _propostaRepo = propostaRepo;
        _preferenciaRepo = preferenciaRepo;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<DashboardMetricasDto> ObterMetricasAsync()
    {
        var projetosEnumerable = await _projetoRepo.GetAllParaDashboardAsync();
        var projetos = projetosEnumerable.OrderByDescending(p => p.CriadoEm).ToList();

        var clientesEnumerable = await _clienteRepo.GetAll();
        var clientes = clientesEnumerable.ToList();

        var leadsEnumerable = await _leadRepo.GetAllParaDashboardAsync();
        var leads = leadsEnumerable.OrderByDescending(l => l.CriadoEm).ToList();

        var propostasEnumerable = await _propostaRepo.GetAll();
        var propostas = propostasEnumerable.OrderByDescending(p => p.CriadoEm).ToList();

        var kpis = CalcularKpis(projetos, clientes, leads, propostas);
        var projetosPorStatus = CalcularProjetosPorStatus(projetos);
        var projetosPorTipo = CalcularProjetosPorTipo(projetos);
        var leadsPorStatus = CalcularLeadsPorStatus(leads);
        var leadsPorOrigem = CalcularLeadsPorOrigem(leads);
        var propostasMensais = CalcularPropostasMensais(propostas);

        var clientesDict = clientes.ToDictionary(c => c.Id, c => c.Nome);
        var projetosRecentes = MapearProjetosRecentes(projetos, clientesDict);
        var leadsRecentes = MapearLeadsRecentes(leads);
        var propostasRecentes = MapearPropostasRecentes(propostas);

        return new DashboardMetricasDto(
            kpis,
            projetosPorStatus,
            projetosPorTipo,
            leadsPorStatus,
            leadsPorOrigem,
            propostasMensais,
            projetosRecentes,
            leadsRecentes,
            propostasRecentes
        );
    }

    /// <summary>
    /// Calcula os KPIs executivos do dashboard.
    /// Regra de Negócio para Receita Potencial (ValorTotalPropostas):
    /// Totaliza o valor de todas as propostas registradas no pipeline.
    /// Utiliza ValorFinalAjustado caso tenha sido customizado (> 0); caso contrário, adota ValorTotalSugerido.
    /// </summary>
    private static DashboardKpisDto CalcularKpis(
        List<Projeto> projetos,
        List<Cliente> clientes,
        List<Lead> leads,
        List<PropostaHonorario> propostas)
    {
        var totalProjetosAtivos = projetos.Count(p => p.Status != StatusProjeto.Concluido && p.Status != StatusProjeto.Cancelado);
        var totalProjetosConcluidos = projetos.Count(p => p.Status == StatusProjeto.Concluido);

        var totalLeads = leads.Count;
        var totalLeadsAtivos = leads.Count(l => l.Status != StatusLead.Convertido && l.Status != StatusLead.Perdido);
        var totalLeadsConvertidos = leads.Count(l => l.Status == StatusLead.Convertido);
        var taxaConversao = totalLeads > 0
            ? Math.Round((decimal)totalLeadsConvertidos / totalLeads * 100m, 1)
            : 0m;

        var totalClientes = clientes.Count(c => c.Ativo);

        var totalPropostas = propostas.Count;
        var valorTotalPropostas = propostas.Sum(p => p.ValorFinalAjustado > 0 ? p.ValorFinalAjustado : p.ValorTotalSugerido);
        var valorMedioProposta = totalPropostas > 0
            ? Math.Round(valorTotalPropostas / totalPropostas, 2)
            : 0m;

        return new DashboardKpisDto(
            totalProjetosAtivos,
            totalProjetosConcluidos,
            totalLeadsAtivos,
            totalLeadsConvertidos,
            taxaConversao,
            totalClientes,
            totalPropostas,
            valorTotalPropostas,
            valorMedioProposta
        );
    }

    private static List<ProjetosPorStatusDto> CalcularProjetosPorStatus(List<Projeto> projetos)
    {
        var totalProjetos = projetos.Count;
        return Enum.GetValues<StatusProjeto>()
            .Select(status =>
            {
                var qtd = projetos.Count(p => p.Status == status);
                var pct = totalProjetos > 0 ? Math.Round((decimal)qtd / totalProjetos * 100m, 1) : 0m;
                var nome = StatusProjetoNomes.TryGetValue(status, out var n) ? n : status.ToString();
                return new ProjetosPorStatusDto(status.ToString(), nome, qtd, pct);
            })
            .ToList();
    }

    private static List<ProjetosPorTipoDto> CalcularProjetosPorTipo(List<Projeto> projetos)
    {
        var totalProjetos = projetos.Count;
        return Enum.GetValues<TipoProjeto>()
            .Select(tipo =>
            {
                var qtd = projetos.Count(p => p.Tipo == tipo);
                var pct = totalProjetos > 0 ? Math.Round((decimal)qtd / totalProjetos * 100m, 1) : 0m;
                var nome = TipoProjetoNomes.TryGetValue(tipo, out var n) ? n : tipo.ToString();
                return new ProjetosPorTipoDto(tipo.ToString(), nome, qtd, pct);
            })
            .ToList();
    }

    private static List<LeadsPorStatusDto> CalcularLeadsPorStatus(List<Lead> leads)
    {
        var totalLeads = leads.Count;
        return Enum.GetValues<StatusLead>()
            .Select(status =>
            {
                var qtd = leads.Count(l => l.Status == status);
                var pct = totalLeads > 0 ? Math.Round((decimal)qtd / totalLeads * 100m, 1) : 0m;
                var nome = StatusLeadNomes.TryGetValue(status, out var n) ? n : status.ToString();
                return new LeadsPorStatusDto(status.ToString(), nome, qtd, pct);
            })
            .ToList();
    }

    private static List<LeadsPorOrigemDto> CalcularLeadsPorOrigem(List<Lead> leads)
    {
        var totalLeads = leads.Count;
        var lista = leads
            .GroupBy(l => string.IsNullOrWhiteSpace(l.Origem?.Descricao) ? "Direto" : l.Origem.Descricao)
            .Select(g =>
            {
                var qtd = g.Count();
                var pct = totalLeads > 0 ? Math.Round((decimal)qtd / totalLeads * 100m, 1) : 0m;
                return new LeadsPorOrigemDto(g.Key, qtd, pct);
            })
            .OrderByDescending(o => o.Quantidade)
            .ToList();

        if (lista.Count == 0)
        {
            lista.Add(new LeadsPorOrigemDto("Sem registros", 0, 0m));
        }

        return lista;
    }

    private static List<PropostasMensalDto> CalcularPropostasMensais(List<PropostaHonorario> propostas)
    {
        var hoje = DateTime.UtcNow;
        var ptBr = new CultureInfo("pt-BR");
        var ultimos6Meses = Enumerable.Range(0, 6)
            .Select(i => hoje.AddMonths(-5 + i))
            .ToList();

        return ultimos6Meses.Select(dataRef =>
        {
            var mesAnoKey = dataRef.ToString("yyyy-MM");
            var rotulo = dataRef.ToString("MMM/yy", ptBr);
            rotulo = char.ToUpper(rotulo[0]) + rotulo.Substring(1);

            var propostasDoMes = propostas.Where(p => p.CriadoEm.Year == dataRef.Year && p.CriadoEm.Month == dataRef.Month).ToList();
            var qtd = propostasDoMes.Count;
            var val = propostasDoMes.Sum(p => p.ValorFinalAjustado > 0 ? p.ValorFinalAjustado : p.ValorTotalSugerido);

            return new PropostasMensalDto(mesAnoKey, rotulo, qtd, val);
        }).ToList();
    }

    private static List<ProjetoResumoDashboardDto> MapearProjetosRecentes(
        List<Projeto> projetos,
        Dictionary<Guid, string> clientesDict)
    {
        return projetos
            .Take(5)
            .Select(p =>
            {
                var clienteNome = p.ClienteId.HasValue && clientesDict.TryGetValue(p.ClienteId.Value, out var cNome)
                    ? cNome
                    : "Cliente não associado";

                var totalEtapas = p.Etapas?.Count ?? 0;
                var etapasConcluidas = p.Etapas?.Count(e => e.Status == StatusEtapa.Concluida) ?? 0;
                var progresso = totalEtapas > 0
                    ? Math.Round((decimal)etapasConcluidas / totalEtapas * 100m, 0)
                    : 0m;

                var status = p.Status ?? StatusProjeto.Briefing;
                var tipo = p.Tipo ?? TipoProjeto.Residencial;

                return new ProjetoResumoDashboardDto(
                    p.Id,
                    p.Nome ?? "Projeto sem nome",
                    clienteNome,
                    StatusProjetoNomes.TryGetValue(status, out var st) ? st : status.ToString(),
                    TipoProjetoNomes.TryGetValue(tipo, out var tp) ? tp : tipo.ToString(),
                    p.MetragemTotal,
                    totalEtapas,
                    etapasConcluidas,
                    progresso,
                    p.DataInicio,
                    p.DataPrevistaEntrega
                );
            })
            .ToList();
    }

    private static List<LeadResumoDashboardDto> MapearLeadsRecentes(List<Lead> leads)
    {
        return leads
            .Take(5)
            .Select(l => new LeadResumoDashboardDto(
                l.Id,
                l.Nome,
                l.Email,
                l.Telefone,
                StatusLeadNomes.TryGetValue(l.Status, out var st) ? st : l.Status.ToString(),
                l.Origem?.Descricao ?? "Direto",
                l.CriadoEm
            ))
            .ToList();
    }

    private static List<PropostaResumoDashboardDto> MapearPropostasRecentes(List<PropostaHonorario> propostas)
    {
        return propostas
            .Take(5)
            .Select(p => new PropostaResumoDashboardDto(
                p.Id,
                p.Titulo,
                p.Codigo,
                ObterDestinatarioProposta(p),
                p.MetragemQuadrada,
                p.ValorFinalAjustado > 0 ? p.ValorFinalAjustado : p.ValorTotalSugerido,
                StatusPropostaNomes.TryGetValue(p.Status, out var st) ? st : p.Status.ToString(),
                p.CriadoEm
            ))
            .ToList();
    }

    private static string ObterDestinatarioProposta(PropostaHonorario proposta)
    {
        if (!string.IsNullOrWhiteSpace(proposta.ClienteNome))
        {
            return proposta.ClienteNome;
        }

        if (!string.IsNullOrWhiteSpace(proposta.LeadNome))
        {
            return proposta.LeadNome;
        }

        return "Sem destinatário";
    }

    private static readonly HashSet<string> KnownWidgetIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "kpi_resumo",
        "atalhos_rapidos",
        "grafico_projetos_status",
        "grafico_projetos_tipo",
        "grafico_funil_leads",
        "grafico_origens_lead",
        "grafico_propostas_mensal",
        "tabela_projetos_recentes",
        "lista_leads_recentes",
        "lista_propostas_recentes"
    };

    public async Task<PreferenciaDashboardDto?> ObterPreferenciasAsync()
    {
        var usuarioId = ObterUsuarioIdContexto();
        var pref = await _preferenciaRepo.ObterPorUsuarioIdAsync(usuarioId);
        if (pref == null)
            return null;

        return new PreferenciaDashboardDto(pref.UsuarioId, pref.LayoutJson, pref.AtualizadoEm);
    }

    public async Task<PreferenciaDashboardDto> SalvarPreferenciasAsync(SalvarPreferenciaDashboardCommand command)
    {
        var usuarioId = ObterUsuarioIdContexto();
        ValidarLayoutJson(command.LayoutJson);

        var pref = await _preferenciaRepo.ObterPorUsuarioIdAsync(usuarioId);

        if (pref == null)
        {
            pref = new PreferenciaDashboard
            {
                UsuarioId = usuarioId,
                LayoutJson = command.LayoutJson,
                AtualizadoEm = DateTime.UtcNow
            };
            await _preferenciaRepo.Create(pref);
        }
        else
        {
            pref.LayoutJson = command.LayoutJson;
            pref.AtualizadoEm = DateTime.UtcNow;
            await _preferenciaRepo.Update(pref);
        }

        await _unitOfWork.Commit();

        return new PreferenciaDashboardDto(pref.UsuarioId, pref.LayoutJson, pref.AtualizadoEm);
    }

    private static void ValidarLayoutJson(string? layoutJson)
    {
        if (string.IsNullOrWhiteSpace(layoutJson))
            throw new ArgumentException("O layout JSON não pode ser vazio.");

        if (layoutJson.Length > 16384)
            throw new ArgumentException("O tamanho do layout JSON excede o limite máximo permitido de 16KB.");

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(layoutJson);
        }
        catch (JsonException)
        {
            throw new ArgumentException("O formato do layout não é um JSON válido.");
        }

        using (doc)
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
                throw new ArgumentException("O layout deve ser uma lista (array) de widgets.");

            var arrayLength = doc.RootElement.GetArrayLength();
            if (arrayLength == 0)
                throw new ArgumentException("O layout deve conter pelo menos um widget configurado.");

            if (arrayLength > 20)
                throw new ArgumentException("O layout não pode conter mais de 20 widgets.");

            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                    throw new ArgumentException("Cada item do layout deve ser um objeto representando um widget.");

                if (!element.TryGetProperty("id", out var idProp) || idProp.ValueKind != JsonValueKind.String)
                    throw new ArgumentException("Todo widget deve conter a propriedade 'id' do tipo string.");

                var widgetId = idProp.GetString()?.Trim();
                if (string.IsNullOrEmpty(widgetId) || !KnownWidgetIds.Contains(widgetId))
                    throw new ArgumentException($"O identificador de widget '{widgetId}' é desconhecido ou inválido.");

                if (!seenIds.Add(widgetId))
                    throw new ArgumentException($"O widget '{widgetId}' está duplicado no layout.");

                if (element.TryGetProperty("ordem", out var ordemProp))
                {
                    if (ordemProp.ValueKind != JsonValueKind.Number || !ordemProp.TryGetInt32(out var ordem) || ordem < 1 || ordem > 50)
                        throw new ArgumentException($"A ordem do widget '{widgetId}' deve ser um número inteiro entre 1 e 50.");
                }

                if (element.TryGetProperty("visivel", out var visivelProp))
                {
                    if (visivelProp.ValueKind != JsonValueKind.True && visivelProp.ValueKind != JsonValueKind.False)
                        throw new ArgumentException($"A propriedade 'visivel' do widget '{widgetId}' deve ser um booleano.");
                }

                if (element.TryGetProperty("largura", out var larguraProp))
                {
                    if (larguraProp.ValueKind != JsonValueKind.String)
                        throw new ArgumentException($"A propriedade 'largura' do widget '{widgetId}' deve ser uma string.");

                    var largura = larguraProp.GetString()?.Trim().ToLowerInvariant();
                    if (largura != "full" && largura != "half")
                        throw new ArgumentException($"A largura do widget '{widgetId}' deve ser 'full' ou 'half'.");
                }
            }
        }
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
}