using ArchiFlow.Domain.Projetos.Enum;
using System;

namespace ArchiFlow.Application.Honorarios.Strategies;

public interface ICalculoHonorarioStrategy
{
    TipoProjeto Tipo { get; }
    string NomeTipologia { get; }
    decimal Multiplicador { get; }
    string Descricao { get; }

    decimal CalcularFatorTipologia(decimal valorBase);
    decimal CalcularHorasEstimadas(decimal metragem, decimal fatorEscopo);
    decimal AjustarValorEscopo(decimal valorSubtotal, decimal fatorEscopo);
}

public abstract class CalculoHonorarioStrategyBase : ICalculoHonorarioStrategy
{
    public abstract TipoProjeto Tipo { get; }
    public abstract string NomeTipologia { get; }
    public abstract decimal Multiplicador { get; }
    public abstract string Descricao { get; }

    public virtual decimal CalcularFatorTipologia(decimal valorBase) =>
        Math.Round(valorBase * (Multiplicador - 1.0m), 2);

    public abstract decimal CalcularHorasEstimadas(decimal metragem, decimal fatorEscopo);

    public virtual decimal AjustarValorEscopo(decimal valorSubtotal, decimal fatorEscopo)
    {
        if (fatorEscopo >= 1.0m) return Math.Round(valorSubtotal, 2);
        return Math.Round(valorSubtotal * (0.5m + (fatorEscopo * 0.5m)), 2);
    }
}
