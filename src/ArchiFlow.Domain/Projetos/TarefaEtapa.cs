namespace ArchiFlow.Domain.Projetos;

public class TarefaEtapa
{
    public Guid Id { get; set; }
    public Guid EtapaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public bool Concluida { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public virtual EtapaProjeto? Etapa { get; set; }
}
