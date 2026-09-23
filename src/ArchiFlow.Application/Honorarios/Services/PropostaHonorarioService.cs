using ArchiFlow.Application.Honorarios.Builders;
using ArchiFlow.Application.Honorarios.Commands;
using ArchiFlow.Application.Honorarios.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Honorarios.Services;

public class PropostaHonorarioService : IPropostaHonorarioService
{
    private readonly IPropostaHonorarioRepository _propostaRepo;
    private readonly IClienteRepository _clienteRepo;
    private readonly ILeadRepository _leadRepo;
    private readonly ICalculadoraHonorariosService _calculadora;
    private readonly IUnitOfWork _unitOfWork;

    public PropostaHonorarioService(
        IPropostaHonorarioRepository propostaRepo,
        IClienteRepository clienteRepo,
        ILeadRepository leadRepo,
        ICalculadoraHonorariosService calculadora,
        IUnitOfWork unitOfWork)
    {
        _propostaRepo = propostaRepo;
        _clienteRepo = clienteRepo;
        _leadRepo = leadRepo;
        _calculadora = calculadora;
        _unitOfWork = unitOfWork;
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
}
