namespace ArchiFlow.Domain.Projetos;

public class TemplateEtapa
{
    public Guid Id { get; set; }
    public Guid TemplateProjetoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int Ordem { get; set; }
    public string? TarefasJson { get; set; }
    public virtual TemplateProjeto? TemplateProjeto { get; set; }
}
