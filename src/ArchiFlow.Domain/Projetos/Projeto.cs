using ArchiFlow.Domain.Projetos.Enum;

namespace ArchiFlow.Domain.Projetos;

public class Projeto
{
    public Guid Id { get; set; }
    public string? Nome { get; set; }
    public string? Descricao { get; set; } 
    public StatusProjetoEnum? Status { get; set; }
    public TipoProjetoEnum? Tipo { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataPrevistaEntrega { get; set; }
    public decimal? MetragemTotal { get; set; }
    public Guid? ClienteId { get; set; }
    public DateTime? CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }
    public ICollection<EtapaProjeto>? Etapas { get; set; }
}