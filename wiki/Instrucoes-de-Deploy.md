# Instruções de Deploy — ArchiFlow Backend

Este documento serve como guia prático para implantar e executar a API do ArchiFlow em diferentes ambientes.

---

## 1. Execução Local (Desenvolvimento)

### Pré-requisitos
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [PostgreSQL 15+](https://www.postgresql.org/download/)

### Passos para Iniciar
1. Copie o arquivo `.env.example` para `.env` na raiz do projeto e configure as variáveis de conexão com o banco de dados local e chaves de segurança JWT:
   ```env
   DB_HOST=localhost
   DB_PORT=5432
   DB_NAME=archiflow
   DB_USER=postgres
   DB_PASSWORD=suasenha
   JWT_SECRET=supersecretkey_minimo_32_caracteres_12345
   JWT_ISSUER=ArchiFlow
   JWT_AUDIENCE=ArchiFlowClients
   JWT_EXPIRATION_MINUTES=60
   ```
2. Restaure as dependências do .NET:
   ```bash
   dotnet restore
   ```
3. Execute as Migrations para criar as tabelas e seeds no PostgreSQL:
   ```bash
   cd src/ArchiFlow.Migrations
   dotnet run -- migrate
   ```
4. Inicie o servidor da API:
   ```bash
   cd ../ArchiFlow.API
   dotnet run
   ```
5. Acesse o console interativo do Swagger em: `http://localhost:5000/swagger` ou verifique a saúde em `http://localhost:5000/health`.

---

## 2. Execução via Docker (Ambientes de Teste / Homologação Local)

A raiz do projeto contém um arquivo `docker-compose.yml` que orquestra a API e o banco de dados PostgreSQL em containers separados.

### Passos para Iniciar
1. Certifique-se de ter o Docker e Docker Compose instalados.
2. Execute o comando para construir e inicializar os containers:
   ```bash
   docker-compose up --build -d
   ```
3. O Docker Compose irá subir:
   - PostgreSQL rodando na porta `5432` interna (exposta na porta configurável no `.env`).
   - A API ArchiFlow escutando na porta `5000`.
4. Os health checks do Docker verificarão o banco antes de liberar a API, garantindo robustez na inicialização.

---

## 3. Deploy em Nuvem (Produção via AWS CDK)

O projeto de infraestrutura de nuvem está localizado no diretório `infra/ArchiFlow.Infra` e utiliza o AWS CDK (Cloud Development Kit) escrito em C# para realizar o provisionamento da infraestrutura como código (IaC).

O deploy é segmentado em fases estratégicas utilizando a variável de ambiente `DEPLOYMENT_PHASE`:

### Fases de Deploy

1. **Fase 1: MVP (Mínimo Produto Viável)**
   - **Variável:** `DEPLOYMENT_PHASE=Phase1`
   - **Serviços:** AWS App Runner (hospedagem da API em container), Amazon S3 e CloudFront (hospedagem estática do frontend Angular), Amazon RDS PostgreSQL (`db.t3.micro` de baixo custo).
   - **Foco:** Baixo custo e validação rápida.

2. **Fase 1.5: Bootstrap (Crescimento Inicial)**
   - **Variável:** `DEPLOYMENT_PHASE=Phase15`
   - **Serviços:** AWS App Runner com containers mantidos em execução constante (*Always Warm* para evitar *cold starts*), RDS PostgreSQL (`db.t3.small` Single-AZ).
   - **Foco:** Até 5 clientes pagantes simultâneos sem impacto de latência por cold start.

3. **Fase 2: Enterprise (Alta Disponibilidade)**
   - **Variável:** `DEPLOYMENT_PHASE=Phase2`
   - **Serviços:** Amazon ECS Fargate (escala automática por CPU/Memória), Application Load Balancer (ALB) com health checks apontando para `/health`, Amazon RDS Multi-AZ PostgreSQL para failover automático, Amazon ElastiCache (Redis) para caching distribuído.
   - **Foco:** Alta disponibilidade, tolerância a falhas e escalabilidade.

4. **Fase 3: Multi-Account (Pipeline CI/CD)**
   - **Variável:** `DEPLOYMENT_PHASE=Phase3`
   - **Serviços:** Criação de múltiplos ambientes de infraestrutura isolados (Dev, Staging, Production) com pipeline integrado.

### Comando para Deploy CDK
Na pasta `infra/ArchiFlow.Infra`:
```bash
# Definir variáveis obrigatórias
export DEPLOYMENT_PHASE=Phase1
export DOMAIN_NAME=seu-dominio.com
export CDK_DEFAULT_ACCOUNT=123456789012
export CDK_DEFAULT_REGION=us-east-1

# Fazer o deploy
cdk deploy --all
```
