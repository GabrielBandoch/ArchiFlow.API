using ArchiFlow.API.HealthChecks;
using ArchiFlow.API.Security;
using ArchiFlow.Application.Mappings;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.Projetos.Facades;
using ArchiFlow.Application.Projetos.Services;
using ArchiFlow.Application.Usuarios.Services;
using ArchiFlow.Application.Leads.Services;
using ArchiFlow.Application.Leads.Facades;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Usuarios;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Infrastructure.Data;
using ArchiFlow.Infrastructure.Repositories;
using ArchiFlow.Infrastructure.Repositories.Projetos;
using ArchiFlow.Infrastructure.Repositories.Usuarios;
using ArchiFlow.Infrastructure.Repositories.Clientes;
using ArchiFlow.Infrastructure.Repositories.Leads;
using ArchiFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace ArchiFlow.API.Extensions;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ArchiFlowDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("Database");

        return services;
    }

    public static IServiceCollection ConfigureDependencyInjection(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ArchiFlowMappingProfile));

        // Repositories
        services.AddScoped<IProjetoRepository, ProjetoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IOrigemLeadRepository, OrigemLeadRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services & Facades
        services.AddScoped<IProjetoService, ProjetoService>();
        services.AddScoped<IProjetoFacade, ProjetoFacade>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<ILeadFacade, LeadFacade>();
        services.AddScoped<IOrigemLeadService, OrigemLeadService>();
        services.AddScoped<IOrigemLeadFacade, OrigemLeadFacade>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();

        // Security / Authorization
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthorizationHandler, ProjetoOwnerHandler>();

        return services;
    }

    public static IServiceCollection ConfigureSecurity(
        this IServiceCollection services, 
        string jwtSecret, 
        string jwtIssuer, 
        string jwtAudience)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy("ApenasAdmin", policy => policy.RequireRole("Administrador"))
            .AddPolicy("ApenasGerenteOuAdmin", policy => policy.RequireRole("Administrador", "Gerente"))
            .AddPolicy("AcessoArquiteto", policy => policy.RequireRole("Administrador", "Gerente", "Colaborador"))
            .AddPolicy("ProjetoOwner", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new ProjetoOwnerRequirement());
            });

        return services;
    }

    public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "ArchiFlow API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Autenticação baseada em JWT. Insira: Bearer {seu_token}",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
