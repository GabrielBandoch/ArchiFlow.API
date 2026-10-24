using System.Threading.Tasks;

namespace ArchiFlow.Domain.Empresa;

public interface IDadosEmpresaRepository
{
    Task<DadosEmpresa?> ObterAsync();
    Task SalvarAsync(DadosEmpresa dadosEmpresa);
}

