using ArchiFlow.Application.Honorarios.Strategies;
using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Projetos.Enum;
using System.Collections.Generic;
using System.Linq;

namespace ArchiFlow.Application.Honorarios.Factories;

public class HonorarioStrategyFactory : IHonorarioStrategyFactory
{
    private readonly Dictionary<TipoProjeto, ICalculoHonorarioStrategy> _tipologiaStrategies;
    private readonly Dictionary<PadraoImovel, IPadraoImovelStrategy> _padraoStrategies;
    private readonly ICalculoHonorarioStrategy _defaultTipologia;
    private readonly IPadraoImovelStrategy _defaultPadrao;

    public HonorarioStrategyFactory(
        IEnumerable<ICalculoHonorarioStrategy> tipologiaStrategies,
        IEnumerable<IPadraoImovelStrategy> padraoStrategies)
    {
        _tipologiaStrategies = tipologiaStrategies.ToDictionary(s => s.Tipo);
        _padraoStrategies = padraoStrategies.ToDictionary(s => s.Padrao);

        _defaultTipologia = _tipologiaStrategies.TryGetValue(TipoProjeto.Residencial, out var t) 
            ? t 
            : new ResidencialCalculoStrategy();

        _defaultPadrao = _padraoStrategies.TryGetValue(PadraoImovel.Medio, out var p) 
            ? p 
            : new MedioPadraoStrategy();
    }

    public ICalculoHonorarioStrategy ObterStrategy(TipoProjeto tipo)
    {
        return _tipologiaStrategies.TryGetValue(tipo, out var strategy) 
            ? strategy 
            : _defaultTipologia;
    }

    public IPadraoImovelStrategy ObterPadraoStrategy(PadraoImovel padrao)
    {
        return _padraoStrategies.TryGetValue(padrao, out var strategy) 
            ? strategy 
            : _defaultPadrao;
    }
}
