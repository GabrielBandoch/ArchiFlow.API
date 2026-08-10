# Bem-vindo à Wiki do ArchiFlow API 🚀

Esta wiki centraliza as especificações técnicas, decisões arquiteturais e guias operacionais para o backend da plataforma **ArchiFlow**.

## Páginas de Documentação

1. **[Arquitetura do Sistema](Arquitetura.md)**
   - Detalhamento do padrão Clean Architecture adotado.
   - Divisão de responsabilidades por camadas.
   - Estratégias de segurança e autenticação (JWT).
   - Práticas de qualidade e cobertura de testes.

2. **[Instruções de Deploy](Instrucoes-de-Deploy.md)**
   - Passos para execução local da API.
   - Configuração e execução via Docker Compose.
   - Estratégia de infraestrutura e deploy contínuo em nuvem usando AWS CDK (Fase 1, Fase 1.5 e Fase 2).

3. **[Roadmap de Infraestrutura (Fases 1 a 7)](Plano-Futuro-Infraestrutura.md)**
   - Evolução estratégica da infraestrutura cloud do ArchiFlow.
   - Detalhamento do escopo atual (Fases 1 a 3) e o planejamento futuro (Fases 4 a 7: Aurora Serverless, WAF/KMS, Observabilidade e Multi-Região).

---

## Módulos do Sistema e Roadmap

- **Projetos:** Módulo principal de acompanhamento de progresso.
- **Clientes:** Cadastro de clientes da plataforma.
- **Autenticação:** Segurança baseada em tokens JWT.
- **Health Checks:** Monitoramento de saúde e integridade com o banco de dados.
