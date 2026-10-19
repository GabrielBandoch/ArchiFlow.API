using System;

namespace ArchiFlow.Domain.Clientes;

public class RepresentanteCliente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClienteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Cargo { get; set; }
    public string? SenhaHash { get; set; }
    public bool Ativo { get; set; } = true;
    public string? ConviteTokenHash { get; set; }
    public DateTime? ConviteExpiraEm { get; set; }
    public DateTime? ConviteUsadoEm { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    public Cliente? Cliente { get; set; }

    public bool IsConvitePendente() => ConviteUsadoEm == null && ConviteExpiraEm.HasValue && ConviteExpiraEm.Value > DateTime.UtcNow;
}
