# ArchiFlow — Backend

API REST em **.NET 8 / C#** para a plataforma de gestão de projetos de arquitetura.  
Projeto de Portfólio — Católica SC · Linha: Web Applications  
Autor: Gabriel Felipe Alves Bandoch

![CI](https://github.com/seu-usuario/archiflow-backend/actions/workflows/ci.yml/badge.svg)

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

```sql
-- Criar banco e usuário
CREATE DATABASE archiflow;
CREATE USER archiflow WITH PASSWORD 'archiflow123';
GRANT ALL PRIVILEGES ON DATABASE archiflow TO archiflow;
```

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

| Recurso    | URL                          |
|------------|------------------------------|
| API        | http://localhost:5000         |
| Swagger    | http://localhost:5000/swagger |

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

## Roadmap de Módulos

| Módulo      | Status     |
|-------------|------------|
| Projetos    | 🔜 PR-01  |
| Leads       | 🔜 PR-04  |
| Clientes    | 🔜 PR-07  |
| Honorários  | 🔜 PR-10  |
| Dashboard   | 🔜 PR-13  |
