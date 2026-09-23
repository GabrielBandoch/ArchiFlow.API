using ArchiFlow.Application.Honorarios.Strategies;
using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Projetos.Enum;

namespace ArchiFlow.Application.Honorarios.Factories;

public interface IHonorarioStrategyFactory
{
    ICalculoHonorarioStrategy ObterStrategy(TipoProjeto tipo);
    IPadraoImovelStrategy ObterPadraoStrategy(PadraoImovel padrao);
}
