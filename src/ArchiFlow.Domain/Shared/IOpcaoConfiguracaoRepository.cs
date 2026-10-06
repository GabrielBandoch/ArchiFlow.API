namespace ArchiFlow.Domain.Shared;

public interface IOpcaoConfiguracaoRepository : IRepository<OpcaoConfiguracao>
{
    Task<IEnumerable<OpcaoConfiguracao>> ObterPorCategoriaAsync(string categoria);
    Task<IDictionary<string, IEnumerable<OpcaoConfiguracao>>> ObterTodasAgrupadasAsync();
}
