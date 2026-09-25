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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Dashboard.Services;

public class DashboardService : IDashboardService
{
    private readonly IProjetoRepository _projetoRepo;
    private readonly IClienteRepository _clienteRepo;
    private readonly ILeadRepository _leadRepo;
    private readonly IPropostaHonorarioRepository _propostaRepo;
    private readonly IPreferenciaDashboardRepository _preferenciaRepo;
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(
        IProjetoRepository projetoRepo,
        IClienteRepository clienteRepo,
        ILeadRepository leadRepo,
        IPropostaHonorarioRepository propostaRepo,
        IPreferenciaDashboardRepository preferenciaRepo,
        IUnitOfWork unitOfWork)
    {
        _projetoRepo = projetoRepo;
        _clienteRepo = clienteRepo;
        _leadRepo = leadRepo;
        _propostaRepo = propostaRepo;
        _preferenciaRepo = preferenciaRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardMetricasDto> ObterMetricasAsync()
    {
        var projetosEnumerable = await _projetoRepo.GetAllWithEtapas();
        var projetos = projetosEnumerable.OrderByDescending(p => p.CriadoEm).ToList();

        var clientesEnumerable = await _clienteRepo.GetAll();
        var clientes = clientesEnumerable.ToList();

        var leadsEnumerable = await _leadRepo.GetAllWithHistorico();
        var leads = leadsEnumerable.OrderByDescending(l => l.CriadoEm).ToList();

        var propostasEnumerable = await _propostaRepo.GetAll();
        var propostas = propostasEnumerable.OrderByDescending(p => p.CriadoEm).ToList();

        var totalProjetos = projetos.Count;
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

        var kpis = new DashboardKpisDto(
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

        var statusProjetoNomes = new Dictionary<StatusProjeto, string>
        {
            { StatusProjeto.Briefing, "Briefing" },
            { StatusProjeto.Desenvolvimento, "Desenvolvimento" },
            { StatusProjeto.Revisao, "Revisão" },
            { StatusProjeto.Aprovacao, "Aprovação" },
            { StatusProjeto.Execucao, "Execução" },
            { StatusProjeto.Concluido, "Concluído" },
            { StatusProjeto.Cancelado, "Cancelado" }
        };

        var projetosPorStatus = Enum.GetValues<StatusProjeto>()
            .Select(status =>
            {
                var qtd = projetos.Count(p => p.Status == status);
                var pct = totalProjetos > 0 ? Math.Round((decimal)qtd / totalProjetos * 100m, 1) : 0m;
                var nome = statusProjetoNomes.TryGetValue(status, out var n) ? n : status.ToString();
                return new ProjetosPorStatusDto(status.ToString(), nome, qtd, pct);
            })
            .ToList();

        var tipoProjetoNomes = new Dictionary<TipoProjeto, string>
        {
            { TipoProjeto.Residencial, "Residencial" },
            { TipoProjeto.Comercial, "Comercial" },
            { TipoProjeto.Corporativo, "Corporativo" },
            { TipoProjeto.Interiores, "Design de Interiores" }
        };

        var projetosPorTipo = Enum.GetValues<TipoProjeto>()
            .Select(tipo =>
            {
                var qtd = projetos.Count(p => p.Tipo == tipo);
                var pct = totalProjetos > 0 ? Math.Round((decimal)qtd / totalProjetos * 100m, 1) : 0m;
                var nome = tipoProjetoNomes.TryGetValue(tipo, out var n) ? n : tipo.ToString();
                return new ProjetosPorTipoDto(tipo.ToString(), nome, qtd, pct);
            })
            .ToList();

        var statusLeadNomes = new Dictionary<StatusLead, string>
        {
            { StatusLead.Novo, "Novo Lead" },
            { StatusLead.EmContato, "Em Contato" },
            { StatusLead.PropostaEnviada, "Proposta Enviada" },
            { StatusLead.Negociando, "Em Negociação" },
            { StatusLead.Convertido, "Convertido em Cliente" },
            { StatusLead.Perdido, "Oportunidade Perdida" }
        };

        var leadsPorStatus = Enum.GetValues<StatusLead>()
            .Select(status =>
            {
                var qtd = leads.Count(l => l.Status == status);
                var pct = totalLeads > 0 ? Math.Round((decimal)qtd / totalLeads * 100m, 1) : 0m;
                var nome = statusLeadNomes.TryGetValue(status, out var n) ? n : status.ToString();
                return new LeadsPorStatusDto(status.ToString(), nome, qtd, pct);
            })
            .ToList();

        var leadsPorOrigem = leads
            .GroupBy(l => l.Origem?.Descricao ?? "Direto / Indicação")
            .Select(g =>
            {
                var qtd = g.Count();
                var pct = totalLeads > 0 ? Math.Round((decimal)qtd / totalLeads * 100m, 1) : 0m;
                return new LeadsPorOrigemDto(g.Key, qtd, pct);
            })
            .OrderByDescending(o => o.Quantidade)
            .ToList();

        if (leadsPorOrigem.Count == 0)
        {
            leadsPorOrigem.Add(new LeadsPorOrigemDto("Sem registros", 0, 0m));
        }

        var hoje = DateTime.UtcNow;
        var ptBr = new CultureInfo("pt-BR");
        var ultimos6Meses = Enumerable.Range(0, 6)
            .Select(i => hoje.AddMonths(-5 + i))
            .ToList();

        var propostasMensais = ultimos6Meses.Select(dataRef =>
        {
            var mesAnoKey = dataRef.ToString("yyyy-MM");
            var rotulo = dataRef.ToString("MMM/yy", ptBr);
            rotulo = char.ToUpper(rotulo[0]) + rotulo.Substring(1);

            var propostasDoMes = propostas.Where(p => p.CriadoEm.Year == dataRef.Year && p.CriadoEm.Month == dataRef.Month).ToList();
            var qtd = propostasDoMes.Count;
            var val = propostasDoMes.Sum(p => p.ValorFinalAjustado > 0 ? p.ValorFinalAjustado : p.ValorTotalSugerido);

            return new PropostasMensalDto(mesAnoKey, rotulo, qtd, val);
        }).ToList();

        var clientesDict = clientes.ToDictionary(c => c.Id, c => c.Nome);

        var projetosRecentes = projetos
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
                    statusProjetoNomes.TryGetValue(status, out var st) ? st : status.ToString(),
                    tipoProjetoNomes.TryGetValue(tipo, out var tp) ? tp : tipo.ToString(),
                    p.MetragemTotal,
                    totalEtapas,
                    etapasConcluidas,
                    progresso,
                    p.DataInicio,
                    p.DataPrevistaEntrega
                );
            })
            .ToList();

