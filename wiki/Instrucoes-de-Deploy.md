# Instruções de Deploy — COTU (ArchiFlow) Backend

Este guia cobre a execução da API em ambiente local, via Docker em desenvolvimento/produção (VPS Hostinger + Caddy 2) e a opção IaC com AWS CDK.

---

## 1. Execução Local (Desenvolvimento)

### Pré-requisitos
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [PostgreSQL 15+](https://www.postgresql.org/download/)

### Passos para Iniciar

1. Copie `.env.example` para `.env` na raiz do repositório `ArchiFlow.API` e ajuste as variáveis locais:
   ```env
   DB_HOST=localhost
   DB_PORT=5432
   DB_NAME=cotu
   DB_USER=postgres
   DB_PASSWORD=suasenha

   JWT_SECRET=suachavesecretaaquicomnominimode32caracteres
   JWT_ISSUER=Cotu
   JWT_AUDIENCE=CotuClients
   JWT_EXPIRATION_MINUTES=480

   SEED_ADMIN_NAME=Administrador do Sistema
   SEED_ADMIN_EMAIL=admin@cotu.com
   SEED_ADMIN_PASSWORD=SuaSenhaForteAqui
   ```
   *(Variáveis opcionais para e-mail SMTP, AWS S3/SES e Google Calendar OAuth também constam em `.env.example`.)*

2. Restaure as dependências e execute a API (migrations, DDL auxiliar e seeds são executados automaticamente na inicialização):
   ```bash
   dotnet restore ArchiFlow.sln
   cd src/ArchiFlow.API
   dotnet run
   ```

3. Acesse localmente:
   - API: `http://localhost:5000`
   - Swagger UI *(ativo em `Development`)*: `http://localhost:5000/swagger`
   - Health Check: `http://localhost:5000/health`

---

## 2. Execução via Docker Compose (Desenvolvimento Local)

A raiz do repositório possui um `docker-compose.yml` para subir API + PostgreSQL localmente:

```bash
docker compose up --build -d
```

---

## 3. Deploy em Produção (Hostinger VPS Ubuntu + Caddy 2)

A stack oficial de produção está na pasta [`deploy/`](../deploy/README.md) e orquestra **9 containers** em [`deploy/docker-compose.prod.yml`](../deploy/docker-compose.prod.yml):
- `db` (`cotu-db`, PostgreSQL 16 com `init-tenants.sql`)
- `api` (`cotu-api`), `api-staging` (`cotu-api-staging`), `api-app` (`cotu-api-app`)
- `web` (`cotu-web`), `web-staging` (`cotu-web-staging`), `admin` (`cotu-admin`), `landing` (`cotu-landing`)
- `proxy` (`cotu-proxy`, Caddy 2 com HTTPS automático e `on_demand_tls` para `*.cotu.com.br`)

### Passos resumidos na VPS
1. Configure o DNS apontando `@`, `app`, `api`, `staging`, `*` (ou subdomínios específicos) para o IP público da VPS.
2. Copie `deploy/.env.prod.example` para `deploy/.env` e preencha `DOMAIN_NAME`, `ACME_EMAIL`, `DB_PASSWORD`, `JWT_SECRET`, `SEED_ADMIN_*`, `SMTP_*`, `GOOGLE_CALENDAR_*` e `AWS_*`.
3. Execute o script automatizado:
   ```bash
   cd /opt/cotu/ArchiFlow.API/deploy
   chmod +x deploy.sh
   ./deploy.sh
   ```
4. A cada `push` na branch `main`, o workflow [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) executa testes + SonarCloud e realiza o redeploy automático via SSH de `api` e `api-staging` após validar `https://api.cotu.com.br/health`.

> Consulte todos os detalhes em [`deploy/README.md`](../deploy/README.md) e [`Multi-Tenancy-e-Provisionamento.md`](Multi-Tenancy-e-Provisionamento.md).

---

## 4. Opção IaC em Nuvem AWS (AWS CDK)

O diretório [`infra/ArchiFlow.Infra`](../infra/ArchiFlow.Infra) contém o projeto **AWS CDK em C#** versionado como caminho de infraestrutura para evolução Enterprise na AWS (controlado por `DEPLOYMENT_PHASE`: `Phase1`, `Phase15`, `Phase2`, `Phase3`).

```bash
cd infra/ArchiFlow.Infra
export DEPLOYMENT_PHASE=Phase1
export DOMAIN_NAME=seu-dominio.com
export CDK_DEFAULT_ACCOUNT=123456789012
export CDK_DEFAULT_REGION=us-east-1

cdk deploy --all
```

Para detalhes das Fases 1 a 7 propostas na AWS, consulte [`Plano-Futuro-Infraestrutura.md`](Plano-Futuro-Infraestrutura.md).
