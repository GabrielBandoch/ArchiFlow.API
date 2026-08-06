# ArchiFlow — Backend

API REST em **.NET 8 / C#** para a plataforma de gestão de projetos de arquitetura.  
Projeto de Portfólio — Católica SC · Linha: Web Applications  
Autor: Gabriel Felipe Alves Bandoch

![CI](https://github.com/GabrielBandoch/ArchiFlow.API/actions/workflows/ci.yml/badge.svg)

---

## Stack

| Camada         | Tecnologia                              |
|----------------|-----------------------------------------|
| API            | ASP.NET Core 8 · Swagger                |
| ORM            | Entity Framework Core 8 · PostgreSQL    |
| Mapeamento     | AutoMapper 13                           |
| Testes         | xUnit · FluentAssertions · Moq · Bogus  |
| CI             | GitHub Actions                          |

---

## Estrutura da Solução

```
ArchiFlow.sln
├── src/
│   ├── ArchiFlow.Domain/          # Entidades, enums — sem dependências externas
│   ├── ArchiFlow.Infrastructure/  # DbContext, Migrations (EF Core + PostgreSQL)
│   ├── ArchiFlow.Application/     # Services, Facades, Commands, DTOs, AutoMapper
│   ├── ArchiFlow.API/             # Controllers, Middleware, Program.cs
│   └── ArchiFlow.Migrations/      # CLI: migrate · seed · status · reset
└── tests/
    └── ArchiFlow.Tests/           # xUnit + FluentAssertions + EF InMemory
```

### Dependências entre projetos

```
Domain  ←── Infrastructure
Domain  ←── Application ←── Infrastructure
Domain  ←── Application ←── API
Domain  ←── Application ←── Tests
```

> **Infrastructure não referencia Application** — sem dependência circular.  
> O `ArchiFlowMappingProfile` vive em **Application/Mappings**.

---

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [PostgreSQL 15+](https://www.postgresql.org/download/)

---

## Como Executar

```bash
# 1. Aplicar migrations
cd src/ArchiFlow.Migrations
dotnet run -- migrate

# 2. Subir a API
cd src/ArchiFlow.API
dotnet run
```

| Recurso       | URL                            | Descrição                                         |
|---------------|--------------------------------|---------------------------------------------------|
| API           | http://localhost:5000          | Ponto de entrada base da API                      |
| Swagger       | http://localhost:5000/swagger  | Interface interativa de documentação da API       |
| Health Check  | http://localhost:5000/health   | Endpoint de verificação de integridade do sistema |

---

## Testes

```bash
# Rodar todos os testes
dotnet test

# Com relatório de cobertura
dotnet test --collect:"XPlat Code Coverage"
```

---

## CI/CD

O pipeline roda automaticamente em todo `push` e `pull_request` para `main`:

1. Restore de dependências
2. Build em modo Release
3. Execução dos testes com coleta de cobertura
4. Upload do relatório de cobertura como artefato

---

## Documentação Técnica (Wiki)

A documentação de arquitetura e deploy está estruturada para a Wiki do GitHub e pode ser acessada localmente na pasta [`wiki`](file:///c:/TCC/ArchiFlow.API/wiki/):

- **[Arquitetura do Sistema](file:///c:/TCC/ArchiFlow.API/wiki/Arquitetura.md)**: Detalhamento da Clean Architecture, modelagem e decisões de segurança.
- **[Instruções de Deploy](file:///c:/TCC/ArchiFlow.API/wiki/Instrucoes-de-Deploy.md)**: Guia completo para deploy local, Docker Compose e AWS CDK (Fases 1, 1.5, 2 e 3).
- **[Roadmap de Infraestrutura (Fases 1 a 7)](file:///c:/TCC/ArchiFlow.API/wiki/Plano-Futuro-Infraestrutura.md)**: Evolução estratégica proposta para a infraestrutura cloud do ArchiFlow, detalhando as fases iniciais sugeridas (Fases 1 a 3) e o planejamento futuro (Fases 4 a 7).

---

## Roadmap de Módulos

| Módulo      | Status     |
|-------------|------------|
| Projetos    | 🔜 PR-01  |
| Leads       | 🔜 PR-04  |
| Clientes    | 🔜 PR-07  |
| Honorários  | 🔜 PR-10  |
| Dashboard   | 🔜 PR-13  |
