namespace ArchiFlow.Domain.Leads;

public class HistoricoContatoLead
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public Lead? Lead { get; set; }
    public DateTime DataContato { get; set; } = DateTime.UtcNow;
    public string Canal { get; set; } = string.Empty;
    public string Resumo { get; set; } = string.Empty;
}
