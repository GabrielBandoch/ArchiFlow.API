using ArchiFlow.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Data;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class DbSeeder
{
    public static async Task SeedAsync(ArchiFlowDbContext context)
    {
        if (await context.Usuarios.AnyAsync())
        {
            return;
        }

        var admin = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Administrador do Sistema",
            Email = "admin@archiflow.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12),
            Role = Roles.Administrador,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var gerente = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Gerente Arquiteto",
            Email = "gerente@archiflow.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("Gerente@123", workFactor: 12),
            Role = Roles.Gerente,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var colaborador = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Colaborador Arquiteto",
            Email = "colaborador@archiflow.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("Colaborador@123", workFactor: 12),
            Role = Roles.Colaborador,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        await context.Usuarios.AddRangeAsync(admin, gerente, colaborador);
        await context.SaveChangesAsync();
    }
}
