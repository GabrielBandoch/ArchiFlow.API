using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ArchiFlow.Infrastructure.MultiTenancy;

public class TenantDatabaseProvisioner : ITenantDatabaseProvisioner
{
    private readonly string _baseConnectionString;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TenantDatabaseProvisioner> _logger;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private static readonly ConcurrentDictionary<string, bool> _initializedTenants = new(StringComparer.OrdinalIgnoreCase);

    public TenantDatabaseProvisioner(
        string baseConnectionString,
        IServiceProvider serviceProvider,
        ILogger<TenantDatabaseProvisioner> logger)
    {
        _baseConnectionString = baseConnectionString ?? string.Empty;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task EnsureTenantDatabaseAsync(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return;
        }

        var cleanTenant = SanitizeTenant(tenantId);


        if (_initializedTenants.ContainsKey(cleanTenant))
        {
            return;
        }

        var semaphore = _locks.GetOrAdd(cleanTenant, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();

        try
        {
            if (_initializedTenants.ContainsKey(cleanTenant))
            {
                return;
            }

            var targetDbName = ResolveDatabaseName(cleanTenant);
            _logger.LogInformation("Verificando e provisionando banco de dados para o tenant '{Tenant}' (DB: '{DbName}')...", cleanTenant, targetDbName);

            await EnsureDatabaseExistsOnServerAsync(targetDbName);
            await RunMigrationsAndSeedAsync(targetDbName);

            _initializedTenants.TryAdd(cleanTenant, true);
            _logger.LogInformation("Banco de dados '{DbName}' para o tenant '{Tenant}' provisionado e pronto com sucesso.", targetDbName, cleanTenant);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Falha ao provisionar o banco de dados do tenant '{cleanTenant}'", ex);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static void ValidarNomeBanco(string dbName)
    {
        if (string.IsNullOrWhiteSpace(dbName) || !System.Text.RegularExpressions.Regex.IsMatch(dbName, @"^[a-z0-9_]+$", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromMilliseconds(250)))
        {
            throw new ArgumentException($"Nome de banco de dados inválido: '{dbName}'", nameof(dbName));
        }
    }

    private static string ResolveDatabaseName(string tenant)
    {
        var envDb = Environment.GetEnvironmentVariable("DB_NAME");
        var baseDb = !string.IsNullOrWhiteSpace(envDb) ? envDb.Trim().ToLowerInvariant() : "cotu";

        if (tenant.Equals("default", StringComparison.OrdinalIgnoreCase))
        {
            return baseDb;
        }

        return $"{baseDb}_{tenant.ToLowerInvariant().Replace("-", "_")}";
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("csharpsquid", "S2077", Justification = "PostgreSQL CREATE DATABASE does not support parameters; targetDbName is validated against strict whitelist.")]
    private async Task EnsureDatabaseExistsOnServerAsync(string targetDbName)
    {
        if (string.IsNullOrWhiteSpace(_baseConnectionString))
        {
            return;
        }


        var masterBuilder = new NpgsqlConnectionStringBuilder(_baseConnectionString)
        {
            Database = "postgres"
        };

        try
        {
            await using var conn = new NpgsqlConnection(masterBuilder.ConnectionString);
            await conn.OpenAsync();

            await using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @dbName;";
            checkCmd.Parameters.AddWithValue("dbName", targetDbName);

            var exists = await checkCmd.ExecuteScalarAsync();
            if (exists == null)
            {
                _logger.LogInformation("Banco de dados '{DbName}' não encontrado no PostgreSQL. Criando novo banco...", targetDbName);

                ValidarNomeBanco(targetDbName);
                await using var createCmd = conn.CreateCommand();
#pragma warning disable S2077 
                createCmd.CommandText = $"CREATE DATABASE \"{targetDbName}\" ENCODING 'UTF8';";
#pragma warning restore S2077
                await createCmd.ExecuteNonQueryAsync();

                _logger.LogInformation("Banco de dados '{DbName}' criado com sucesso no PostgreSQL.", targetDbName);
            }
        }
        catch (NpgsqlException ex) when (ex.Message.Contains("postgres"))
        {

            _logger.LogWarning(ex, "Não foi possível conectar ao banco de manutenção 'postgres', tentando conexão no banco base.");
            await EnsureDatabaseExistsFallbackAsync(targetDbName);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("csharpsquid", "S2077", Justification = "PostgreSQL CREATE DATABASE does not support parameters; targetDbName is validated against strict whitelist.")]
    private async Task EnsureDatabaseExistsFallbackAsync(string targetDbName)
    {
        await using var conn = new NpgsqlConnection(_baseConnectionString);
        await conn.OpenAsync();

        await using var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @dbName;";
        checkCmd.Parameters.AddWithValue("dbName", targetDbName);

        var exists = await checkCmd.ExecuteScalarAsync();
        if (exists == null)
        {
            ValidarNomeBanco(targetDbName);
            await using var createCmd = conn.CreateCommand();
#pragma warning disable S2077 
            createCmd.CommandText = $"CREATE DATABASE \"{targetDbName}\" ENCODING 'UTF8';";
#pragma warning restore S2077
            await createCmd.ExecuteNonQueryAsync();
        }
    }

    private async Task RunMigrationsAndSeedAsync(string targetDbName)
    {
        var tenantConnStr = new NpgsqlConnectionStringBuilder(_baseConnectionString)
        {
            Database = targetDbName
        }.ConnectionString;

        var optionsBuilder = new DbContextOptionsBuilder<ArchiFlowDbContext>();
        optionsBuilder.UseNpgsql(tenantConnStr);

        using var scope = _serviceProvider.CreateScope();
        var env = scope.ServiceProvider.GetService<IHostEnvironment>();
        var isDevelopment = env?.IsDevelopment() ?? false;

        await using var context = new ArchiFlowDbContext(optionsBuilder.Options);
        await DbSeeder.MigrateAndSeedContextAsync(context, scope.ServiceProvider, isDevelopment);
    }

    private static string SanitizeTenant(string input)
    {
        var sanitized = Regex.Replace(input, @"[^a-zA-Z0-9_-]", "", RegexOptions.None, TimeSpan.FromSeconds(1));
        return string.IsNullOrWhiteSpace(sanitized) ? "default" : sanitized.ToLowerInvariant();
    }
}

