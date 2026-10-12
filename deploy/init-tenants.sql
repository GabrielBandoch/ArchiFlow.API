-- Inicialização dos bancos de dados Multi-Tenant (1 banco de dados por cliente)
-- Executado automaticamente na inicialização do container PostgreSQL

SELECT 'CREATE DATABASE cotu'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'cotu')\gexec

SELECT 'CREATE DATABASE cotu_staging'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'cotu_staging')\gexec

SELECT 'CREATE DATABASE cotu_duna'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'cotu_duna')\gexec
