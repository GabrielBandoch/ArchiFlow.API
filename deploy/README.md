# Guia de Deploy — Cotu Platform na Hostinger VPS (Ubuntu)

Este guia ensina o passo a passo completo para colocar a plataforma **Cotu** em produção na sua VPS Ubuntu da Hostinger, com **1 banco de dados por cliente** e certificados SSL automáticos gratuitos para os subdomínios `staging.seudominio.com` e `duna.seudominio.com`.

---

## 1. Apontamento de DNS na Hostinger

No painel da Hostinger (Zona DNS do seu domínio):
Crie os seguintes registros do tipo **A**, apontando para o **IP público da sua VPS**:

| Tipo | Nome (Host) | Conteúdo (IP) | TTL | Descrição |
| :--- | :--- | :--- | :--- | :--- |
| **A** | `@` | `SEU_IP_VPS` | 14400 | Domínio principal (`seudominio.com`) |
| **A** | `staging` | `SEU_IP_VPS` | 14400 | Ambiente de Homologação / Staging |
| **A** | `duna` | `SEU_IP_VPS` | 14400 | Instância isolada do cliente Duna |
| **A** | `api` | `SEU_IP_VPS` | 14400 | Acesso direto à API e Swagger |
| **A** | `*` *(Opcional)* | `SEU_IP_VPS` | 14400 | Wildcard: qualquer novo tenant futuro |

---

## 2. Passo a Passo no Servidor VPS Ubuntu

Conecte-se via SSH na sua máquina da Hostinger:

```bash
ssh root@SEU_IP_VPS
```

### 2.1. Criar pasta e clonar os repositórios

Recomendamos manter os dois projetos dentro do diretório `/opt/cotu`:

```bash
mkdir -p /opt/cotu
cd /opt/cotu

# Clonar o backend e frontend
git clone https://github.com/GabrielBandoch/ArchiFlow.API.git
git clone https://github.com/GabrielBandoch/ArchiFlow.git
```

### 2.2. Configurar as variáveis de produção

Acesse a pasta de deploy e crie o `.env`:

```bash
cd /opt/cotu/ArchiFlow.API/deploy
cp .env.prod.example .env
nano .env
```

Preencha os campos essenciais:
- `DOMAIN_NAME`: seu domínio registrado (ex: `cotu.com.br` ou `meuescritorio.com.br`)
- `ACME_EMAIL`: seu e-mail para emissão do SSL Let's Encrypt
- `DB_PASSWORD`: senha forte para o PostgreSQL
- `JWT_SECRET`: chave aleatória com mais de 32 caracteres
- `SEED_ADMIN_PASSWORD`: senha inicial do seu usuário administrador
- `SMTP_*`: credenciais de e-mail da Hostinger

Salve e saia do nano com `Ctrl + O`, `Enter` e `Ctrl + X`.

### 2.3. Executar o Deploy com 1 Comando

Dê permissão de execução e inicie o script:

```bash
chmod +x deploy.sh
./deploy.sh
```

O script irá:
1. Instalar o Docker oficial caso ainda não esteja instalado.
2. Criar a rede e os volumes de dados persistentes.
3. Subir o PostgreSQL 16 com os bancos `cotu`, `cotu_staging` e `cotu_duna`.
4. Compilar e inicializar o Backend .NET 8 (aplicando as migrações e seeds em todos os bancos).
5. Compilar o Frontend Angular otimizado em produção e servir via Nginx.
6. Inicializar o Caddy Proxy, que emite os certificados HTTPS automaticamente para seus subdomínios.

---

## 3. Ambientes Disponíveis após o Deploy

Assim que o deploy finalizar, seus ambientes estarão imediatamente disponíveis:

- 🧪 **Staging / Homologação**: `https://staging.seu-dominio.com`  
  *Conectado isoladamente ao banco `cotu_staging`.*
- 🏛️ **Cliente Duna**: `https://duna.seu-dominio.com`  
  *Conectado isoladamente ao banco `cotu_duna`.*
- 📖 **Swagger / Documentação da API**: `https://api.seu-dominio.com/swagger`

---

## 4. Como funciona o Multi-Tenancy (1 Banco por Cliente)

1. **Resolução de Tenant**: O Caddy recebe a requisição em `duna.seu-dominio.com` e a encaminha para a API com o cabeçalho `Host: duna.seu-dominio.com`.
2. **Isolamento de Conexão**: O `TenantResolutionMiddleware` detecta o subdomínio `duna` e direciona todas as queries do Entity Framework para a base `cotu_duna`.
3. **Provisionamento Dinâmico**: O serviço `ITenantDatabaseProvisioner` garante que os bancos sejam criados e migrados automaticamente.
4. **Novos Clientes Futuros**: Para adicionar um novo cliente (ex: `escritorio-alfa`):
   - Basta apontar o subdomínio `alfa.seu-dominio.com` para a VPS.
   - No primeiro acesso, a API criará o banco `cotu_alfa`, aplicará todas as migrações e tabelas sem necessidade de reiniciar o servidor.

---

## 5. Comandos Úteis do Dia a Dia

- **Ver status dos containers**:
  ```bash
  docker compose -f docker-compose.prod.yml ps
  ```
- **Ver logs em tempo real**:
  ```bash
  docker compose -f docker-compose.prod.yml logs -f
  # Ou logs apenas da API:
  docker compose -f docker-compose.prod.yml logs -f api
  # Ou logs do Caddy (certificados SSL):
  docker compose -f docker-compose.prod.yml logs -f proxy
  ```
- **Atualizar a aplicação com novas versões do Git**:
  ```bash
  ./deploy.sh
  ```
