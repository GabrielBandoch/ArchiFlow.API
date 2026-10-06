using ArchiFlow.API.HealthChecks;
using ArchiFlow.API.Security;
using ArchiFlow.Application.Honorarios.Builders;
using ArchiFlow.Application.Honorarios.Factories;
using ArchiFlow.Application.Honorarios.Strategies;
using ArchiFlow.Application.Mappings;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.Projetos.Facades;
using ArchiFlow.Application.Projetos.Services;
using ArchiFlow.Application.Usuarios.Services;
using ArchiFlow.Application.Leads.Services;
using ArchiFlow.Application.Leads.Facades;
using ArchiFlow.Application.Clientes.Services;
using ArchiFlow.Application.Clientes.Facades;
using ArchiFlow.Application.Dashboard.Services;
using ArchiFlow.Application.Dashboard.Facades;
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
using ArchiFlow.Infrastructure.MultiTenancy;
using ArchiFlow.Infrastructure.Repositories.Leads;
using ArchiFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace ArchiFlow.API.Extensions;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ArchiFlowDbContext>((sp, options) =>
        {
            var tenantContext = sp.GetService<ITenantContext>();
            var effectiveConnection = tenantContext != null && tenantContext.IsResolved
                ? tenantContext.BuildConnectionString(connectionString)
                : connectionString;

            options.UseNpgsql(effectiveConnection);
        });

        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("Database");

        return services;
    }

    public static IServiceCollection ConfigureDependencyInjection(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddAutoMapper(typeof(ArchiFlowMappingProfile));

        // Multi-Tenancy
        services.AddScoped<ITenantContext, TenantContext>();

        // Repositories
        services.AddScoped<IProjetoRepository, ProjetoRepository>();
        services.AddScoped<ITemplateProjetoRepository, TemplateProjetoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IOrigemLeadRepository, OrigemLeadRepository>();
        services.AddScoped<IArquivoRepository, ArquivoRepository>();
        services.AddScoped<ArchiFlow.Domain.Honorarios.IPropostaHonorarioRepository, ArchiFlow.Infrastructure.Repositories.Honorarios.PropostaHonorarioRepository>();
        services.AddScoped<ArchiFlow.Domain.Honorarios.IConfiguracaoPropostaRepository, ArchiFlow.Infrastructure.Repositories.Honorarios.ConfiguracaoPropostaRepository>();
        services.AddScoped<ArchiFlow.Domain.Dashboard.IPreferenciaDashboardRepository, ArchiFlow.Infrastructure.Repositories.Dashboard.PreferenciaDashboardRepository>();
        services.AddScoped<ArchiFlow.Domain.Financeiro.IParcelaFinanceiraRepository, ArchiFlow.Infrastructure.Repositories.Financeiro.ParcelaFinanceiraRepository>();
        services.AddScoped<ArchiFlow.Domain.Financeiro.IContratoFinanceiroRepository, ArchiFlow.Infrastructure.Repositories.Financeiro.ContratoFinanceiroRepository>();
        services.AddScoped<ArchiFlow.Domain.Financeiro.IDespesaProjetoRepository, ArchiFlow.Infrastructure.Repositories.Financeiro.DespesaProjetoRepository>();
        services.AddScoped<ArchiFlow.Domain.Agenda.ICompromissoRepository, ArchiFlow.Infrastructure.Repositories.Agenda.CompromissoRepository>();
        services.AddScoped<ArchiFlow.Domain.Agenda.IConfiguracaoAgendaRepository, ArchiFlow.Infrastructure.Repositories.Agenda.ConfiguracaoAgendaRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services & Facades
        services.AddScoped<IProjetoService, ProjetoService>();
        services.AddScoped<IProjetoFacade, ProjetoFacade>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<ILeadFacade, LeadFacade>();
        services.AddScoped<IOrigemLeadService, OrigemLeadService>();
        services.AddScoped<IOrigemLeadFacade, OrigemLeadFacade>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IClienteFacade, ClienteFacade>();
        services.AddScoped<IArquivoService, ArchiFlow.Application.Arquivos.Services.ArquivoService>();
        services.AddScoped<IArquivoFacade, ArchiFlow.Application.Arquivos.Facades.ArquivoFacade>();
        // Financeiro Services & Facades
        services.AddScoped<IFinanceiroService, ArchiFlow.Application.Financeiro.Services.FinanceiroService>();
        services.AddScoped<IFinanceiroFacade, ArchiFlow.Application.Financeiro.Facades.FinanceiroFacade>();
        // Honorários - Strategy, Factory & Builder Patterns (GoF)
        services.AddScoped<ICalculoHonorarioStrategy, ResidencialCalculoStrategy>();
        services.AddScoped<ICalculoHonorarioStrategy, ComercialCalculoStrategy>();
        services.AddScoped<ICalculoHonorarioStrategy, CorporativoCalculoStrategy>();
        services.AddScoped<ICalculoHonorarioStrategy, InterioresCalculoStrategy>();
        services.AddScoped<IPadraoImovelStrategy, EconomicoPadraoStrategy>();
        services.AddScoped<IPadraoImovelStrategy, MedioPadraoStrategy>();
        services.AddScoped<IPadraoImovelStrategy, AltoPadraoStrategy>();
        services.AddScoped<IPadraoImovelStrategy, LuxoPadraoStrategy>();
        services.AddScoped<IHonorarioStrategyFactory, HonorarioStrategyFactory>();
        services.AddTransient<PropostaHonorarioBuilder>();
        services.AddScoped<ICalculadoraHonorariosService, ArchiFlow.Application.Honorarios.Services.CalculadoraHonorariosService>();
        services.AddScoped<IPropostaHonorarioService, ArchiFlow.Application.Honorarios.Services.PropostaHonorarioService>();
        services.AddScoped<IPropostaHonorarioFacade, ArchiFlow.Application.Honorarios.Facades.PropostaHonorarioFacade>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, ArchiFlow.Application.Usuarios.Services.UsuarioService>();
        services.AddScoped<IUsuarioFacade, ArchiFlow.Application.Usuarios.Facades.UsuarioFacade>();

        // Chat Repositories, Services & Facades
        services.AddScoped<ArchiFlow.Domain.Chat.IMensagemChatRepository, ArchiFlow.Infrastructure.Repositories.Chat.MensagemChatRepository>();
        services.AddScoped<IMensagemChatService, ArchiFlow.Application.Chat.Services.MensagemChatService>();
        services.AddScoped<IMensagemChatFacade, ArchiFlow.Application.Chat.Facades.MensagemChatFacade>();

        // Dashboard Services & Facades
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IDashboardFacade, DashboardFacade>();

        // Agenda Services & Facades
        services.AddScoped<IUserContextService, UserContextService>();
        services.AddScoped<IAgendaValidationService, ArchiFlow.Application.Agenda.Services.AgendaValidationService>();
        services.AddSingleton<IOAuthStateService, OAuthStateService>();
        services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
        services.AddScoped<IAgendaService, ArchiFlow.Application.Agenda.Services.AgendaService>();
        services.AddScoped<IAgendaFacade, ArchiFlow.Application.Agenda.Facades.AgendaFacade>();

        // Storage & Email (Automatic environment and configuration-based registration)
        var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
        if (!string.IsNullOrWhiteSpace(smtpUser))
        {
            services.AddScoped<IEmailService, SmtpEmailService>();
        }
        else if (environment.IsProduction())
        {
            services.AddScoped<Amazon.SimpleEmail.IAmazonSimpleEmailService, Amazon.SimpleEmail.AmazonSimpleEmailServiceClient>();
            services.AddScoped<IEmailService, SesEmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, ConsoleEmailService>();
        }

        if (environment.IsProduction())
        {
            services.AddScoped<Amazon.S3.IAmazonS3, Amazon.S3.AmazonS3Client>();
            services.AddScoped<IStorageService, S3StorageService>();
        }
        else
        {
            services.AddScoped<IStorageService, LocalStorageService>();
        }

        // SignalR
        services.AddSignalR();

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

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy("ApenasAdmin", policy => policy.RequireRole(Roles.Administrador, Roles.ArquitetoAdmin))
            .AddPolicy("ApenasGerenteOuAdmin", policy => policy.RequireRole(Roles.Administrador, Roles.Gerente, Roles.ArquitetoAdmin))
            .AddPolicy("AcessoArquiteto", policy => policy.RequireRole(
                Roles.Administrador,
                Roles.Gerente,
                Roles.Colaborador,
                Roles.ArquitetoAdmin,
                Roles.ArquitetoColaborador,
                Roles.Estagiario,
                Roles.Financeiro))
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
