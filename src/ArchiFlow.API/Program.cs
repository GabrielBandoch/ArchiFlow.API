using ArchiFlow.API;
using ArchiFlow.API.Middleware;
using ArchiFlow.API.Extensions;
using ArchiFlow.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

await EnvLoader.LoadAsync();

var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPass = Environment.GetEnvironmentVariable("DB_PASSWORD");

if (string.IsNullOrEmpty(dbHost) || string.IsNullOrEmpty(dbPort) || string.IsNullOrEmpty(dbName) || string.IsNullOrEmpty(dbUser) || string.IsNullOrEmpty(dbPass))
{
    throw new InvalidOperationException("Configuração do Banco de Dados (.env) incompleta.");
}

var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPass}";

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
var jwtExpirationStr = Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES");

if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience) || string.IsNullOrEmpty(jwtExpirationStr))
{
    throw new InvalidOperationException("Configuração do JWT (.env) incompleta.");
}

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.ConfigureDatabase(connectionString);
builder.Services.ConfigureDependencyInjection(builder.Environment);
builder.Services.ConfigureSecurity(jwtSecret, jwtIssuer, jwtAudience);
builder.Services.ConfigureSwagger();

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

var allowedOrigins = builder.Configuration.GetValue<string>("AllowedOrigins")?.Split(',') ?? Array.Empty<string>();

builder.Services.AddCors(options =>
    options.AddPolicy("ArchiFlowPolicy", p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyMethod()
         .AllowAnyHeader()
         .AllowCredentials()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("ArchiFlowPolicy");
app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await DbSeeder.MigrateAndSeedAsync(app.Services, app.Environment.IsDevelopment());

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.ToString()
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

await app.RunAsync();

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class Program { }
