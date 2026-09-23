using ArchiFlow.Domain.Projetos.Enum;

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
