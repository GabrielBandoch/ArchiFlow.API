using ArchiFlow.Domain.Projetos;
using Microsoft.EntityFrameworkCore;

namespace ArchiFlow.Infrastructure.Data;

public class ArchiFlowDbContext : DbContext
{
    public ArchiFlowDbContext(DbContextOptions<ArchiFlowDbContext> options) : base(options) { }

    public DbSet<Projeto>      Projetos      => Set<Projeto>();
    public DbSet<EtapaProjeto> EtapasProjeto => Set<EtapaProjeto>();

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
    }
}
