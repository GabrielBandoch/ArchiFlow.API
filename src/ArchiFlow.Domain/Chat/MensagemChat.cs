using System;

namespace ArchiFlow.Domain.Chat;

public class MensagemChat
{
    public Guid Id { get; set; }
    public Guid ProjetoId { get; set; }
    public Guid RemetenteId { get; set; }
    public string RemetenteNome { get; set; } = string.Empty;
    public string RemetentePerfil { get; set; } = string.Empty; // "Arquiteto", "Cliente", etc.
    public string Conteudo { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public bool Lida { get; set; } = false;
}
