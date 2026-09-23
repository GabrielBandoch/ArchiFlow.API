using ArchiFlow.Domain.Honorarios;

namespace ArchiFlow.Application.Honorarios.Strategies;

public interface IPadraoImovelStrategy
{
    PadraoImovel Padrao { get; }
    string NomePadrao { get; }
    decimal Multiplicador { get; }
    string Descricao { get; }

    decimal CalcularFatorPadrao(decimal valorBase);
}
