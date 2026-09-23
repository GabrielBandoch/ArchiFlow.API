using ArchiFlow.Domain.Projetos.Enum;
using System;

namespace ArchiFlow.Application.Honorarios.Strategies;

public class ResidencialCalculoStrategy : CalculoHonorarioStrategyBase
{
    public override TipoProjeto Tipo => TipoProjeto.Residencial;
    public override string NomeTipologia => "Residencial";
    public override decimal Multiplicador => 1.10m;
    public override string Descricao => "Residencial (1.1)";

    public override decimal CalcularHorasEstimadas(decimal metragem, decimal fatorEscopo)
    {
        return Math.Max(30, Math.Round(metragem * 1.0m * fatorEscopo, 0));
    }
}
