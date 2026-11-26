# Wiki Técnica — COTU (ArchiFlow) Backend API

Bem-vindo à Wiki técnica do backend da plataforma **COTU (ArchiFlow)** — API REST e SignalR em **.NET 8 / C#** com arquitetura limpa e isolamento **Multi-Tenant Database-per-Tenant**.

---

## Índice de Documentação

| Página | Conteúdo |
| :--- | :--- |
| **[Arquitetura do Sistema](Arquitetura.md)** | Clean Architecture, Facades, Diagramas C4 atualizados (Níveis 1, 2 e 3), Segurança OWASP Top 10, RBAC granular e Health Checks. |
| **[Multi-Tenancy e Provisionamento](Multi-Tenancy-e-Provisionamento.md)** | Funcionamento completo de 1 banco PostgreSQL por cliente (`cotu_<subdominio>`), `TenantResolutionMiddleware`, `TenantDatabaseProvisioner`, Caddy TLS e `DbSeeder`. |
| **[Modelo de Dados e Migrações](Modelo-de-Dados-e-Migracoes.md)** | Comparativo RFC (9 tabelas) vs. Atual (23 tabelas por tenant + `Tenants_Assinaturas` no master), ERD em Mermaid, conversor UTC e estratégia de seeds/migrações. |
| **[Evolução Arquitetural e ADRs](Evolucao-Arquitetural-e-ADRs.md)** | Comparativo entre o RFC original (Março/2026) e a versão entregue (Novembro/2026) + **9 ADRs** documentados. |
| **[Instruções de Deploy](Instrucoes-de-Deploy.md)** | Guia completo para execução local, Docker Compose e deploy AWS CDK (`infra/`). |
| **[Deploy em Produção (VPS + Caddy)](../deploy/README.md)** | Passo a passo da stack Docker em produção na Hostinger VPS (`docker-compose.prod.yml` e `Caddyfile`). |
| **[Roadmap de Infraestrutura Cloud](Plano-Futuro-Infraestrutura.md)** | Evolução estratégica proposta na AWS (Fases 1 a 7: App Runner → ECS → Aurora Serverless → Multi-Região). |

---

## Documentação Complementar (Frontend & Banca)

- **Repositório Frontend (`ArchiFlow`):** [Hub de Documentação (`docs/README.md`)](https://github.com/GabrielBandoch/ArchiFlow/blob/main/docs/README.md)
- **Guia de Demonstração para a Banca:** [`GUIA_BANCA_DEMONSTRACAO.md`](https://github.com/GabrielBandoch/ArchiFlow/blob/main/docs/GUIA_BANCA_DEMONSTRACAO.md) (`https://tcc.cotu.com.br/`)
- **RFC Original v1.0:** [`docs/rfc.md`](https://github.com/GabrielBandoch/ArchiFlow/blob/main/docs/rfc.md)
