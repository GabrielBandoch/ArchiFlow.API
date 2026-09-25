using ArchiFlow.Domain.Honorarios;
using System;

namespace ArchiFlow.Application.Honorarios.Strategies;

public class EconomicoPadraoStrategy : PadraoImovelStrategyBase
{
    public override PadraoImovel Padrao => PadraoImovel.Economico;
    public override string NomePadrao => "Econômico";
    public override decimal Multiplicador => 0.80m;
    public override string Descricao => "Econômico (0.8)";
}

public class MedioPadraoStrategy : PadraoImovelStrategyBase
{
    public override PadraoImovel Padrao => PadraoImovel.Medio;
    public override string NomePadrao => "Médio";
    public override decimal Multiplicador => 1.00m;
    public override string Descricao => "Médio (1.0)";
}

public class AltoPadraoStrategy : PadraoImovelStrategyBase
{
    public override PadraoImovel Padrao => PadraoImovel.AltoPadrao;
    public override string NomePadrao => "Alto Padrão";
    public override decimal Multiplicador => 1.30m;
    public override string Descricao => "Alto Padrão (1.3)";
}

public class LuxoPadraoStrategy : PadraoImovelStrategyBase
{
    public override PadraoImovel Padrao => PadraoImovel.Luxo;
    public override string NomePadrao => "Luxo";
    public override decimal Multiplicador => 1.60m;
    public override string Descricao => "Luxo (1.6)";
}
