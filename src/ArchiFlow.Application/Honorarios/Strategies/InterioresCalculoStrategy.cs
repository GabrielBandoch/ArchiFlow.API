using ArchiFlow.Domain.Projetos.Enum;
using System;

namespace ArchiFlow.Application.Honorarios.Strategies;

public class InterioresCalculoStrategy : CalculoHonorarioStrategyBase
{
    public override TipoProjeto Tipo => TipoProjeto.Interiores;
    public override string NomeTipologia => "Interiores";
    public override decimal Multiplicador => 1.15m;
    public override string Descricao => "Interiores (1.15)";

    public override decimal CalcularHorasEstimadas(decimal metragem, decimal fatorEscopo)
    {
        return Math.Max(25, Math.Round(metragem * 0.88m * fatorEscopo, 0));
    }
}
