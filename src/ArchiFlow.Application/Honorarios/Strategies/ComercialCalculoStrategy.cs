using ArchiFlow.Domain.Projetos.Enum;
using System;

namespace ArchiFlow.Application.Honorarios.Strategies;

public class ComercialCalculoStrategy : CalculoHonorarioStrategyBase
{
    public override TipoProjeto Tipo => TipoProjeto.Comercial;
    public override string NomeTipologia => "Comercial";
    public override decimal Multiplicador => 1.20m;
    public override string Descricao => "Comercial (1.2)";

    public override decimal CalcularHorasEstimadas(decimal metragem, decimal fatorEscopo)
    {
        return Math.Max(25, Math.Round(metragem * 0.85m * fatorEscopo, 0));
    }
}
