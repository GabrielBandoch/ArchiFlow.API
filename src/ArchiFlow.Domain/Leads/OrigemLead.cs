using System;

namespace ArchiFlow.Domain.Leads;

public class OrigemLead
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
