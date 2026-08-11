using ArchiFlow.Domain.Leads.Enum;

namespace ArchiFlow.Domain.Leads;

public class Lead
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public Guid? OrigemId { get; set; }
    public OrigemLead? Origem { get; set; }
    public string? MotivoPerda { get; set; }
    public StatusLead Status { get; set; } = StatusLead.Novo;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }
    public ICollection<HistoricoContatoLead> HistoricoContatos { get; set; } = new List<HistoricoContatoLead>();
}
