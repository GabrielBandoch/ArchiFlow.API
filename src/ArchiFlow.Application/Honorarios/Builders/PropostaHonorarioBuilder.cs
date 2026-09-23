using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Projetos.Enum;
using System;
using System.Collections.Generic;

namespace ArchiFlow.Application.Honorarios.Builders;

public class PropostaHonorarioBuilder
{
    private readonly PropostaHonorario _proposta;

    public PropostaHonorarioBuilder()
    {
        _proposta = new PropostaHonorario
        {
            Id = Guid.NewGuid(),
            Status = StatusProposta.Rascunho,
            CriadoEm = DateTime.UtcNow,
            ItensEtapa = new List<ItemPropostaEtapa>()
        };
    }

    public static PropostaHonorarioBuilder Criar() => new();

    public PropostaHonorarioBuilder ComIdentificacao(string titulo, string codigo)
    {
        _proposta.Titulo = (titulo ?? string.Empty).Trim();
        _proposta.Codigo = (codigo ?? string.Empty).Trim();
        return this;
    }

    public PropostaHonorarioBuilder ComTipologia(TipoProjeto tipo, PadraoImovel padrao, decimal metragem)
    {
        _proposta.TipoProjeto = tipo;
        _proposta.PadraoImovel = padrao;
        _proposta.MetragemQuadrada = Math.Round(metragem, 2);
        return this;
    }

    public PropostaHonorarioBuilder ComTaxasBase(decimal valorM2Base, decimal valorHoraBase)
    {
        _proposta.ValorMetroQuadradoBase = Math.Round(valorM2Base, 2);
        _proposta.ValorHoraBase = Math.Round(valorHoraBase, 2);
        return this;
    }

    public PropostaHonorarioBuilder ComMemoriaCalculo(
        decimal valorBase,
        decimal valorFatorPadrao,
        decimal valorFatorTipologia,
        decimal valorEscopo,
        decimal valorTotalSugerido,
        decimal horasEstimadasTotal)
    {
        _proposta.ValorBase = Math.Round(valorBase, 2);
        _proposta.ValorFatorPadrao = Math.Round(valorFatorPadrao, 2);
        _proposta.ValorFatorTipologia = Math.Round(valorFatorTipologia, 2);
        _proposta.ValorEscopo = Math.Round(valorEscopo, 2);
        _proposta.ValorTotalSugerido = Math.Round(valorTotalSugerido, 2);
        _proposta.HorasEstimadasTotal = Math.Round(horasEstimadasTotal, 2);
        return this;
    }

    public PropostaHonorarioBuilder ComValorFinalAjustado(decimal valorFinalAjustado)
    {
        _proposta.ValorFinalAjustado = Math.Round(valorFinalAjustado, 2);
        return this;
    }

    public PropostaHonorarioBuilder ComCliente(Guid? clienteId, string? clienteNome)
    {
        _proposta.ClienteId = clienteId;
        _proposta.ClienteNome = clienteNome;
        return this;
    }

    public PropostaHonorarioBuilder ComLead(Guid? leadId, string? leadNome)
    {
        _proposta.LeadId = leadId;
        _proposta.LeadNome = leadNome;
        return this;
    }

    public PropostaHonorarioBuilder ComObservacoes(string? observacoes)
    {
        _proposta.Observacoes = observacoes?.Trim();
        return this;
    }

    public PropostaHonorarioBuilder ComStatus(StatusProposta status)
    {
        _proposta.Status = status;
        return this;
    }

    public PropostaHonorarioBuilder AdicionarItemEtapa(
        string nomeEtapa,
        string? descricao,
        bool incluso,
        decimal percentual,
        decimal valor,
        decimal horasEstimadas,
        int ordem)
    {
        var item = new ItemPropostaEtapa
        {
            Id = Guid.NewGuid(),
            PropostaId = _proposta.Id,
            NomeEtapa = nomeEtapa,
            Descricao = descricao,
            Incluso = incluso,
            Percentual = percentual,
            Valor = valor,
            HorasEstimadas = horasEstimadas,
            Ordem = ordem
        };

        _proposta.ItensEtapa.Add(item);
        return this;
    }

    public PropostaHonorario Build()
    {
        if (string.IsNullOrWhiteSpace(_proposta.Titulo))
        {
            throw new ArgumentException("O título da proposta é obrigatório.");
        }

        if (_proposta.MetragemQuadrada <= 0)
        {
            throw new ArgumentException("A metragem quadrada deve ser maior que zero.");
        }

        if (_proposta.ValorFinalAjustado <= 0)
        {
            throw new ArgumentException("O valor final ajustado deve ser maior que zero.");
        }

        return _proposta;
    }
}
