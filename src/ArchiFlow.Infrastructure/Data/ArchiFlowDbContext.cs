using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Usuarios;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Chat;
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
    public DbSet<MensagemChat>          MensagensChat         => Set<MensagemChat>();

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
            entity.Property(u => u.Nome).HasColumnName("USR_Nome").IsRequired().HasMaxLength(200);
            entity.Property(u => u.Email).HasColumnName("USR_Email").IsRequired().HasMaxLength(256);
            entity.Property(u => u.SenhaHash).HasColumnName("USR_Senha_Hash").IsRequired();
            entity.Property(u => u.Role).HasColumnName("USR_Role").IsRequired().HasMaxLength(50);
            entity.Property(u => u.Ativo).HasColumnName("USR_Ativo");
            entity.Property(u => u.CriadoEm).HasColumnName("USR_Criado_Em");
            entity.Property(u => u.AtualizadoEm).HasColumnName("USR_Atualizado_Em");

            entity.HasIndex(u => u.Email).IsUnique();
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
    }
}
