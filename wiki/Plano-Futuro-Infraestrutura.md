# Roadmap de Evolução da Infraestrutura Cloud — ArchiFlow

Este documento apresenta a proposta de evolução estratégica da infraestrutura na nuvem AWS para a plataforma **ArchiFlow**, organizando-a em fases incrementais projetadas para guiar o crescimento desde o setup inicial de validação (MVP) até arquiteturas avançadas de alta disponibilidade global e otimização automatizada.

---

## 🚀 Fases Propostas para a Infraestrutura Cloud

### Fase 1: MVP (Mínimo Produto Viável)
* **Objetivo:** Validação inicial com baixo custo operacional.
* **Arquitetura Proposta:** AWS App Runner (API REST containerizada), Amazon S3 + Amazon CloudFront (hospedagem estática do frontend Angular) e Amazon RDS PostgreSQL (`db.t3.micro`).

---

### Fase 1.5: Bootstrap (Crescimento Inicial)
* **Objetivo:** Suportar os primeiros clientes pagantes garantindo tempos de resposta rápidos e sem impacto de latência inicial (*cold start*).
* **Arquitetura Proposta:** AWS App Runner com configuração *Always Warm* e Amazon RDS PostgreSQL (`db.t3.small` Single-AZ).

---

### Fase 2: Enterprise (Alta Disponibilidade)
* **Objetivo:** Resiliência a falhas, escalabilidade horizontal e alta performance sob demanda.
* **Arquitetura Proposta:** Amazon ECS Fargate (escala de containers por consumo de CPU/Memória), Application Load Balancer (ALB) com health checks automáticos apontando para `/health`, Amazon RDS Multi-AZ PostgreSQL (failover automático em sub-redes distintas) e Amazon ElastiCache (Redis) para caching distribuído.

---

### Fase 3: Multi-Account & CI/CD Pipeline
* **Objetivo:** Ciclo de entrega contínuo, governança e isolamento de ambientes de teste.
* **Arquitetura Proposta:** AWS CodePipeline, AWS CodeBuild e separação de contas AWS via AWS Organizations (Ambientes isolados e independentes de Desenvolvimento, Homologação e Produção).

---

### Fase 4: Otimização de Custos e Auto-Scaling de Banco de Dados
* **Objetivo:** Escalar recursos de banco de dados sob demanda e reduzir custos em até 50% em períodos ociosos (finais de semana e madrugadas).
* **Arquitetura Proposta:** Migração das instâncias RDS PostgreSQL para o Amazon Aurora Serverless v2, permitindo escala automática e instantânea das ACUs (Aurora Capacity Units) conforme a demanda real de requisições.

---

### Fase 5: Segurança Avançada, WAF e Compliance (LGPD)
* **Objetivo:** Mitigar ameaças comuns da web, garantir controle de acesso estrito e criptografia robusta dos dados armazenados.
* **Arquitetura Proposta:**
  - Vinculação do AWS WAF (Web Application Firewall) ao ALB para proteção contra injeção SQL, scripts maliciosos (XSS) e rate limiting por IP.
  - Criptografia em repouso dos arquivos no S3 e volumes de banco utilizando chaves KMS gerenciadas pelo cliente.
  - Controle de segredos e rotação de credenciais via AWS Secrets Manager.

---

### Fase 6: Observabilidade e Rastreabilidade Avançada
* **Objetivo:** Monitoramento fim-a-fim, identificação preventiva de gargalos e telemetria detalhada.
* **Arquitetura Proposta:** Coleta de métricas e traces usando OpenTelemetry integrando com AWS X-Ray e Amazon CloudWatch ServiceLens para rastrear latências em consultas SQL e chamadas internas da API REST.

---

### Fase 7: Alta Disponibilidade Multi-Região e DR (Disaster Recovery)
* **Objetivo:** Garantir a continuidade do negócio com RTO (Recovery Time Objective) e RPO (Recovery Point Objective) próximos a zero frente a incidentes severos na nuvem.
* **Arquitetura Proposta:**
  - Setup de infraestrutura em regiões AWS distintas atuando de forma ativa-passiva ou ativa-ativa.
  - Replicação de dados em tempo real (sub-segundo) usando o Aurora Global Database.
  - Roteamento inteligente e failover automático de tráfego gerenciado pelo Route 53 com base em checagens de saúde e proximidade geográfica.
