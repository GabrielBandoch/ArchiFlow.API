using ArchiFlow.Application.Honorarios.Builders;
using ArchiFlow.Application.Honorarios.Commands;
using ArchiFlow.Application.Honorarios.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Domain.Shared;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Honorarios.Services;

public class PropostaHonorarioService : IPropostaHonorarioService
{
    private readonly IPropostaHonorarioRepository _propostaRepo;
    private readonly IClienteRepository _clienteRepo;
    private readonly ILeadRepository _leadRepo;
    private readonly ICalculadoraHonorariosService _calculadora;
    private readonly IConfiguracaoPropostaRepository _configPropostaRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public PropostaHonorarioService(
        IPropostaHonorarioRepository propostaRepo,
        IClienteRepository clienteRepo,
        ILeadRepository leadRepo,
        ICalculadoraHonorariosService calculadora,
        IConfiguracaoPropostaRepository configPropostaRepo,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _propostaRepo = propostaRepo;
        _clienteRepo = clienteRepo;
        _leadRepo = leadRepo;
        _calculadora = calculadora;
        _configPropostaRepo = configPropostaRepo;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<PropostaHonorarioDto>> GetAll()
    {
        var propostas = await _propostaRepo.GetAllWithItens();
        return propostas.Select(MapearParaDto);
    }

    public async Task<PropostaHonorarioDto?> GetById(Guid id)
    {
        var proposta = await _propostaRepo.GetByIdWithItens(id);
        return proposta == null ? null : MapearParaDto(proposta);
    }

    public async Task<IEnumerable<PropostaHonorarioDto>> GetByClienteId(Guid clienteId)
    {
        var propostas = await _propostaRepo.GetByClienteId(clienteId);
        return propostas.Select(MapearParaDto);
    }

    public async Task<IEnumerable<PropostaHonorarioDto>> GetByLeadId(Guid leadId)
    {
        var propostas = await _propostaRepo.GetByLeadId(leadId);
        return propostas.Select(MapearParaDto);
    }

    public async Task<PropostaHonorarioDto> Criar(CriarPropostaCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Titulo))
        {
            throw new ArgumentException("O título da proposta é obrigatório.");
        }

        if (command.MetragemQuadrada <= 0)
        {
            throw new ArgumentException("A metragem quadrada deve ser maior que zero.");
        }

        string? clienteNome = command.ClienteNome;
        if (command.ClienteId.HasValue)
        {
            var cliente = await _clienteRepo.GetById(command.ClienteId.Value);
            if (cliente != null)
            {
                clienteNome = cliente.Nome;
            }
        }

        string? leadNome = command.LeadNome;
        if (command.LeadId.HasValue)
        {
            var lead = await _leadRepo.GetById(command.LeadId.Value);
            if (lead != null)
            {
                leadNome = lead.Nome;
            }
        }

        var simulacao = _calculadora.Calcular(new SimulacaoParametrosDto(
            command.MetragemQuadrada,
            command.TipoProjeto,
            command.PadraoImovel,
            command.EtapasInclusas,
            command.ValorHoraBase,
            command.ValorMetroQuadradoBase
        ));

        var codigo = await _propostaRepo.GerarProximoCodigo();

        var valorFinal = command.ValorFinalAjustado.HasValue && command.ValorFinalAjustado.Value > 0
            ? command.ValorFinalAjustado.Value
            : simulacao.ValorTotalSugerido;

        var builder = PropostaHonorarioBuilder.Criar()
            .ComIdentificacao(command.Titulo, codigo)
            .ComTipologia(command.TipoProjeto, command.PadraoImovel, simulacao.MetragemQuadrada)
            .ComTaxasBase(simulacao.MemoriaCalculo.ValorMetroQuadradoBase, simulacao.MemoriaCalculo.ValorHoraEstimado)
            .ComMemoriaCalculo(
                simulacao.MemoriaCalculo.ValorBase,
                simulacao.MemoriaCalculo.ValorFatorPadrao,
                simulacao.MemoriaCalculo.ValorFatorTipologia,
                simulacao.MemoriaCalculo.ValorEscopo,
                simulacao.ValorTotalSugerido,
                simulacao.HorasEstimadasTotal)
            .ComValorFinalAjustado(valorFinal)
            .ComCliente(command.ClienteId, clienteNome)
            .ComLead(command.LeadId, leadNome)
            .ComObservacoes(command.Observacoes)
            .ComStatus(StatusProposta.Rascunho);

