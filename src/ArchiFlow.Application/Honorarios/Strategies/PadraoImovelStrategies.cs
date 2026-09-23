using ArchiFlow.Domain.Honorarios;
using System;

namespace ArchiFlow.Application.Honorarios.Strategies;

public class EconomicoPadraoStrategy : IPadraoImovelStrategy
{
    public PadraoImovel Padrao => PadraoImovel.Economico;
    public string NomePadrao => "Econômico";
    public decimal Multiplicador => 0.80m;
    public string Descricao => "Econômico (0.8)";

    public decimal CalcularFatorPadrao(decimal valorBase)
    {
        return Math.Round(valorBase * (Multiplicador - 1.0m), 2);
    }
}

public class MedioPadraoStrategy : IPadraoImovelStrategy
{
    public PadraoImovel Padrao => PadraoImovel.Medio;
    public string NomePadrao => "Médio";
    public decimal Multiplicador => 1.00m;
    public string Descricao => "Médio (1.0)";

    public decimal CalcularFatorPadrao(decimal valorBase)
    {
        return Math.Round(valorBase * (Multiplicador - 1.0m), 2);
    }
}

public class AltoPadraoStrategy : IPadraoImovelStrategy
{
    public PadraoImovel Padrao => PadraoImovel.AltoPadrao;
    public string NomePadrao => "Alto Padrão";
    public decimal Multiplicador => 1.30m;
    public string Descricao => "Alto Padrão (1.3)";

    public decimal CalcularFatorPadrao(decimal valorBase)
    {
        return Math.Round(valorBase * (Multiplicador - 1.0m), 2);
    }
}

public class LuxoPadraoStrategy : IPadraoImovelStrategy
{
    public PadraoImovel Padrao => PadraoImovel.Luxo;
    public string NomePadrao => "Luxo";
    public decimal Multiplicador => 1.60m;
    public string Descricao => "Luxo (1.6)";

    public decimal CalcularFatorPadrao(decimal valorBase)
    {
        return Math.Round(valorBase * (Multiplicador - 1.0m), 2);
    }
}
