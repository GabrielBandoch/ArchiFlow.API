# COTU (ArchiFlow) — Backend API (.NET 8)

API RESTful e Hub SignalR em **.NET 8 / C#** para a plataforma multi-tenant de gestão de escritórios de arquitetura **COTU (ArchiFlow)**.

- **Instituição:** Centro Universitário Católica de Santa Catarina — Joinville/SC (Engenharia de Software — TCC)
- **Autor:** Gabriel Felipe Alves Bandoch
- **Parceiro de Extensão & Validação:** Duna Arquitetura (Joinville/SC)
- **Repositório Frontend:** [GabrielBandoch/ArchiFlow](https://github.com/GabrielBandoch/ArchiFlow)

![CI](https://github.com/GabrielBandoch/ArchiFlow.API/actions/workflows/ci.yml/badge.svg)

---

## Ambiente de Demonstração para a Banca

- **Aplicação Web (Tenant TCC):** `https://tcc.cotu.com.br/`
- **Escritório configurado (White-Label):** `COTU Studio — Demonstração TCC` (Joinville/SC)
- **Health Check Produção:** `https://api.cotu.com.br/health`
- **Guia Passo a Passo:** [Guia da Banca (`ArchiFlow/docs/GUIA_BANCA_DEMONSTRACAO.md`)](https://github.com/GabrielBandoch/ArchiFlow/blob/main/docs/GUIA_BANCA_DEMONSTRACAO.md)

### Credenciais de Teste Prontas (Admin, Equipe RBAC e Portal do Cliente)

| Perfil / Papel | E-mail | Senha | Módulos / Acesso |
| :--- | :--- | :--- | :--- |
| **Arquiteto Sócio / Admin (Banca)** | `professores@cotu.com.br` | `catolicasc123456` | Acesso completo aos **7 módulos operacionais** |
| **Portal do Cliente (`Família Nogueira`)** | `nogueira.cliente@cotustudio.exemplo.br` | `Cliente@2026` | `/portal` (etapas, arquivos e chat de *Residência Vale Verde*) |
| **Gerente de Projetos** *(Camila Ferreira)* | `camila.ferreira@cotustudio.exemplo.br` | `Equipe@2026` | Dashboard, Projetos, Clientes, Leads, Fornecedores e Financeiro |
| **Arquiteto Colaborador** *(Rafael Duarte)* | `rafael.duarte@cotustudio.exemplo.br` | `Equipe@2026` | Dashboard, Projetos, Clientes e Fornecedores |
| **Administrativo / Financeiro** *(Beatriz Mendes)* | `beatriz.mendes@cotustudio.exemplo.br` | `Equipe@2026` | Dashboard, Financeiro e Clientes |
| **Estagiário** *(Lucas Andrade)* | `lucas.andrade@cotustudio.exemplo.br` | `Equipe@2026` | Projetos e Fornecedores |

> **Dados já populados em `cotu_tcc`:** 4 Clientes (`Família Nogueira`, `Helena & Marcos Silveira`, `Clínica OdontoPrime`, `Café Botânico Joinville`), 4 Projetos (735 m²), 5 Fornecedores (3 avaliados), 9 Leads no Kanban, 3 Contratos (`R$ 82.000,00` / `R$ 29.000,00` recebidos), 5 Despesas (`R$ 6.345,00`), 2 arquivos visíveis no Portal e Chat SignalR ativo.

---

## Onde Está Localizada Cada Documentação

### Neste Repositório (`ArchiFlow.API`)

| Documento | Localização | O que você encontra |
| :--- | :--- | :--- |
| **Índice da Wiki** | [`wiki/Home.md`](wiki/Home.md) | Hub da documentação técnica do backend. |
| **Arquitetura & Segurança** | [`wiki/Arquitetura.md`](wiki/Arquitetura.md) | Clean Architecture, Padrão Facade, C4 atualizado (Níveis 1, 2 e 3), OWASP Top 10, RBAC e Health Checks. |
| **Multi-Tenancy & Criação da Base** | [`wiki/Multi-Tenancy-e-Provisionamento.md`](wiki/Multi-Tenancy-e-Provisionamento.md) | Fluxo completo Database-per-Tenant (`cotu_<subdominio>`), `TenantResolutionMiddleware`, `TenantDatabaseProvisioner` e `DbSeeder`. |
| **Modelo de Dados & Migrações** | [`wiki/Modelo-de-Dados-e-Migracoes.md`](wiki/Modelo-de-Dados-e-Migracoes.md) | Comparativo RFC (9 tabelas) vs. Atual (23 tabelas por tenant + `Tenants_Assinaturas` no master), ERD em Mermaid, conversor UTC e seeds. |
| **Evolução Arquitetural & ADRs** | [`wiki/Evolucao-Arquitetural-e-ADRs.md`](wiki/Evolucao-Arquitetural-e-ADRs.md) | Mudanças desde o RFC v1.0, consolidação de escopo (remoção temporária do Simulador) e ADRs 01–09. |
| **Instruções de Deploy** | [`wiki/Instrucoes-de-Deploy.md`](wiki/Instrucoes-de-Deploy.md) | Execução local, Docker Compose e infraestrutura AWS CDK. |
| **Deploy VPS Hostinger & Caddy** | [`deploy/README.md`](deploy/README.md) | Guia prático de produção (`docker-compose.prod.yml`, `Caddyfile` e `deploy.sh`). |
| **Roadmap Cloud (Fases 1 a 7)** | [`wiki/Plano-Futuro-Infraestrutura.md`](wiki/Plano-Futuro-Infraestrutura.md) | Evolução estratégica em nuvem AWS (App Runner → ECS → Aurora Serverless → Multi-Região). |

### Documentação Complementar no Frontend (`ArchiFlow/docs/`)

| Documento | Link |
| :--- | :--- |
| Hub Central & RFC Original | [`ArchiFlow/docs/README.md`](https://github.com/GabrielBandoch/ArchiFlow/blob/main/docs/README.md) · [`rfc.md`](https://github.com/GabrielBandoch/ArchiFlow/blob/main/docs/rfc.md) |
| Evolução RFC vs. Sistema Entregue | [`EVOLUCAO_RFC_VS_ENTREGUE.md`](https://github.com/GabrielBandoch/ArchiFlow/blob/main/docs/EVOLUCAO_RFC_VS_ENTREGUE.md) |
| Multi-Tenancy, Pagamento & Provisionamento | [`ARQUITETURA_MULTITENANT_E_PROVISIONAMENTO.md`](https://github.com/GabrielBandoch/ArchiFlow/blob/main/docs/ARQUITETURA_MULTITENANT_E_PROVISIONAMENTO.md) |
| Modelo de Dados Completo (23 + 1 Tabelas) | [`MODELO_DE_DADOS.md`](https://github.com/GabrielBandoch/ArchiFlow/blob/main/docs/MODELO_DE_DADOS.md) |

---

## Stack Tecnológica

| Camada | Tecnologia |
| :--- | :--- |
| API & Web | ASP.NET Core 8 · REST Controllers · Swagger/OpenAPI (Development) |
| Tempo Real | SignalR (`/hubs/chat`) autenticado via JWT |
| ORM & Banco | Entity Framework Core 8 · PostgreSQL 16 (Database-per-Tenant) |
| Mapeamento & Segurança | AutoMapper 12 · BCrypt.Net (`workFactor: 12`) · JWT Bearer (HS256) |
| Integrações | Amazon S3 (Storage) · Amazon SES / MailKit SMTP (E-mails) |
| Observabilidade | Serilog · ASP.NET Core Health Checks (`/health`) |
| Testes & Qualidade | xUnit · FluentAssertions · Moq · Bogus (`84` suítes) · SonarCloud |
| Produção & IaC | Docker Compose · Caddy 2 · GitHub Actions · AWS CDK (`infra/`) |

---

## Estrutura da Solução ("Onde está cada coisa")

```text
ArchiFlow.sln
├── src/
│   ├── ArchiFlow.Domain/           # Entidades, Enums, Roles, ModulosApp e interfaces de Repository/Tenant (sem deps externas)
│   ├── ArchiFlow.Application/      # Commands, DTOs, Services, Facades, AutoMapper Profile e FileSecurityValidator
│   ├── ArchiFlow.Infrastructure/   # ArchiFlowDbContext, DbSeeder, MultiTenancy (TenantContext, TenantDatabaseProvisioner),
│   │                               # Repositories, UnitOfWork, S3/LocalStorage e SES/SMTP Email
│   └── ArchiFlow.API/              # Controllers (17), ChatHub, Middlewares (Exception, TenantResolution),
│                                   # Security (RequireModule, ProjetoOwnerHandler), HealthChecks, Program.cs
├── tests/
│   └── ArchiFlow.Tests/            # 84 arquivos de testes unitários e de integração (xUnit + FluentAssertions + Moq)
├── deploy/                         # docker-compose.prod.yml, Caddyfile, deploy.sh, init-tenants.sql, README.md
├── infra/                          # Projeto AWS CDK em C# (IaC para fases Cloud AWS)
└── wiki/                           # Documentação técnica detalhada da arquitetura, banco, deploy e ADRs
```

### Dependências entre Projetos

```text
Domain  <── Infrastructure
Domain  <── Application <── Infrastructure
Domain  <── Application <── API
Domain  <── Application <── Tests
```

> **Infrastructure não referencia Application** — garantindo ausência de dependências circulares e mantendo `Domain` puro.

---

## Mapa de Controllers, SignalR Hub e Autorização

| Recurso | Rota Base | Módulo (`[RequireModule]`) | Políticas / Acesso |
| :--- | :--- | :--- | :--- |
| `AutenticacaoController` | `/api/auth` | Público | `login`, `register` |
| `AtivacaoContaController` | `/api/auth` | Público | `convite/{token}`, `ativar-conta` (Representantes) |
| `RecuperacaoSenhaController` | `/api/recuperacao-senha` | Público | Solicitação, validação e redefinição por token |
| `DashboardController` | `/api/dashboard` | `dashboard` | `AcessoArquiteto` (métricas e preferências de widgets) |
| `LeadsController` | `/api/leads` | `leads` | `AcessoArquiteto` (`ApenasAdmin` para exclusão) |
| `OrigensLeadController` | `/api/origens-lead` | `leads` / `configuracoes` | `AcessoArquiteto` / `ApenasGerenteOuAdmin` |
| `ClientesController` | `/api/clientes` | `clientes` | `AcessoArquiteto` (conversão de lead, CRUD, portal) |
| `ClienteRepresentantesController` | `/api/clientes/{id}/representantes` | `clientes` | `AcessoArquiteto` (multi-representantes e convites) |
| `ProjetosController` | `/api/projetos` | `projetos` | `AcessoArquiteto`, `ApenasGerenteOuAdmin`, `ProjetoOwner` |
| `TemplatesProjetoController` | `/api/templates-projeto` | `projetos` | `AcessoArquiteto` / `ApenasGerenteOuAdmin` |
| `ArquivosController` | `/api/arquivos` | — | `ProjetoOwner` (leitura) / `AcessoArquiteto` (upload/delete) |
| `MensagensChatController` | `/api/mensagens` | — | `ProjetoOwner` (histórico e marcação de leitura) |
| `ChatHub` *(SignalR)* | `/hubs/chat` | — | `[Authorize]` + verificação `PossuiAcessoAoProjeto` + tenant |
| `FinanceiroController` | `/api/financeiro` | `financeiro` | `AcessoArquiteto` (contratos, parcelas, baixas, despesas) |
| `FornecedoresController` | `/api/fornecedores` | `fornecedores` | `AcessoArquiteto` (parceiros, notas 1–5 e vínculos) |
| `UsuariosController` | `/api/usuarios` | `configuracoes` *(equipe)* | Perfil próprio (`/me`) + `ApenasGerenteOuAdmin` (equipe) |
| `EmpresaConfiguracoesController` | `/api/configuracoes/empresa` | `configuracoes` | `AcessoArquiteto` / `ApenasGerenteOuAdmin` (White-Label) |
| `ConfiguracoesController` | `/api/configuracoes/opcoes` | `configuracoes` | `AcessoArquiteto` / `ApenasGerenteOuAdmin` |
| `DatabaseHealthCheck` | `/health` | Público | Verificação ativa do PostgreSQL (`Healthy` / `Unhealthy`) |

---

## Pré-requisitos e Execução

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [PostgreSQL 15+](https://www.postgresql.org/download/)

### Execução Local

```bash
# 1. Copie o arquivo de exemplo e configure DB_*, JWT_* e variáveis opcionais
cp .env.example .env

# 2. Restaure dependências e execute a API (migrations e seeds rodam automaticamente)
dotnet restore ArchiFlow.sln
cd src/ArchiFlow.API
dotnet run
```

| Recurso | URL Local | Descrição |
| :--- | :--- | :--- |
| API Base | `http://localhost:5000` | Endpoint principal da API |
| Swagger UI | `http://localhost:5000/swagger` | Documentação interativa *(habilitada em `Development`)* |
| Health Check | `http://localhost:5000/health` | Status de saúde da API e PostgreSQL |

### Testes Automatizados

```bash
# Rodar todas as suítes de testes
dotnet test ArchiFlow.sln

# Com relatório de cobertura (Cobertura XML)
dotnet test ArchiFlow.sln --collect:"XPlat Code Coverage" --settings test.runsettings
```

---

## CI/CD, Cobertura de Testes e Conformidade

O pipeline em [`.github/workflows/ci.yml`](.github/workflows/ci.yml) executa em todo `push` e `pull_request` para `main`:
1. Sobe container PostgreSQL 16 de teste para testes de integração reais (`UseNpgsql`).
2. Executa análise estática de qualidade e segurança via **SonarCloud** (**Quality Gate: Passed**, `0` issues e `0` security hotspots).
3. Compila em modo `Release`, executa todas as `84` suítes xUnit e gera relatório de cobertura (**97,69% Line Coverage** e **80,56% Branch Coverage**, superando a meta de 75%).
4. No `push` em `main`, executa deploy automatizado via GitHub Actions CI/CD reconstruindo `api` e `api-staging` e validando `/health`.

### Conformidade com Normas
- **LGPD (Lei nº 13.709/2018):** isolamento físico Database-per-Tenant (`cotu_<subdominio>`), `BCrypt` (`workFactor: 12`) e hashes SHA-256 para tokens de convite/recuperação.
- **OWASP Top 10:** EF Core parametrizado, policies RBAC + `[RequireModule]` + `ProjetoOwnerHandler` + proteção anti-IDOR no `ChatHub`, `FileSecurityValidator` e Swagger restrito a `Development`.
- **Licença:** distribuído sob **MIT License** ([`LICENSE`](LICENSE)).
