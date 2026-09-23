using ArchiFlow.Domain.Honorarios;
using System;

namespace ArchiFlow.Application.Honorarios.Strategies;

public interface IPadraoImovelStrategy
{
    PadraoImovel Padrao { get; }
    string NomePadrao { get; }
    decimal Multiplicador { get; }
    string Descricao { get; }

    decimal CalcularFatorPadrao(decimal valorBase);
}

public abstract class PadraoImovelStrategyBase : IPadraoImovelStrategy
{
    public abstract PadraoImovel Padrao { get; }
    public abstract string NomePadrao { get; }
    public abstract decimal Multiplicador { get; }
    public abstract string Descricao { get; }

    public virtual decimal CalcularFatorPadrao(decimal valorBase) =>
        Math.Round(valorBase * (Multiplicador - 1.0m), 2);
}
