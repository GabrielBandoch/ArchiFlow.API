using ArchiFlow.Domain.Projetos.Enum;
using System;

namespace ArchiFlow.Application.Honorarios.Strategies;

public class CorporativoCalculoStrategy : CalculoHonorarioStrategyBase
{
    public override TipoProjeto Tipo => TipoProjeto.Corporativo;
    public override string NomeTipologia => "Corporativo";
    public override decimal Multiplicador => 1.30m;
    public override string Descricao => "Corporativo (1.3)";

    public override decimal CalcularHorasEstimadas(decimal metragem, decimal fatorEscopo)
    {
        return Math.Max(30, Math.Round(metragem * 0.9m * fatorEscopo, 0));
    }
}
