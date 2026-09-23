using ArchiFlow.Domain.Projetos.Enum;
using System;

namespace ArchiFlow.Application.Honorarios.Strategies;

public class CorporativoCalculoStrategy : ICalculoHonorarioStrategy
{
    public TipoProjeto Tipo => TipoProjeto.Corporativo;
    public string NomeTipologia => "Corporativo";
    public decimal Multiplicador => 1.30m;
    public string Descricao => "Corporativo (1.3)";

    public decimal CalcularFatorTipologia(decimal valorBase)
    {
        return Math.Round(valorBase * (Multiplicador - 1.0m), 2);
    }

    public decimal CalcularHorasEstimadas(decimal metragem, decimal fatorEscopo)
    {
        return Math.Max(30, Math.Round(metragem * 0.9m * fatorEscopo, 0));
    }

    public decimal AjustarValorEscopo(decimal valorSubtotal, decimal fatorEscopo)
    {
        if (fatorEscopo >= 1.0m) return Math.Round(valorSubtotal, 2);
        return Math.Round(valorSubtotal * (0.5m + (fatorEscopo * 0.5m)), 2);
    }
}
