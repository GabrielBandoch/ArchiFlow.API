using ArchiFlow.Domain.Projetos.Enum;
using System;

namespace ArchiFlow.Application.Honorarios.Strategies;

public class ResidencialCalculoStrategy : ICalculoHonorarioStrategy
{
    public TipoProjeto Tipo => TipoProjeto.Residencial;
    public string NomeTipologia => "Residencial";
    public decimal Multiplicador => 1.10m;
    public string Descricao => "Residencial (1.1)";

    public decimal CalcularFatorTipologia(decimal valorBase)
    {
        return Math.Round(valorBase * (Multiplicador - 1.0m), 2);
    }

    public decimal CalcularHorasEstimadas(decimal metragem, decimal fatorEscopo)
    {
        return Math.Max(30, Math.Round(metragem * 1.0m * fatorEscopo, 0));
    }

    public decimal AjustarValorEscopo(decimal valorSubtotal, decimal fatorEscopo)
    {
        if (fatorEscopo >= 1.0m) return Math.Round(valorSubtotal, 2);
        return Math.Round(valorSubtotal * (0.5m + (fatorEscopo * 0.5m)), 2);
    }
}
