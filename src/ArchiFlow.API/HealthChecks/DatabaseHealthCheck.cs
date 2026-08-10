using Microsoft.Extensions.Diagnostics.HealthChecks;
using ArchiFlow.Infrastructure.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiFlow.API.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ArchiFlowDbContext _context;

    public DatabaseHealthCheck(ArchiFlowDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            if (canConnect)
            {
                return HealthCheckResult.Healthy("Conexão com o banco de dados PostgreSQL estabelecida com sucesso.");
            }

            return HealthCheckResult.Unhealthy("Não foi possível conectar ao banco de dados PostgreSQL.");
        }
        catch (System.Exception ex)
        {
            return HealthCheckResult.Unhealthy("Falha ao verificar a saúde do banco de dados.", ex);
        }
    }
}
