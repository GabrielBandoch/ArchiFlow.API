# Arquitetura do Backend — ArchiFlow

A API do ArchiFlow foi projetada com base nos princípios da **Clean Architecture** (Arquitetura Limpa), visando o desacoplamento de dependências externas, facilidade de manutenção e alta testabilidade.

## Estrutura de Camadas

A solução está organizada em 5 projetos principais no diretório `src/`:

```
ArchiFlow.sln
├── src/
│   ├── ArchiFlow.Domain/          # Entidades de Domínio, Interfaces base e Enums.
│   ├── ArchiFlow.Application/     # Serviços de Aplicação, DTOs, Mapeamentos (AutoMapper), Interfaces de Facade e Casos de Uso.
│   ├── ArchiFlow.Infrastructure/  # Acesso a Dados (EF Core + PostgreSQL), Repositórios, Unit of Work e Migrations.
│   ├── ArchiFlow.API/             # Controllers, Middlewares de Tratamento de Erros, Configurações de Start (Program.cs) e Health Checks.
│   └── ArchiFlow.Migrations/      # Ferramenta CLI para controle de Migrations e Seed de Dados de forma independente.
```

### Detalhes das Camadas

1. **Domain (Domínio):** Contém as entidades de negócio principais (ex: `Projeto`, `Usuario`, `Cliente`). É totalmente independente de frameworks e ORMs.
2. **Application (Aplicação):** Contém as regras de aplicação e portas de entrada. Orquestra a execução usando padrões como DTOs (Data Transfer Objects) e Services. A comunicação entre o Domínio e a API é mediada por esta camada.
3. **Infrastructure (Infraestrutura):** Implementa o acesso a banco de dados real via PostgreSQL utilizando o EF Core. Aqui vivem os repositórios reais que estendem as interfaces do Domínio.
4. **API (Interface Web):** Ponto de entrada do sistema via protocolo HTTP. Implementa controllers RESTful, middleware de tratamento global de exceções, autenticação baseada em JWT e o monitoramento `/health`.

---

## Design de Segurança e Autenticação

- **Autenticação JWT:** A segurança de endpoints é governada por tokens JWT (JSON Web Tokens). O login gera um token com tempo de expiração configurável e informações do usuário autenticado.
- **Autorização Baseada em Roles (Papéis):** O sistema utiliza controle de acessos (ex: `Administrador`, `Arquiteto`, `Cliente`) para restringir o acesso a recursos específicos da API.
- **Proteção de CORS:** Regras rígidas configuradas via variável de ambiente `AllowedOrigins`.

---

## Diagramas de Arquitetura (Modelo C4)

### 1. Nível 1 — Diagrama de Contexto do Sistema (C4 Context)

```mermaid
graph TD
    Arquiteto["Arquiteto / Administrador<br/>(Usuário Principal)"]
    Cliente["Cliente / Contratante<br/>(Usuário Externo)"]
    Lead["Lead / Prospect<br/>(Potencial Cliente)"]

    Sistema["<b>ArchiFlow Platform</b><br/>Gestão de Projetos, CRM de Leads, Precificação de Honorários e Finanças"]

    WhatsApp["WhatsApp API / Web<br/>(Comunicação Externa)"]
    EmailService["Serviço de E-mail / AWS SES<br/>(Notificações e Alertas)"]

    Arquiteto -->|"Gerencia projetos, simula honorários, gera propostas e controla finanças"| Sistema
    Cliente -->|"Acompanha evolução de etapas, arquivos e faturas no Portal do Cliente"| Sistema
    Lead -->|"Recebe propostas comerciais e links de atendimento"| Sistema

    Sistema -->|"Dispara mensagens formatadas de proposta"| WhatsApp
    Sistema -->|"Envia e-mails de convite e notificações"| EmailService
```

### 2. Nível 2 — Diagrama de Contêineres (C4 Container)

```mermaid
graph TD
    subgraph Cliente["Navegador Web / Dispositivo do Usuário"]
        SPA["<b>Frontend Single Page App</b><br/>[Angular 17, TypeScript, SCSS]<br/>Interface web responsiva com Design System próprio"]
    end

    subgraph Backend["Ambiente de Servidor / Cloud"]
        API["<b>Backend REST API</b><br/>[.NET 8 / ASP.NET Core]<br/>Clean Architecture, Controllers REST, Validações e Health Check"]
        Auth["<b>Módulo de Segurança & JWT</b><br/>Autenticação stateless e RBAC"]
        DB[("<b>Banco de Dados Relacional</b><br/>[PostgreSQL 16]<br/>Tabelas com suporte multi-tenant e EF Core")]
        Storage["<b>Armazenamento de Arquivos</b><br/>[Local / AWS S3]<br/>Comprovantes financeiros e entregáveis técnicos"]
    end

    SPA -->|"HTTPS / REST JSON"| API
    API -->|"Autentica requisições"| Auth
    API -->|"Persiste e consulta dados (EF Core)"| DB
    API -->|"Faz upload e streaming de anexos"| Storage
```

---

## Monitoramento e Resiliência (Health Checks)

O backend possui o endpoint `/health` que realiza verificações ativas no banco de dados relacional.
- Se o banco de dados PostgreSQL responder com sucesso às conexões, o endpoint retorna HTTP 200 (OK) com o status `Healthy`.
- Se houver falha de rede ou indisponibilidade da base de dados, o endpoint retorna HTTP 503 (Service Unavailable) com o status `Unhealthy` e os detalhes da falha.
