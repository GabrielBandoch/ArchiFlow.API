using ArchiFlow.Domain.Usuarios;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Projetos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Data;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class DbSeeder
{
    public static async Task MigrateAndSeedAsync(IServiceProvider serviceProvider, bool isDevelopment = true)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ArchiFlowDbContext>();
            await context.Database.MigrateAsync();

            await EnsureAdditionalTablesAndColumnsAsync(context, services);
            await SeedAsync(context, isDevelopment);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
            logger.LogError(ex, "Ocorreu um erro ao aplicar as migrações ou semear o banco.");
        }
    }

    private static async Task EnsureAdditionalTablesAndColumnsAsync(ArchiFlowDbContext context, IServiceProvider services)
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE ""Clientes"" ADD COLUMN IF NOT EXISTS ""CLI_Foto_Url"" text;

                CREATE TABLE IF NOT EXISTS ""Tarefas_Etapa"" (
                    ""TAR_Id"" uuid NOT NULL PRIMARY KEY,
                    ""TAR_Etapa_Id"" uuid NOT NULL,
                    ""TAR_Titulo"" character varying(300) NOT NULL,
                    ""TAR_Concluida"" boolean NOT NULL DEFAULT false,
                    ""TAR_Criado_Em"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""FK_Tarefas_Etapa_Etapas_Projeto"" FOREIGN KEY (""TAR_Etapa_Id"") REFERENCES ""Etapas_Projeto"" (""ETo_Id"") ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS ""Templates_Projeto"" (
                    ""TMP_Id"" uuid NOT NULL PRIMARY KEY,
                    ""TMP_Codigo"" character varying(100) NOT NULL,
                    ""TMP_Nome"" character varying(200) NOT NULL,
                    ""TMP_Descricao"" text,
                    ""TMP_Icone"" character varying(50),
                    ""TMP_Ativo"" boolean NOT NULL DEFAULT true,
                    ""TMP_Criado_Em"" timestamp with time zone NOT NULL
                );

                CREATE TABLE IF NOT EXISTS ""Templates_Etapa"" (
                    ""TME_Id"" uuid NOT NULL PRIMARY KEY,
                    ""TME_Template_Id"" uuid NOT NULL,
                    ""TME_Nome"" character varying(200) NOT NULL,
                    ""TME_Descricao"" text,
                    ""TME_Ordem"" integer NOT NULL,
                    ""TME_Tarefas_Json"" text,
                    CONSTRAINT ""FK_Templates_Etapa_Templates_Projeto"" FOREIGN KEY (""TME_Template_Id"") REFERENCES ""Templates_Projeto"" (""TMP_Id"") ON DELETE CASCADE
                );
            ");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
            logger.LogWarning(ex, "Verificação de tabelas auxiliares concluída.");
        }
    }

    public static async Task SeedAsync(ArchiFlowDbContext context, bool isDevelopment = true)
    {
        await SeedOrigensLeadAsync(context);
        await SeedTemplatesProjetoAsync(context);
        await SeedUsuariosAsync(context, isDevelopment);
    }

    private static async Task SeedUsuariosAsync(ArchiFlowDbContext context, bool isDevelopment)
    {
        if (await context.Usuarios.AnyAsync())
        {
            return;
        }

        var adminEmail = Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL") ?? "admin@archiflow.com";
        var adminName = Environment.GetEnvironmentVariable("SEED_ADMIN_NAME") ?? "Administrador do Sistema";
        var adminPass = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD") ?? "Admin@123";

        var admin = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = adminName,
            Email = adminEmail,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(adminPass, workFactor: 12),
            Role = Roles.Administrador,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var usuarios = new List<Usuario> { admin };

        if (isDevelopment)
        {
            var gerenteEmail = Environment.GetEnvironmentVariable("SEED_GERENTE_EMAIL") ?? "gerente@archiflow.com";
            var gerenteName = Environment.GetEnvironmentVariable("SEED_GERENTE_NAME") ?? "Gerente Arquiteto";
            var gerentePass = Environment.GetEnvironmentVariable("SEED_GERENTE_PASSWORD") ?? "Gerente@123";

            var colabEmail = Environment.GetEnvironmentVariable("SEED_COLABORADOR_EMAIL") ?? "colaborador@archiflow.com";
            var colabName = Environment.GetEnvironmentVariable("SEED_COLABORADOR_NAME") ?? "Colaborador Arquiteto";
            var colabPass = Environment.GetEnvironmentVariable("SEED_COLABORADOR_PASSWORD") ?? "Colaborador@123";

            usuarios.Add(new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = gerenteName,
                Email = gerenteEmail,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(gerentePass, workFactor: 12),
                Role = Roles.Gerente,
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            });

            usuarios.Add(new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = colabName,
                Email = colabEmail,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(colabPass, workFactor: 12),
                Role = Roles.Colaborador,
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            });
        }

        await context.Usuarios.AddRangeAsync(usuarios);
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

    private static async Task SeedTemplatesProjetoAsync(ArchiFlowDbContext context)
    {
        if (await context.TemplatesProjeto.AnyAsync())
        {
            return;
        }

        var residencialId = Guid.NewGuid();
        var residencial = new TemplateProjeto
        {
            Id = residencialId,
            Codigo = "residencial-completo",
            Nome = "Projeto Arquitetônico Residencial",
            Descricao = "Fluxo completo para casas e edifícios: do levantamento ao caderno executivo final.",
            Icone = "home",
            Ativo = true,
            CriadoEm = DateTime.UtcNow,
            Etapas = new List<TemplateEtapa>
            {
                new TemplateEtapa
                {
                    Id = Guid.NewGuid(),
                    TemplateProjetoId = residencialId,
                    Ordem = 1,
                    Nome = "Briefing e Estudo Preliminar",
                    Descricao = "Levantamento de necessidades, programa e zoneamento espacial.",
                    TarefasJson = JsonSerializer.Serialize(new[]
                    {
                        "Entrevista de briefing com o cliente e alinhamento de expectativas",
                        "Levantamento métrico e fotográfico no terreno/local",
                        "Estudo de insolação, ventilação e plano diretor municipal",
                        "Esboço preliminar de zoneamento e volumetria"
                    })
                },
                new TemplateEtapa
                {
                    Id = Guid.NewGuid(),
                    TemplateProjetoId = residencialId,
                    Ordem = 2,
                    Nome = "Anteprojeto e Modelagem 3D",
                    Descricao = "Definição arquitetônica, plantas cotadas e maquete eletrônica.",
                    TarefasJson = JsonSerializer.Serialize(new[]
                    {
                        "Plantas baixas cotadas com layout humanizado",
                        "Cortes esquemáticos longitudinais e transversais",
                        "Fachadas e volumetria externa",
                        "Modelagem 3D e renderizações fotorealistas"
                    })
                },
                new TemplateEtapa
                {
                    Id = Guid.NewGuid(),
                    TemplateProjetoId = residencialId,
                    Ordem = 3,
                    Nome = "Projeto Executivo e Detalhamento",
                    Descricao = "Detalhamento construtivo para obra, ampliações e paginações.",
                    TarefasJson = JsonSerializer.Serialize(new[]
                    {
                        "Plantas executivas de alvenaria e cotas de obra",
                        "Paginação de pisos, revestimentos e forro de gesso",
                        "Projeto luminotécnico e pontos elétricos/hidráulicos",
                        "Detalhamento de esquadrias, guarda-corpos e bancadas",
                        "Memorial descritivo de materiais e acabamentos"
                    })
                },
                new TemplateEtapa
                {
                    Id = Guid.NewGuid(),
                    TemplateProjetoId = residencialId,
                    Ordem = 4,
                    Nome = "Compatibilização e Entrega Técnica",
                    Descricao = "Compatibilização com projetos complementares e caderno de pranchas.",
                    TarefasJson = JsonSerializer.Serialize(new[]
                    {
                        "Compatibilização com projetos estrutural e hidrossanitário",
                        "Reunião de entrega técnica e validação final",
                        "Emissão do caderno final de pranchas em PDF para o cliente"
                    })
                }
            }
        };

        var interioresId = Guid.NewGuid();
        var interiores = new TemplateProjeto
        {
            Id = interioresId,
            Codigo = "interiores-reforma",
            Nome = "Design de Interiores & Reforma",
            Descricao = "Foco em estética, marcenaria sob medida, iluminação e produção de ambientes.",
            Icone = "chair",
            Ativo = true,
            CriadoEm = DateTime.UtcNow,
            Etapas = new List<TemplateEtapa>
            {
                new TemplateEtapa
                {
                    Id = Guid.NewGuid(),
                    TemplateProjetoId = interioresId,
                    Ordem = 1,
                    Nome = "Conceito e Moodboard",
                    Descricao = "Alinhamento de estilo, paleta de cores e layout funcional.",
                    TarefasJson = JsonSerializer.Serialize(new[]
                    {
                        "Entrevista de estilo e levantamento de necessidades dos ambientes",
                        "Moodboard de referências visuais e paleta de materiais",
                        "Planta de layout preliminar com disposição de mobiliário"
                    })
                },
                new TemplateEtapa
                {
                    Id = Guid.NewGuid(),
                    TemplateProjetoId = interioresId,
                    Ordem = 2,
                    Nome = "Modelagem 3D e Especificação",
                    Descricao = "Renders fotorrealistas e seleção de mobiliário solto.",
                    TarefasJson = JsonSerializer.Serialize(new[]
                    {
                        "Modelagem tridimensional dos ambientes decorados",
                        "Renderizações foto-realistas com iluminação de cena",
                        "Catálogo preliminar de mobiliário solto e tapeçaria"
                    })
                },
                new TemplateEtapa
                {
                    Id = Guid.NewGuid(),
                    TemplateProjetoId = interioresId,
                    Ordem = 3,
                    Nome = "Detalhamento de Marcenaria",
                    Descricao = "Desenhos técnicos de marcenaria, pedras e paginação de acabamentos.",
                    TarefasJson = JsonSerializer.Serialize(new[]
                    {
                        "Desenhos técnicos executivos de marcenaria sob medida",
                        "Detalhamento de bancadas, cubas e marmoraria",
                        "Planta luminotécnica com circuitos e especificações de lâmpadas",
                        "Memorial descritivo de tecidos, papéis de parede e tintas"
                    })
                },
                new TemplateEtapa
                {
                    Id = Guid.NewGuid(),
                    TemplateProjetoId = interioresId,
                    Ordem = 4,
                    Nome = "Acompanhamento e Produção",
                    Descricao = "Orçamentos de fornecedores, guia de compras e vistoria de montagem.",
                    TarefasJson = JsonSerializer.Serialize(new[]
                    {
                        "Planilha consolidada de orçamentos e fornecedores parceiros",
                        "Guia de compras de objetos de decoração e arte",
                        "Vistoria de produção e entrega dos ambientes"
                    })
                }
            }
        };

        var comercialId = Guid.NewGuid();
        var comercial = new TemplateProjeto
        {
            Id = comercialId,
            Codigo = "comercial-corporativo",
            Nome = "Projeto Comercial & Corporativo",
            Descricao = "Projetos de escritórios, lojas e restaurantes com fluxo de clientes e normas.",
            Icone = "storefront",
            Ativo = true,
            CriadoEm = DateTime.UtcNow,
            Etapas = new List<TemplateEtapa>
            {
                new TemplateEtapa
                {
                    Id = Guid.NewGuid(),
                    TemplateProjetoId = comercialId,
                    Ordem = 1,
                    Nome = "Identidade e Estudo de Fluxo",
                    Descricao = "Branding do espaço, ergonomia e circulação de clientes.",
                    TarefasJson = JsonSerializer.Serialize(new[]
                    {
                        "Alinhamento de identidade visual e experiência da marca no espaço",
                        "Estudo de fluxo de clientes, colaboradores e acessibilidade NBR 9050",
                        "Zoneamento de áreas de atendimento, estoque e trabalho"
                    })
                },
                new TemplateEtapa
                {
                    Id = Guid.NewGuid(),
                    TemplateProjetoId = comercialId,
                    Ordem = 2,
                    Nome = "Layout Técnico e 3D",
                    Descricao = "Modulação de estações de trabalho e ambientação comercial.",
                    TarefasJson = JsonSerializer.Serialize(new[]
                    {
                        "Planta de layout de estações de trabalho e modulação técnica",
                        "Modelagem 3D com iluminação comercial e display de produtos",
                        "Especificação de materiais de alto tráfego e conforto acústico"
                    })
                },
                new TemplateEtapa
                {
                    Id = Guid.NewGuid(),
                    TemplateProjetoId = comercialId,
                    Ordem = 3,
                    Nome = "Executivo e Aprovações",
                    Descricao = "Pranchas para execução rápida e conformidade normativa.",
                    TarefasJson = JsonSerializer.Serialize(new[]
                    {
                        "Projeto de comunicação visual e fachada comercial",
                        "Detalhamento de expositores, balcões e marcenaria técnica",
                        "Compatibilização de ar-condicionado, dados e combate a incêndio"
                    })
                }
            }
        };

        var consultoriaId = Guid.NewGuid();
        var consultoria = new TemplateProjeto
        {
            Id = consultoriaId,
            Codigo = "consultoria-viabilidade",
            Nome = "Consultoria & Estudo de Viabilidade",
            Descricao = "Diagnóstico rápido de potencial construtivo e viabilidade para investidores.",
            Icone = "analytics",
            Ativo = true,
            CriadoEm = DateTime.UtcNow,
            Etapas = new List<TemplateEtapa>
            {
                new TemplateEtapa
                {
                    Id = Guid.NewGuid(),
                    TemplateProjetoId = consultoriaId,
                    Ordem = 1,
                    Nome = "Diagnóstico e Legislação",
                    Descricao = "Análise de zoneamento municipal e parâmetros urbanísticos.",
                    TarefasJson = JsonSerializer.Serialize(new[]
                    {
                        "Levantamento das diretrizes urbanísticas e recuos municipais",
                        "Vistoria técnica do imóvel ou loteamento",
                        "Cálculo de taxa de ocupação e potencial construtivo"
                    })
                },
                new TemplateEtapa
                {
                    Id = Guid.NewGuid(),
                    TemplateProjetoId = consultoriaId,
                    Ordem = 2,
                    Nome = "Relatório Conceitual",
                    Descricao = "Emissão do parecer técnico e croquis conceituais.",
                    TarefasJson = JsonSerializer.Serialize(new[]
                    {
                        "Elaboração de croquis conceituais de implantação",
                        "Estimativa preliminar de custos e prazos de obra",
                        "Apresentação e emissão do laudo de viabilidade em PDF"
                    })
                }
            }
        };

        await context.TemplatesProjeto.AddRangeAsync(residencial, interiores, comercial, consultoria);
        await context.SaveChangesAsync();
    }
}