        var leadsRecentes = leads
            .Take(5)
            .Select(l => new LeadResumoDashboardDto(
                l.Id,
                l.Nome,
                l.Email,
                l.Telefone,
                statusLeadNomes.TryGetValue(l.Status, out var st) ? st : l.Status.ToString(),
                l.Origem?.Descricao ?? "Direto",
                l.CriadoEm
            ))
            .ToList();

        var statusPropostaNomes = new Dictionary<StatusProposta, string>
        {
            { StatusProposta.Rascunho, "Rascunho" },
            { StatusProposta.Enviada, "Enviada" },
            { StatusProposta.Aprovada, "Aprovada" },
            { StatusProposta.Recusada, "Recusada" }
        };

        var propostasRecentes = propostas
            .Take(5)
            .Select(p => new PropostaResumoDashboardDto(
                p.Id,
                p.Titulo,
                p.Codigo,
                !string.IsNullOrWhiteSpace(p.ClienteNome) ? p.ClienteNome : (!string.IsNullOrWhiteSpace(p.LeadNome) ? p.LeadNome : "Sem destinatário"),
                p.MetragemQuadrada,
                p.ValorFinalAjustado > 0 ? p.ValorFinalAjustado : p.ValorTotalSugerido,
                statusPropostaNomes.TryGetValue(p.Status, out var st) ? st : p.Status.ToString(),
                p.CriadoEm
            ))
            .ToList();

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

    public async Task<PreferenciaDashboardDto?> ObterPreferenciasAsync(Guid usuarioId)
    {
        var pref = await _preferenciaRepo.ObterPorUsuarioIdAsync(usuarioId);
        if (pref == null)
            return null;

        return new PreferenciaDashboardDto(pref.UsuarioId, pref.LayoutJson, pref.AtualizadoEm);
    }

    public async Task<PreferenciaDashboardDto> SalvarPreferenciasAsync(Guid usuarioId, SalvarPreferenciaDashboardCommand command)
    {
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
}