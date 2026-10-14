using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Usuarios;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Chat;
using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Dashboard;
using ArchiFlow.Domain.Financeiro;
using ArchiFlow.Domain.Fornecedores;
using ArchiFlow.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace ArchiFlow.Infrastructure.Data;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class ArchiFlowDbContext : DbContext
{
    public ArchiFlowDbContext(DbContextOptions<ArchiFlowDbContext> options) : base(options) { }

    public DbSet<Projeto>              Projetos              => Set<Projeto>();
    public DbSet<EtapaProjeto>         EtapasProjeto         => Set<EtapaProjeto>();
    public DbSet<TarefaEtapa>          TarefasEtapa          => Set<TarefaEtapa>();
    public DbSet<TemplateProjeto>      TemplatesProjeto      => Set<TemplateProjeto>();
    public DbSet<TemplateEtapa>        TemplatesEtapa        => Set<TemplateEtapa>();
    public DbSet<Usuario>              Usuarios              => Set<Usuario>();
    public DbSet<Cliente>              Clientes              => Set<Cliente>();
    public DbSet<Lead>                 Leads                 => Set<Lead>();
    public DbSet<HistoricoContatoLead> HistoricosContatoLead => Set<HistoricoContatoLead>();
    public DbSet<OrigemLead>           OrigensLead           => Set<OrigemLead>();
    public DbSet<Arquivo>              Arquivos              => Set<Arquivo>();
    public DbSet<MensagemChat>         MensagensChat         => Set<MensagemChat>();
    public DbSet<PropostaHonorario>    PropostasHonorarios   => Set<PropostaHonorario>();
    public DbSet<ItemPropostaEtapa>    ItensPropostasEtapas  => Set<ItemPropostaEtapa>();
    public DbSet<PreferenciaDashboard> PreferenciasDashboard => Set<PreferenciaDashboard>();
    public DbSet<ContratoFinanceiro>   ContratosFinanceiros  => Set<ContratoFinanceiro>();
    public DbSet<ParcelaFinanceira>    ParcelasFinanceiras   => Set<ParcelaFinanceira>();
    public DbSet<DespesaProjeto>       DespesasProjetos      => Set<DespesaProjeto>();
    public DbSet<ConfiguracaoProposta> ConfiguracoesProposta => Set<ConfiguracaoProposta>();
    public DbSet<ArchiFlow.Domain.Agenda.Compromisso> Compromissos => Set<ArchiFlow.Domain.Agenda.Compromisso>();
    public DbSet<ArchiFlow.Domain.Agenda.ConfiguracaoAgendaEscritorio> ConfiguracoesAgendaEscritorio => Set<ArchiFlow.Domain.Agenda.ConfiguracaoAgendaEscritorio>();
    public DbSet<Fornecedor>           Fornecedores          => Set<Fornecedor>();
    public DbSet<AvaliacaoFornecedor>  AvaliacoesFornecedores=> Set<AvaliacaoFornecedor>();
    public DbSet<ProjetoFornecedor>    ProjetosFornecedores  => Set<ProjetoFornecedor>();
    public DbSet<OpcaoConfiguracao>    OpcoesConfiguracao    => Set<OpcaoConfiguracao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Projeto>(entity =>
        {
            entity.ToTable("Projetos");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("PJT_Id");
            entity.Property(p => p.Nome).HasColumnName("PJT_Nome").IsRequired().HasMaxLength(200);
            entity.Property(p => p.Descricao).HasColumnName("PJT_Descricao");
            entity.Property(p => p.Status).HasColumnName("PJT_Status");
            entity.Property(p => p.Tipo).HasColumnName("PJT_Tipo");
            entity.Property(p => p.DataInicio).HasColumnName("PJT_Data_Inicio");
            entity.Property(p => p.DataPrevistaEntrega).HasColumnName("PJT_Data_Prevista_Entrega");
            entity.Property(p => p.MetragemTotal).HasColumnName("PJT_Metragem_Total");
            entity.Property(p => p.ClienteId).HasColumnName("PJT_Cliente_Id");
            entity.Property(p => p.CriadoEm).HasColumnName("PJT_Criado_Em");
            entity.Property(p => p.AtualizadoEm).HasColumnName("PJT_Atualizado_Em");

            entity.HasIndex(p => p.ClienteId);

            entity.HasOne<Cliente>()
                  .WithMany()
                  .HasForeignKey(p => p.ClienteId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EtapaProjeto>(entity =>
        {
            entity.ToTable("Etapas_Projeto");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ETo_Id");
            entity.Property(e => e.ProjetoId).HasColumnName("ETo_Projeto_Id");
            entity.Property(e => e.Nome).HasColumnName("ETo_Nome").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasColumnName("ETo_Descricao");
            entity.Property(e => e.Status).HasColumnName("ETo_Status");
            entity.Property(e => e.Ordem).HasColumnName("ETo_Ordem");
            entity.Property(e => e.DataConclusao).HasColumnName("dETo_Data_Conclusao");

            entity.HasOne(e => e.Projeto)
                  .WithMany(p => p.Etapas)
                  .HasForeignKey(e => e.ProjetoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TarefaEtapa>(entity =>
        {
            entity.ToTable("Tarefas_Etapa");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("TAR_Id");
            entity.Property(t => t.EtapaId).HasColumnName("TAR_Etapa_Id");
            entity.Property(t => t.Titulo).HasColumnName("TAR_Titulo").IsRequired().HasMaxLength(300);
            entity.Property(t => t.Concluida).HasColumnName("TAR_Concluida");
            entity.Property(t => t.CriadoEm).HasColumnName("TAR_Criado_Em");

            entity.HasOne(t => t.Etapa)
                  .WithMany(e => e.Tarefas)
                  .HasForeignKey(t => t.EtapaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TemplateProjeto>(entity =>
        {
            entity.ToTable("Templates_Projeto");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("TMP_Id");
            entity.Property(t => t.Codigo).HasColumnName("TMP_Codigo").IsRequired().HasMaxLength(100);
            entity.Property(t => t.Nome).HasColumnName("TMP_Nome").IsRequired().HasMaxLength(200);
            entity.Property(t => t.Descricao).HasColumnName("TMP_Descricao");
            entity.Property(t => t.Icone).HasColumnName("TMP_Icone").HasMaxLength(50);
            entity.Property(t => t.Ativo).HasColumnName("TMP_Ativo");
            entity.Property(t => t.CriadoEm).HasColumnName("TMP_Criado_Em");
        });

        modelBuilder.Entity<TemplateEtapa>(entity =>
        {
            entity.ToTable("Templates_Etapa");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("TME_Id");
            entity.Property(e => e.TemplateProjetoId).HasColumnName("TME_Template_Id");
            entity.Property(e => e.Nome).HasColumnName("TME_Nome").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasColumnName("TME_Descricao");
            entity.Property(e => e.Ordem).HasColumnName("TME_Ordem");
            entity.Property(e => e.TarefasJson).HasColumnName("TME_Tarefas_Json");

            entity.HasOne(e => e.TemplateProjeto)
                  .WithMany(t => t.Etapas)
                  .HasForeignKey(e => e.TemplateProjetoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("USR_Id");
            entity.Property(u => u.EscritorioId).HasColumnName("USR_Escritorio_Id");
            entity.Property(u => u.Nome).HasColumnName("USR_Nome").IsRequired().HasMaxLength(200);
            entity.Property(u => u.Email).HasColumnName("USR_Email").IsRequired().HasMaxLength(256);
            entity.Property(u => u.SenhaHash).HasColumnName("USR_Senha_Hash").IsRequired();
            entity.Property(u => u.Role).HasColumnName("USR_Role").IsRequired().HasMaxLength(50);
            entity.Property(u => u.Cargo).HasColumnName("USR_Cargo").HasMaxLength(100);
            entity.Property(u => u.Telefone).HasColumnName("USR_Telefone").HasMaxLength(30);
            entity.Property(u => u.Ativo).HasColumnName("USR_Ativo");
            entity.Property(u => u.CriadoEm).HasColumnName("USR_Criado_Em");
            entity.Property(u => u.AtualizadoEm).HasColumnName("USR_Atualizado_Em");

            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.EscritorioId);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("CLI_Id");
            entity.Property(c => c.LeadId).HasColumnName("CLI_Lead_Id");
            entity.Property(c => c.Nome).HasColumnName("CLI_Nome").IsRequired().HasMaxLength(200);
            entity.Property(c => c.Email).HasColumnName("CLI_Email").IsRequired().HasMaxLength(256);
            entity.Property(c => c.Telefone).HasColumnName("CLI_Telefone").HasMaxLength(20);
            entity.Property(c => c.CpfCnpj).HasColumnName("CLI_Cpf_Cnpj").HasMaxLength(20);
            entity.Property(c => c.SenhaPortal).HasColumnName("CLI_Senha_Portal");
            entity.Property(c => c.Ativo).HasColumnName("CLI_Ativo");
            entity.Property(c => c.Endereco).HasColumnName("CLI_Endereco");
            entity.Property(c => c.FotoUrl).HasColumnName("CLI_Foto_Url");

            entity.HasIndex(c => c.Email).IsUnique();
        });

        modelBuilder.Entity<Lead>(entity =>
        {
            entity.ToTable("Leads");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Id).HasColumnName("LED_Id");
            entity.Property(l => l.Nome).HasColumnName("LED_Nome").IsRequired().HasMaxLength(200);
            entity.Property(l => l.Email).HasColumnName("LED_Email").IsRequired().HasMaxLength(256);
            entity.Property(l => l.Telefone).HasColumnName("LED_Telefone").HasMaxLength(20);
            entity.Property(l => l.OrigemId).HasColumnName("LED_Origem_Id");
            entity.Property(l => l.MotivoPerda).HasColumnName("LED_Motivo_Perda").HasMaxLength(500);
            entity.Property(l => l.Status).HasColumnName("LED_Status").IsRequired();
            entity.Property(l => l.CriadoEm).HasColumnName("LED_Criado_Em").IsRequired();
            entity.Property(l => l.AtualizadoEm).HasColumnName("LED_Atualizado_Em");

            entity.HasIndex(l => l.Email).IsUnique();

            entity.HasOne(l => l.Origem)
                  .WithMany()
                  .HasForeignKey(l => l.OrigemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrigemLead>(entity =>
        {
            entity.ToTable("Origens_Lead");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).HasColumnName("OL_Id");
            entity.Property(o => o.Descricao).HasColumnName("OL_Descricao").IsRequired().HasMaxLength(100);
            entity.Property(o => o.Ativo).HasColumnName("OL_Ativo").IsRequired();
            entity.Property(o => o.CriadoEm).HasColumnName("OL_Criado_Em").IsRequired();
        });

        modelBuilder.Entity<HistoricoContatoLead>(entity =>
        {
            entity.ToTable("Historicos_Contato_Lead");
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Id).HasColumnName("HCL_Id");
            entity.Property(h => h.LeadId).HasColumnName("HCL_Lead_Id");
            entity.Property(h => h.DataContato).HasColumnName("HCL_Data_Contato").IsRequired();
            entity.Property(h => h.Canal).HasColumnName("HCL_Canal").IsRequired().HasMaxLength(100);
            entity.Property(h => h.Resumo).HasColumnName("HCL_Resumo").IsRequired();

            entity.HasOne(h => h.Lead)
                  .WithMany(l => l.HistoricoContatos)
                  .HasForeignKey(h => h.LeadId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Arquivo>(entity =>
        {
            entity.ToTable("Arquivos");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("ARQ_Id");
            entity.Property(a => a.ProjetoId).HasColumnName("ARQ_Projeto_Id");
            entity.Property(a => a.Nome).HasColumnName("ARQ_Nome").IsRequired().HasMaxLength(255);
            entity.Property(a => a.UrlStorage).HasColumnName("ARQ_Url_Storage").IsRequired().HasMaxLength(1000);
            entity.Property(a => a.Tipo).HasColumnName("ARQ_Tipo").HasMaxLength(100);
            entity.Property(a => a.VisivelCliente).HasColumnName("ARQ_Visivel_Cliente");
            entity.Property(a => a.CriadoEm).HasColumnName("ARQ_Criado_Em");

            entity.HasOne<Projeto>()
                  .WithMany()
                  .HasForeignKey(a => a.ProjetoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MensagemChat>(entity =>
        {
            entity.ToTable("Mensagens_Chat");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id).HasColumnName("MSG_Id");
            entity.Property(m => m.ProjetoId).HasColumnName("MSG_Projeto_Id").IsRequired();
            entity.Property(m => m.RemetenteId).HasColumnName("MSG_Remetente_Id").IsRequired();
            entity.Property(m => m.RemetenteNome).HasColumnName("MSG_Remetente_Nome").IsRequired().HasMaxLength(200);
            entity.Property(m => m.RemetentePerfil).HasColumnName("MSG_Remetente_Perfil").IsRequired().HasMaxLength(50);
            entity.Property(m => m.Conteudo).HasColumnName("MSG_Conteudo").IsRequired();
            entity.Property(m => m.CriadoEm).HasColumnName("MSG_Criado_Em").IsRequired();
            entity.Property(m => m.Lida).HasColumnName("MSG_Lida").IsRequired();

            entity.HasIndex(m => m.ProjetoId);

            entity.HasOne<Projeto>()
                  .WithMany()
                  .HasForeignKey(m => m.ProjetoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PropostaHonorario>(entity =>
        {
            entity.ToTable("Propostas_Honorarios");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("PH_Id");
            entity.Property(p => p.Titulo).HasColumnName("PH_Titulo").IsRequired().HasMaxLength(200);
            entity.Property(p => p.Codigo).HasColumnName("PH_Codigo").IsRequired().HasMaxLength(50);
            entity.HasIndex(p => p.Codigo).IsUnique();
            entity.Property(p => p.ClienteId).HasColumnName("PH_Cliente_Id");
            entity.Property(p => p.ClienteNome).HasColumnName("PH_Cliente_Nome").HasMaxLength(200);
            entity.Property(p => p.LeadId).HasColumnName("PH_Lead_Id");
            entity.Property(p => p.LeadNome).HasColumnName("PH_Lead_Nome").HasMaxLength(200);
            entity.Property(p => p.TipoProjeto).HasColumnName("PH_Tipo_Projeto");
            entity.Property(p => p.PadraoImovel).HasColumnName("PH_Padrao_Imovel");
            entity.Property(p => p.MetragemQuadrada).HasColumnName("PH_Metragem_Quadrada").HasPrecision(18, 2);
            entity.Property(p => p.ValorHoraBase).HasColumnName("PH_Valor_Hora_Base").HasPrecision(18, 2);
            entity.Property(p => p.ValorMetroQuadradoBase).HasColumnName("PH_Valor_M2_Base").HasPrecision(18, 2);
            entity.Property(p => p.HorasEstimadasTotal).HasColumnName("PH_Horas_Estimadas_Total").HasPrecision(18, 2);
            entity.Property(p => p.ValorBase).HasColumnName("PH_Valor_Base").HasPrecision(18, 2);
            entity.Property(p => p.ValorFatorPadrao).HasColumnName("PH_Valor_Fator_Padrao").HasPrecision(18, 2);
            entity.Property(p => p.ValorFatorTipologia).HasColumnName("PH_Valor_Fator_Tipologia").HasPrecision(18, 2);
            entity.Property(p => p.ValorEscopo).HasColumnName("PH_Valor_Escopo").HasPrecision(18, 2);
            entity.Property(p => p.ValorTotalSugerido).HasColumnName("PH_Valor_Total_Sugerido").HasPrecision(18, 2);
            entity.Property(p => p.ValorFinalAjustado).HasColumnName("PH_Valor_Final_Ajustado").HasPrecision(18, 2);
            entity.Property(p => p.Status).HasColumnName("PH_Status");
            entity.Property(p => p.Observacoes).HasColumnName("PH_Observacoes");
            entity.Property(p => p.CriadoEm).HasColumnName("PH_Criado_Em");
            entity.Property(p => p.AtualizadoEm).HasColumnName("PH_Atualizado_Em");

            entity.HasOne(p => p.Cliente)
                  .WithMany()
                  .HasForeignKey(p => p.ClienteId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(p => p.Lead)
                  .WithMany()
                  .HasForeignKey(p => p.LeadId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(p => p.ItensEtapa)
                  .WithOne(i => i.Proposta)
                  .HasForeignKey(i => i.PropostaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemPropostaEtapa>(entity =>
        {
            entity.ToTable("Itens_Propostas_Etapas");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Id).HasColumnName("IPE_Id");
            entity.Property(i => i.PropostaId).HasColumnName("IPE_Proposta_Id");
            entity.Property(i => i.NomeEtapa).HasColumnName("IPE_Nome_Etapa").IsRequired().HasMaxLength(200);
            entity.Property(i => i.Descricao).HasColumnName("IPE_Descricao");
            entity.Property(i => i.Incluso).HasColumnName("IPE_Incluso");
            entity.Property(i => i.Percentual).HasColumnName("IPE_Percentual").HasPrecision(18, 2);
            entity.Property(i => i.Valor).HasColumnName("IPE_Valor").HasPrecision(18, 2);
            entity.Property(i => i.HorasEstimadas).HasColumnName("IPE_Horas_Estimadas").HasPrecision(18, 2);
            entity.Property(i => i.Ordem).HasColumnName("IPE_Ordem");
        });

        modelBuilder.Entity<PreferenciaDashboard>(entity =>
        {
            entity.ToTable("Preferencias_Dashboard");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("PDB_Id");
            entity.Property(p => p.UsuarioId).HasColumnName("PDB_Usuario_Id").IsRequired();
            entity.Property(p => p.LayoutJson).HasColumnName("PDB_Layout_Json").IsRequired();
            entity.Property(p => p.AtualizadoEm).HasColumnName("PDB_Atualizado_Em").IsRequired();

            entity.HasIndex(p => p.UsuarioId).IsUnique();
        });

        modelBuilder.Entity<ContratoFinanceiro>(entity =>
        {
            entity.ToTable("Contratos_Financeiros");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("CTF_Id");
            entity.Property(c => c.ProjetoId).HasColumnName("CTF_Projeto_Id").IsRequired();
            entity.Property(c => c.ValorTotal).HasColumnName("CTF_Valor_Total").HasPrecision(18, 2).IsRequired();
            entity.Property(c => c.CondicoesPagamento).HasColumnName("CTF_Condicoes_Pagamento").HasMaxLength(500);
            entity.Property(c => c.Observacoes).HasColumnName("CTF_Observacoes");
            entity.Property(c => c.CriadoEm).HasColumnName("CTF_Criado_Em").IsRequired();
            entity.Property(c => c.AtualizadoEm).HasColumnName("CTF_Atualizado_Em");

            entity.HasOne(c => c.Projeto)
                  .WithMany()
                  .HasForeignKey(c => c.ProjetoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.Parcelas)
                  .WithOne(p => p.ContratoFinanceiro)
                  .HasForeignKey(p => p.ContratoFinanceiroId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ParcelaFinanceira>(entity =>
        {
            entity.ToTable("Parcelas_Financeiras");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("PAR_Id");
            entity.Property(p => p.ProjetoId).HasColumnName("PAR_Projeto_Id").IsRequired();
            entity.Property(p => p.ContratoFinanceiroId).HasColumnName("PAR_Contrato_Id");
            entity.Property(p => p.NumeroParcela).HasColumnName("PAR_Numero_Parcela").IsRequired();
            entity.Property(p => p.TotalParcelas).HasColumnName("PAR_Total_Parcelas").IsRequired();
            entity.Property(p => p.Descricao).HasColumnName("PAR_Descricao").IsRequired().HasMaxLength(200);
            entity.Property(p => p.Valor).HasColumnName("PAR_Valor").HasPrecision(18, 2).IsRequired();
            entity.Property(p => p.DataVencimento).HasColumnName("PAR_Data_Vencimento").IsRequired();
            entity.Property(p => p.DataPagamento).HasColumnName("PAR_Data_Pagamento");
            entity.Property(p => p.Status).HasColumnName("PAR_Status").IsRequired();
            entity.Property(p => p.FormaPagamento).HasColumnName("PAR_Forma_Pagamento");
            entity.Property(p => p.Observacoes).HasColumnName("PAR_Observacoes");
            entity.Property(p => p.ComprovanteUrl).HasColumnName("PAR_Comprovante_Url").HasMaxLength(1000);
            entity.Property(p => p.CriadoEm).HasColumnName("PAR_Criado_Em").IsRequired();
            entity.Property(p => p.AtualizadoEm).HasColumnName("PAR_Atualizado_Em");

            entity.HasIndex(p => p.ProjetoId);
            entity.HasIndex(p => p.DataVencimento);
            entity.HasIndex(p => p.Status);

            entity.HasOne(p => p.Projeto)
                  .WithMany()
                  .HasForeignKey(p => p.ProjetoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DespesaProjeto>(entity =>
        {
            entity.ToTable("Despesas_Projetos");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).HasColumnName("DSP_Id");
            entity.Property(d => d.ProjetoId).HasColumnName("DSP_Projeto_Id").IsRequired(false);
            entity.Property(d => d.Descricao).HasColumnName("DSP_Descricao").IsRequired().HasMaxLength(200);
            entity.Property(d => d.Valor).HasColumnName("DSP_Valor").HasPrecision(18, 2).IsRequired();
            entity.Property(d => d.DataDespesa).HasColumnName("DSP_Data_Despesa").IsRequired();
            entity.Property(d => d.Categoria).HasColumnName("DSP_Categoria").IsRequired();
            entity.Property(d => d.Observacoes).HasColumnName("DSP_Observacoes");
            entity.Property(d => d.ComprovanteUrl).HasColumnName("DSP_Comprovante_Url").HasMaxLength(1000);
            entity.Property(d => d.CriadoEm).HasColumnName("DSP_Criado_Em").IsRequired();
            entity.Property(d => d.AtualizadoEm).HasColumnName("DSP_Atualizado_Em");

            entity.HasIndex(d => d.ProjetoId);
            entity.HasIndex(d => d.DataDespesa);

            entity.HasOne(d => d.Projeto)
                  .WithMany()
                  .HasForeignKey(d => d.ProjetoId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ConfiguracaoProposta>(entity =>
        {
            entity.ToTable("Configuracoes_Proposta");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("CFP_Id");
            entity.Property(c => c.UsuarioId).HasColumnName("CFP_Usuario_Id");
            entity.Property(c => c.NomeEscritorio).HasColumnName("CFP_Nome_Escritorio").HasMaxLength(200);
            entity.Property(c => c.Slogan).HasColumnName("CFP_Slogan").HasMaxLength(300);
            entity.Property(c => c.RegistroProfissional).HasColumnName("CFP_Registro_Profissional").HasMaxLength(150);
            entity.Property(c => c.Email).HasColumnName("CFP_Email").HasMaxLength(150);
            entity.Property(c => c.Telefone).HasColumnName("CFP_Telefone").HasMaxLength(50);
            entity.Property(c => c.Endereco).HasColumnName("CFP_Endereco").HasMaxLength(300);
            entity.Property(c => c.LogoUrl).HasColumnName("CFP_Logo_Url");
            entity.Property(c => c.CorPrimaria).HasColumnName("CFP_Cor_Primaria").HasMaxLength(20);
            entity.Property(c => c.ExibirCabecalho).HasColumnName("CFP_Exibir_Cabecalho");
            entity.Property(c => c.ExibirResumo).HasColumnName("CFP_Exibir_Resumo");
            entity.Property(c => c.ExibirTabelaEtapas).HasColumnName("CFP_Exibir_Tabela_Etapas");
            entity.Property(c => c.ExibirMemoriaCalculo).HasColumnName("CFP_Exibir_Memoria_Calculo");
            entity.Property(c => c.ExibirCondicoesPagamento).HasColumnName("CFP_Exibir_Condicoes_Pagamento");
            entity.Property(c => c.ExibirTermosGerais).HasColumnName("CFP_Exibir_Termos_Gerais");
            entity.Property(c => c.ExibirAssinaturas).HasColumnName("CFP_Exibir_Assinaturas");
            entity.Property(c => c.TextoApresentacao).HasColumnName("CFP_Texto_Apresentacao");
            entity.Property(c => c.ValidadeDias).HasColumnName("CFP_Validade_Dias");
            entity.Property(c => c.CondicoesPagamentoPadrao).HasColumnName("CFP_Condicoes_Pagamento_Padrao");
            entity.Property(c => c.ChavePix).HasColumnName("CFP_Chave_Pix").HasMaxLength(100);
            entity.Property(c => c.DadosBancarios).HasColumnName("CFP_Dados_Bancarios").HasMaxLength(300);
            entity.Property(c => c.TermosGerais).HasColumnName("CFP_Termos_Gerais");
            entity.Property(c => c.TemplateMensagemWhatsapp).HasColumnName("CFP_Template_Mensagem_Whatsapp");
            entity.Property(c => c.Configurado).HasColumnName("CFP_Configurado");
            entity.Property(c => c.AtualizadoEm).HasColumnName("CFP_Atualizado_Em");

            entity.HasIndex(c => c.UsuarioId);
        });

        modelBuilder.Entity<ArchiFlow.Domain.Agenda.Compromisso>(entity =>
        {
            entity.ToTable("Compromissos_Agenda");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("CMP_Id");
            entity.Property(c => c.EscritorioId).HasColumnName("CMP_Escritorio_Id").IsRequired();
            entity.Property(c => c.UsuarioId).HasColumnName("CMP_Usuario_Id");
            entity.Property(c => c.ProjetoId).HasColumnName("CMP_Projeto_Id");
            entity.Property(c => c.ClienteId).HasColumnName("CMP_Cliente_Id");
            entity.Property(c => c.LeadId).HasColumnName("CMP_Lead_Id");
            entity.Property(c => c.Titulo).HasColumnName("CMP_Titulo").IsRequired().HasMaxLength(200);
            entity.Property(c => c.Descricao).HasColumnName("CMP_Descricao");
            entity.Property(c => c.Tipo).HasColumnName("CMP_Tipo").IsRequired().HasMaxLength(50);
            entity.Property(c => c.Status).HasColumnName("CMP_Status").IsRequired().HasMaxLength(50);
            entity.Property(c => c.DataHoraInicio).HasColumnName("CMP_Data_Hora_Inicio").IsRequired();
            entity.Property(c => c.DataHoraFim).HasColumnName("CMP_Data_Hora_Fim").IsRequired();
            entity.Property(c => c.Local).HasColumnName("CMP_Local").HasMaxLength(300);
            entity.Property(c => c.LinkGoogleMeet).HasColumnName("CMP_Link_Google_Meet").HasMaxLength(500);
            entity.Property(c => c.GoogleEventId).HasColumnName("CMP_Google_Event_Id").HasMaxLength(200);
            entity.Property(c => c.CriadoEm).HasColumnName("CMP_Criado_Em").IsRequired();
            entity.Property(c => c.AtualizadoEm).HasColumnName("CMP_Atualizado_Em");

            entity.HasIndex(c => c.EscritorioId);
            entity.HasIndex(c => new { c.EscritorioId, c.DataHoraInicio });
        });

        modelBuilder.Entity<ArchiFlow.Domain.Agenda.ConfiguracaoAgendaEscritorio>(entity =>
        {
            entity.ToTable("Configuracoes_Agenda_Escritorio");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("CAE_Id");
            entity.Property(c => c.EscritorioId).HasColumnName("CAE_Escritorio_Id").IsRequired();
            entity.Property(c => c.EmailAgendaEmpresa).HasColumnName("CAE_Email_Agenda_Empresa").IsRequired().HasMaxLength(256);
            entity.Property(c => c.GoogleCalendarId).HasColumnName("CAE_Google_Calendar_Id").HasMaxLength(300);
            entity.Property(c => c.ChaveGoogleServiceAccountJson).HasColumnName("CAE_Chave_Service_Account_Json");
            entity.Property(c => c.GoogleOAuthRefreshToken).HasColumnName("CAE_Google_OAuth_Refresh_Token");
            entity.Property(c => c.GoogleOAuthEmail).HasColumnName("CAE_Google_OAuth_Email").HasMaxLength(256);
            entity.Property(c => c.GoogleClientId).HasColumnName("CAE_Google_Client_Id").HasMaxLength(300);
            entity.Property(c => c.GoogleClientSecret).HasColumnName("CAE_Google_Client_Secret").HasMaxLength(300);
            entity.Property(c => c.TipoIntegracao).HasColumnName("CAE_Tipo_Integracao").HasMaxLength(50);
            entity.Property(c => c.NomeAgenda).HasColumnName("CAE_Nome_Agenda").HasMaxLength(200);
            entity.Property(c => c.SincronizacaoAutomaticaAtiva).HasColumnName("CAE_Sincronizacao_Ativa");
            entity.Property(c => c.ConectadoEm).HasColumnName("CAE_Conectado_Em");
            entity.Property(c => c.AtualizadoEm).HasColumnName("CAE_Atualizado_Em");

            entity.HasIndex(c => c.EscritorioId).IsUnique();
        });

        modelBuilder.Entity<Fornecedor>(entity =>
        {
            entity.ToTable("Fornecedores");
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Nome).IsRequired().HasMaxLength(200);
            entity.Property(f => f.Especialidade).IsRequired().HasMaxLength(100);
            entity.Property(f => f.Email).HasMaxLength(200);
            entity.Property(f => f.Telefone).HasMaxLength(50);
            entity.Property(f => f.Cidade).HasMaxLength(100);
            entity.Property(f => f.Estado).HasMaxLength(50);
            entity.Property(f => f.AvaliacaoMedia).HasPrecision(3, 1);
            entity.HasMany(f => f.Avaliacoes).WithOne(a => a.Fornecedor).HasForeignKey(a => a.FornecedorId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(f => f.ProjetosVinculados).WithOne(p => p.Fornecedor).HasForeignKey(p => p.FornecedorId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AvaliacaoFornecedor>(entity =>
        {
            entity.ToTable("Avaliacoes_Fornecedores");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Comentario).HasMaxLength(1000);
            entity.Property(a => a.AutorNome).HasMaxLength(150);
        });

        modelBuilder.Entity<ProjetoFornecedor>(entity =>
        {
            entity.ToTable("Projetos_Fornecedores");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.FuncaoNoProjeto).HasMaxLength(150);
            entity.HasOne(p => p.Projeto)
                  .WithMany()
                  .HasForeignKey(p => p.ProjetoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OpcaoConfiguracao>(entity =>
        {
            entity.ToTable("Opcoes_Configuracao");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Categoria).IsRequired().HasMaxLength(100);
            entity.Property(o => o.Chave).IsRequired().HasMaxLength(100);
            entity.Property(o => o.Rotulo).IsRequired().HasMaxLength(200);
            entity.HasIndex(o => new { o.Categoria, o.Chave }).IsUnique();
        });
    }
}
