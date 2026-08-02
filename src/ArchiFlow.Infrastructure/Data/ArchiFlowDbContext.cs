using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Usuarios;
using ArchiFlow.Domain.Clientes;
using Microsoft.EntityFrameworkCore;

namespace ArchiFlow.Infrastructure.Data;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class ArchiFlowDbContext : DbContext
{
    public ArchiFlowDbContext(DbContextOptions<ArchiFlowDbContext> options) : base(options) { }

    public DbSet<Projeto>      Projetos      => Set<Projeto>();
    public DbSet<EtapaProjeto> EtapasProjeto => Set<EtapaProjeto>();
    public DbSet<Usuario>      Usuarios      => Set<Usuario>();
    public DbSet<Cliente>      Clientes      => Set<Cliente>();

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

            entity.HasIndex(c => c.Email).IsUnique();
        });
    }
}
