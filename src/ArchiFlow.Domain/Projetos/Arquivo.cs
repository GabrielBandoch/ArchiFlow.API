using System;

namespace ArchiFlow.Domain.Projetos;

public class Arquivo
{
    public Guid Id { get; set; }
    public Guid ProjetoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string UrlStorage { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool VisivelCliente { get; set; } = false;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
