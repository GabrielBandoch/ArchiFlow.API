using System;

namespace ArchiFlow.Domain.Dashboard;

public class PreferenciaDashboard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public string LayoutJson { get; set; } = string.Empty;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
}
