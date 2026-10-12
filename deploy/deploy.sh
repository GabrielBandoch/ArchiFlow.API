#!/usr/bin/env bash
set -e

# ==============================================================================
# SCRIPT DE DEPLOY E ATUALIZAÇÃO AUTOMÁTICA - COTU PLATFORM (HOSTINGER UBUNTU)
# ==============================================================================

echo "=========================================================="
echo "      COTU PLATFORM - DEPLOY & ATUALIZAÇÃO (UBUNTU)       "
echo "=========================================================="

# 1. Verificar Docker
if ! command -v docker &> /dev/null; then
    echo "[!] Docker não encontrado. Instalando Docker oficial..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sh get-docker.sh
    systemctl enable --now docker
    rm -f get-docker.sh
fi

DEPLOY_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$DEPLOY_DIR"

# 2. Verificar arquivo de variáveis de ambiente .env
if [ ! -f .env ]; then
    echo "[!] Arquivo deploy/.env não encontrado."
    echo "[+] Criando deploy/.env a partir do modelo .env.prod.example..."
    cp .env.prod.example .env
    echo "----------------------------------------------------------"
    echo "[!] AÇÃO NECESSÁRIA: Edite o arquivo deploy/.env com o seu domínio e senhas!"
    echo "    Execute: nano $DEPLOY_DIR/.env"
    echo "    Depois execute este script novamente: ./deploy.sh"
    echo "----------------------------------------------------------"
    exit 1
fi

# 3. Atualizar código do repositório se for clone Git
echo "[+] Verificando atualizações nos repositórios Git..."
if [ -d "$DEPLOY_DIR/../.git" ]; then
    echo "[-] Atualizando backend (ArchiFlow.API)..."
    git -C "$DEPLOY_DIR/.." pull origin main || true
fi

if [ -d "$DEPLOY_DIR/../../ArchiFlow/.git" ]; then
    echo "[-] Atualizando frontend (ArchiFlow)..."
    git -C "$DEPLOY_DIR/../../ArchiFlow" pull origin main || true
fi

# 4. Construir e inicializar a stack de containers
echo "[+] Construindo e inicializando a stack de produção..."
docker compose -f docker-compose.prod.yml down --remove-orphans || true
docker compose -f docker-compose.prod.yml up -d --build

# 5. Aguardar os containers e checar status
echo "[+] Aguardando estabilização dos serviços..."
sleep 5

docker compose -f docker-compose.prod.yml ps

echo "=========================================================="
echo "    DEPLOY FINALIZADO COM SUCESSO!                        "
echo "=========================================================="

if [ -f .env ]; then
    DOMAIN_NAME=$(grep '^DOMAIN_NAME=' .env | cut -d '=' -f2- | tr -d '\r\n')
    echo "Acesse seus ambientes com SSL automático:"
    echo " -> Landing:  https://${DOMAIN_NAME} (e https://www.${DOMAIN_NAME})"
    echo " -> App SaaS: https://app.${DOMAIN_NAME}"
    echo " -> Staging:  https://staging.${DOMAIN_NAME}"
    echo " -> Duna:     https://duna.${DOMAIN_NAME}"
    echo " -> API Docs: https://api.${DOMAIN_NAME}/swagger"
fi
echo "=========================================================="
