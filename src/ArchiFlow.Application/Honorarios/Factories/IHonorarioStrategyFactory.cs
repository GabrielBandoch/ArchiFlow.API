using ArchiFlow.Application.Honorarios.Strategies;
using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Projetos.Enum;

namespace ArchiFlow.Application.Honorarios.Factories;

/// <summary>
/// Factory Pattern (GoF): Define a fábrica responsável por resolver a estratégia
/// correta de cálculo para tipologias de projeto e padrões de imóvel.
/// </summary>
public interface IHonorarioStrategyFactory
{
    ICalculoHonorarioStrategy ObterStrategy(TipoProjeto tipo);
    IPadraoImovelStrategy ObterPadraoStrategy(PadraoImovel padrao);
}
