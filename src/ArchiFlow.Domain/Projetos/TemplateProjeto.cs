namespace ArchiFlow.Domain.Projetos;

public class TemplateProjeto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Icone { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public virtual ICollection<TemplateEtapa> Etapas { get; set; } = new List<TemplateEtapa>();
}
