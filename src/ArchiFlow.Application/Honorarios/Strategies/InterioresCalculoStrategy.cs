using ArchiFlow.Domain.Projetos.Enum;
using System;

namespace ArchiFlow.Application.Honorarios.Strategies;

public class InterioresCalculoStrategy : ICalculoHonorarioStrategy
{
    public TipoProjeto Tipo => TipoProjeto.Interiores;
    public string NomeTipologia => "Interiores";
    public decimal Multiplicador => 1.15m;
    public string Descricao => "Interiores (1.15)";

    public decimal CalcularFatorTipologia(decimal valorBase)
    {
        return Math.Round(valorBase * (Multiplicador - 1.0m), 2);
    }

    public decimal CalcularHorasEstimadas(decimal metragem, decimal fatorEscopo)
    {
        return Math.Max(25, Math.Round(metragem * 0.88m * fatorEscopo, 0));
    }

    public decimal AjustarValorEscopo(decimal valorSubtotal, decimal fatorEscopo)
    {
        if (fatorEscopo >= 1.0m) return Math.Round(valorSubtotal, 2);
        return Math.Round(valorSubtotal * (0.5m + (fatorEscopo * 0.5m)), 2);
    }
}
