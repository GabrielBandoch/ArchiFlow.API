using System;

namespace ArchiFlow.Domain.Usuarios;

public class TokenRecuperacaoSenha
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public string TipoUsuario { get; set; } = "staff";
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime ExpiraEm { get; set; }
    public DateTime? UsadoEm { get; set; }
    public int Tentativas { get; set; } = 0;

    public bool IsValido() => UsadoEm == null && ExpiraEm > DateTime.UtcNow;
}