        foreach (var etapa in simulacao.Etapas)
        {
            builder.AdicionarItemEtapa(
                etapa.Nome,
                etapa.Descricao,
                etapa.Incluso,
                etapa.Percentual,
                etapa.Valor,
                etapa.HorasEstimadas,
                etapa.Ordem
            );
        }

        var proposta = builder.Build();

        await _propostaRepo.Create(proposta);
        await _unitOfWork.Commit();

        return MapearParaDto(proposta);
    }

    public async Task<PropostaHonorarioDto> AtualizarStatus(Guid id, AtualizarStatusPropostaCommand command)
    {
        var proposta = await _propostaRepo.GetByIdWithItens(id)
            ?? throw new KeyNotFoundException($"Proposta {id} não encontrada.");

        proposta.Status = command.Status;
        proposta.AtualizadoEm = DateTime.UtcNow;

        await _propostaRepo.Update(proposta);
        await _unitOfWork.Commit();

        return MapearParaDto(proposta);
    }

    public async Task<PropostaHonorarioDto> AjustarValor(Guid id, AjustarValorPropostaCommand command)
    {
        if (command.ValorFinalAjustado <= 0)
        {
            throw new ArgumentException("O valor ajustado deve ser maior que zero.");
        }

        var proposta = await _propostaRepo.GetByIdWithItens(id)
            ?? throw new KeyNotFoundException($"Proposta {id} não encontrada.");

        proposta.ValorFinalAjustado = Math.Round(command.ValorFinalAjustado, 2);
        if (!string.IsNullOrWhiteSpace(command.Observacoes))
        {
            proposta.Observacoes = command.Observacoes;
        }
        proposta.AtualizadoEm = DateTime.UtcNow;

        await _propostaRepo.Update(proposta);
        await _unitOfWork.Commit();

        return MapearParaDto(proposta);
    }

    public async Task<bool> Excluir(Guid id)
    {
        var existe = await _propostaRepo.Exists(id);
        if (!existe) return false;

        await _propostaRepo.Delete(id);
        await _unitOfWork.Commit();
        return true;
    }

    private static PropostaHonorarioDto MapearParaDto(PropostaHonorario p)
    {
        var itensDto = p.ItensEtapa
            .OrderBy(i => i.Ordem)
            .Select(i => new ItemPropostaEtapaDto(
                i.Id,
                i.PropostaId,
                i.NomeEtapa,
                i.Descricao,
                i.Incluso,
                i.Percentual,
                i.Valor,
                i.HorasEstimadas,
                i.Ordem
            )).ToList();

        return new PropostaHonorarioDto(
            p.Id,
            p.Titulo,
            p.Codigo,
            p.ClienteId,
            p.ClienteNome,
            p.LeadId,
            p.LeadNome,
            p.TipoProjeto,
            ObterNomeTipoProjeto(p.TipoProjeto),
            p.PadraoImovel,
            ObterNomePadraoImovel(p.PadraoImovel),
            p.MetragemQuadrada,
            p.ValorHoraBase,
            p.ValorMetroQuadradoBase,
            p.HorasEstimadasTotal,
            p.ValorBase,
            p.ValorFatorPadrao,
            p.ValorFatorTipologia,
            p.ValorEscopo,
            p.ValorTotalSugerido,
            p.ValorFinalAjustado,
            p.Status,
            ObterNomeStatus(p.Status),
            p.Observacoes,
            p.CriadoEm,
            p.AtualizadoEm,
            itensDto
        );
    }

    private static string ObterNomeTipoProjeto(TipoProjeto tipo) => tipo switch
    {
        TipoProjeto.Residencial => "Residencial",
        TipoProjeto.Comercial => "Comercial",
        TipoProjeto.Corporativo => "Corporativo",
        TipoProjeto.Interiores => "Interiores",
        _ => "Residencial"
    };

    private static string ObterNomePadraoImovel(PadraoImovel padrao) => padrao switch
    {
        PadraoImovel.Economico => "Econômico",
        PadraoImovel.Medio => "Médio",
        PadraoImovel.AltoPadrao => "Alto Padrão",
        PadraoImovel.Luxo => "Luxo",
        _ => "Médio"
    };

    private static string ObterNomeStatus(StatusProposta status) => status switch
    {
        StatusProposta.Rascunho => "Rascunho",
        StatusProposta.Enviada => "Enviada",
        StatusProposta.Aprovada => "Aprovada",
        StatusProposta.Recusada => "Recusada",
        _ => "Rascunho"
    };

    public async Task<ConfiguracaoPropostaDto> ObterConfiguracaoAsync()
    {
        var usuarioId = ObterUsuarioIdContexto();
        var config = await _configPropostaRepo.ObterPorUsuarioIdAsync(usuarioId);

        if (config == null)
        {
            return new ConfiguracaoPropostaDto(
                null,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                "#765538",
                true,
                true,
                true,
                false,
                true,
                true,
                true,
                "Apresentamos nossa proposta técnica e comercial para o desenvolvimento do projeto arquitetônico, estruturada com rigor metodológico, atendimento personalizado e foco em excelência e viabilidade construtiva.",
                15,
                "Entrada no aceite da proposta + saldo parcelado conforme entrega das etapas contratadas.",
                null,
                null,
                "1. O escopo compreende estritamente as etapas e serviços descritos nesta proposta.\n2. Estão inclusas até 2 (duas) rodadas de revisões conceituais na fase de Estudo Preliminar.\n3. Projetos complementares e taxas de aprovação em órgãos públicos são de responsabilidade do contratante ou contratados à parte.\n4. Os prazos de execução passam a contar a partir da assinatura do contrato e fornecimento das informações necessárias.",
                "Olá {cliente}! Segue a proposta comercial para o projeto *{projeto}* ({metragem} m²) elaborada por {escritorio}.\n\n💰 *Valor Total:* {valor}\n📅 *Validade:* {validade} dias\n\nFicamos à disposição para esclarecer qualquer dúvida!",
                false,
                null
            );
        }

        return MapearConfiguracaoParaDto(config);
    }

    public async Task<ConfiguracaoPropostaDto> SalvarConfiguracaoAsync(SalvarConfiguracaoPropostaCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.NomeEscritorio))
        {
            throw new ArgumentException("O nome do escritório / razão social é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ArgumentException("O e-mail de contato do escritório é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(command.Telefone))
        {
            throw new ArgumentException("O telefone / WhatsApp do escritório é obrigatório.");
        }

        var usuarioId = ObterUsuarioIdContexto();
        var config = await _configPropostaRepo.ObterPorUsuarioIdAsync(usuarioId);

        if (config == null)
        {
            config = new ConfiguracaoProposta
            {
                UsuarioId = usuarioId,
                NomeEscritorio = command.NomeEscritorio.Trim(),
                Slogan = command.Slogan?.Trim() ?? string.Empty,
                RegistroProfissional = command.RegistroProfissional?.Trim() ?? string.Empty,
                Email = command.Email.Trim(),
                Telefone = command.Telefone.Trim(),
                Endereco = command.Endereco?.Trim() ?? string.Empty,
                LogoUrl = command.LogoUrl,
                CorPrimaria = string.IsNullOrWhiteSpace(command.CorPrimaria) ? "#765538" : command.CorPrimaria.Trim(),
                ExibirCabecalho = command.ExibirCabecalho,
                ExibirResumo = command.ExibirResumo,
                ExibirTabelaEtapas = command.ExibirTabelaEtapas,
                ExibirMemoriaCalculo = command.ExibirMemoriaCalculo,
                ExibirCondicoesPagamento = command.ExibirCondicoesPagamento,
                ExibirTermosGerais = command.ExibirTermosGerais,
                ExibirAssinaturas = command.ExibirAssinaturas,
                TextoApresentacao = command.TextoApresentacao?.Trim() ?? string.Empty,
                ValidadeDias = command.ValidadeDias > 0 ? command.ValidadeDias : 15,
                CondicoesPagamentoPadrao = command.CondicoesPagamentoPadrao?.Trim() ?? string.Empty,
                ChavePix = command.ChavePix?.Trim(),
                DadosBancarios = command.DadosBancarios?.Trim(),
                TermosGerais = command.TermosGerais?.Trim() ?? string.Empty,
                TemplateMensagemWhatsapp = command.TemplateMensagemWhatsapp?.Trim() ?? string.Empty,
                Configurado = true,
                AtualizadoEm = DateTime.UtcNow
            };

            await _configPropostaRepo.Create(config);
        }
        else
        {
            config.NomeEscritorio = command.NomeEscritorio.Trim();
            config.Slogan = command.Slogan?.Trim() ?? string.Empty;
            config.RegistroProfissional = command.RegistroProfissional?.Trim() ?? string.Empty;
            config.Email = command.Email.Trim();
            config.Telefone = command.Telefone.Trim();
            config.Endereco = command.Endereco?.Trim() ?? string.Empty;
            config.LogoUrl = command.LogoUrl;
            config.CorPrimaria = string.IsNullOrWhiteSpace(command.CorPrimaria) ? "#765538" : command.CorPrimaria.Trim();
            config.ExibirCabecalho = command.ExibirCabecalho;
            config.ExibirResumo = command.ExibirResumo;
            config.ExibirTabelaEtapas = command.ExibirTabelaEtapas;
            config.ExibirMemoriaCalculo = command.ExibirMemoriaCalculo;
            config.ExibirCondicoesPagamento = command.ExibirCondicoesPagamento;
            config.ExibirTermosGerais = command.ExibirTermosGerais;
            config.ExibirAssinaturas = command.ExibirAssinaturas;
            config.TextoApresentacao = command.TextoApresentacao?.Trim() ?? string.Empty;
            config.ValidadeDias = command.ValidadeDias > 0 ? command.ValidadeDias : 15;
            config.CondicoesPagamentoPadrao = command.CondicoesPagamentoPadrao?.Trim() ?? string.Empty;
            config.ChavePix = command.ChavePix?.Trim();
            config.DadosBancarios = command.DadosBancarios?.Trim();
            config.TermosGerais = command.TermosGerais?.Trim() ?? string.Empty;
            config.TemplateMensagemWhatsapp = command.TemplateMensagemWhatsapp?.Trim() ?? string.Empty;
            config.Configurado = true;
            config.AtualizadoEm = DateTime.UtcNow;

            await _configPropostaRepo.Update(config);
        }

        await _unitOfWork.Commit();
        return MapearConfiguracaoParaDto(config);
    }

    private static ConfiguracaoPropostaDto MapearConfiguracaoParaDto(ConfiguracaoProposta c)
    {
        return new ConfiguracaoPropostaDto(
            c.Id,
            c.NomeEscritorio,
            c.Slogan,
            c.RegistroProfissional,
            c.Email,
            c.Telefone,
            c.Endereco,
            c.LogoUrl,
            c.CorPrimaria,
            c.ExibirCabecalho,
            c.ExibirResumo,
            c.ExibirTabelaEtapas,
            c.ExibirMemoriaCalculo,
            c.ExibirCondicoesPagamento,
            c.ExibirTermosGerais,
            c.ExibirAssinaturas,
            c.TextoApresentacao,
            c.ValidadeDias,
            c.CondicoesPagamentoPadrao,
            c.ChavePix,
            c.DadosBancarios,
            c.TermosGerais,
            c.TemplateMensagemWhatsapp,
            c.Configurado,
            c.AtualizadoEm
        );
    }

    private Guid ObterUsuarioIdContexto()
    {
        var user = _httpContextAccessor?.HttpContext?.User;
        var claim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? user?.FindFirst("nameid")?.Value
                 ?? user?.FindFirst("sub")?.Value;

        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
