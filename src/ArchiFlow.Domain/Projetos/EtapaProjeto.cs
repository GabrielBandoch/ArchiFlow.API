using ArchiFlow.Domain.Projetos.Enum;

namespace ArchiFlow.Domain.Projetos;

public class EtapaProjeto
{
    public Guid Id { get; set; }
    public Guid? ProjetoId { get; set; }
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public StatusEtapa? Status { get; set; }
    public int? Ordem { get; set; }
    public DateTime? DataConclusao { get; set; }
    public virtual Projeto? Projeto { get; set; }
}