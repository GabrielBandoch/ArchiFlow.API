# Evolução Arquitetural e ADRs (RFC v1.0 → Sistema Entregue)

Este documento registra as principais evoluções técnicas do backend desde a aprovação do RFC (Março/2026) até a entrega em produção (Novembro/2026) e as **Decisões Arquiteturais (ADRs)** que explicam o *porquê* de cada escolha.

---

## 1. Resumo das Mudanças Estruturais no Backend

1. **Adoção de Multi-Tenancy Database-per-Tenant:** saída do modelo single-database para `cotu_<subdominio>` com auto-provisionamento thread-safe (`TenantDatabaseProvisioner`) e proteção anti-spoofing (`TenantResolutionMiddleware`).
2. **Introdução da Camada `Facade`:** desacoplamento entre os 17 Controllers REST / `ChatHub` e os Services de aplicação.
3. **Antecipação dos Módulos Financeiro e Fornecedores:** originalmente marcados como Pós-TCC no RFC, foram implementados integralmente e validados com a Duna Arquitetura (incluindo despesas avulsas `DSP_Projeto_Id` nullable).
4. **Novos Módulos Demandados em Campo:**
   - **Chat em Tempo Real (`ChatHub` + `MensagensChatController`):** SignalR com validação anti-IDOR por projeto e suporte multi-tenant.
   - **Templates de Workflow (`TemplatesProjetoController`):** 4 templates pré-semeados instanciáveis na criação de projetos.
   - **Multi-Representantes & Ativação (`ClienteRepresentantesController` + `AtivacaoContaController`):** convites com token hash e ativação individual do Portal do Cliente.
   - **Equipe & RBAC Granular (`UsuariosController` + `[RequireModule]`):** papéis e permissões por módulo (`USR_Modulos_Permitidos`).
   - **Recuperação de Senha (`RecuperacaoSenhaController`):** tokens hash com expiração e limite de tentativas.
   - **White-Label (`EmpresaConfiguracoesController`):** marca, logo S3 e cor primária por escritório.
5. **Consolidação de Escopo (Commit `5d1f57d` — 26/10/2026):** o protótipo do **Simulador de Honorários** (Strategy/Builder) foi removido da versão de entrega do TCC para simplificar a manutenção e manter o foco no fluxo operacional validado em produção com a Duna Arquitetura, permanecendo no roadmap Pós-TCC conforme previsto no RFC.

---

## 2. Registro de Decisões Arquiteturais (ADRs)

| ADR | Decisão | Motivação Principal |
| :--- | :--- | :--- |
| **ADR-01** | **Database-per-Tenant (`cotu_<subdominio>`)** | Garantir isolamento físico absoluto entre escritórios clientes, simplificar backups e atender requisitos da LGPD sem risco de vazamento por filtro SQL esquecido. |
| **ADR-02** | **Provisionamento Automático Sob Demanda + Caddy TLS** | Permitir que novos subdomínios recebam certificado SSL e banco migrado/semeado na primeira requisição sem intervenção manual. |
| **ADR-03** | **Separação do Serviço Privado de Assinaturas** | Isolar a gestão global de assinaturas/faturamento (`Tenants_Assinaturas` no banco master `cotu`) da API dos workspaces de clientes, eliminando riscos de escalonamento cross-tenant. |
| **ADR-04** | **Multi-Representantes por Cliente com Convite Assinado** | Atender projetos com múltiplos tomadores de decisão (casais, sócios, empresas) mantendo credenciais individuais e compatibilidade com acesso legado. |
| **ADR-05** | **Desacoplamento de `ProjetoId` no Financeiro e Prazos por Etapa/Tarefa** | Permitir lançamento de despesas administrativas gerais do escritório e dar visibilidade real de datas por etapa/entregável no Portal do Cliente. |
| **ADR-06** | **Chat em Tempo Real por Projeto (SignalR)** | Atender necessidade real identificada na extensão com a Duna Arquitetura, centralizando a comunicação técnica dentro do projeto e eliminando dispersão no WhatsApp. |
| **ADR-07** | **Remoção Temporária do Simulador de Honorários (`5d1f57d`)** | Reduzir complexidade acidental e priorizar os módulos efetivamente usados na rotina diária do escritório parceiro. |
| **ADR-08** | **Clean Architecture + Facade + RBAC (`[RequireModule]`)** | Manter controllers enxutos, `Domain` livre de dependências e autorizações flexíveis por módulo além da role fixa. |
| **ADR-09** | **Produção em VPS Docker/Caddy + AWS S3/SES + CDK em `infra/`** | Entregar custo previsível, SSL automático e zero cold start no curto prazo, preservando a infraestrutura AWS CDK pronta para escala futura. |

> Para a descrição detalhada (Contexto, Alternativas e Consequências) de cada ADR, consulte também [`EVOLUCAO_RFC_VS_ENTREGUE.md`](https://github.com/GabrielBandoch/ArchiFlow/blob/main/docs/EVOLUCAO_RFC_VS_ENTREGUE.md).
