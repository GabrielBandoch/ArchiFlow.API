using System;

namespace ArchiFlow.Domain.Usuarios;

public class Usuario
{
    public Guid Id { get; set; }
    public Guid? EscritorioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public string? Telefone { get; set; }
    public string? FotoUrl { get; set; }
    public string? ModulosPermitidos { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    private static readonly char[] ModuleSeparators = [',', ';'];

    public bool IsAdmin()
    {
        return string.Equals(Role, Roles.AdministradorGeral, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(Role, Roles.Administrador, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(Role, Roles.ArquitetoAdmin, StringComparison.OrdinalIgnoreCase);
    }

    public bool TemPermissaoModulo(string modulo)
    {
        if (IsAdmin() || IsContaSuporte())
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(ModulosPermitidos))
        {
            return false;
        }

        var modulos = ModulosPermitidos.Split(ModuleSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return modulos.Contains(modulo, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsContaSuporte()
    {
        if (string.Equals(Role, Roles.AdministradorGeral, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Role, "Suporte", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var envEmail = Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL");
        if (!string.IsNullOrWhiteSpace(envEmail) && string.Equals(Email, envEmail.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(Email, "suporte.cotu@cotu.com.br", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(Email, "admin@cotu.com", StringComparison.OrdinalIgnoreCase);
    }
}

