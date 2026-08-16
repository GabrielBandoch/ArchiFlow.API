using ArchiFlow.Domain.Usuarios;
using ArchiFlow.Domain.Leads;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Data;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class DbSeeder
{
    public static async Task MigrateAndSeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ArchiFlowDbContext>();
            await context.Database.MigrateAsync();

            try
            {
                await context.Database.ExecuteSqlRawAsync(@"
                    ALTER TABLE ""Clientes"" ADD COLUMN IF NOT EXISTS ""CLI_Foto_Url"" text;
                ");
            }
            catch
            {
            }

            await SeedAsync(context);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
            logger.LogError(ex, "Ocorreu um erro ao aplicar as migrações ou semear o banco.");
        }
    }

    public static async Task SeedAsync(ArchiFlowDbContext context)
    {
        await SeedUsuariosAsync(context);
        await SeedOrigensLeadAsync(context);
    }

    private static async Task SeedUsuariosAsync(ArchiFlowDbContext context)
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

    private static async Task SeedOrigensLeadAsync(ArchiFlowDbContext context)
    {
        if (await context.OrigensLead.AnyAsync())
        {
            return;
        }

        var origens = new[]
        {
            new OrigemLead { Id = Guid.NewGuid(), Descricao = "Instagram", Ativo = true, CriadoEm = DateTime.UtcNow },
            new OrigemLead { Id = Guid.NewGuid(), Descricao = "Indicação", Ativo = true, CriadoEm = DateTime.UtcNow },
            new OrigemLead { Id = Guid.NewGuid(), Descricao = "Site", Ativo = true, CriadoEm = DateTime.UtcNow },
            new OrigemLead { Id = Guid.NewGuid(), Descricao = "E-mail", Ativo = true, CriadoEm = DateTime.UtcNow },
            new OrigemLead { Id = Guid.NewGuid(), Descricao = "Outros", Ativo = true, CriadoEm = DateTime.UtcNow }
        };

        await context.OrigensLead.AddRangeAsync(origens);
        await context.SaveChangesAsync();
    }
}
