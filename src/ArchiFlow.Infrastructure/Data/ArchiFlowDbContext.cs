using ArchiFlow.Domain.Projetos;
using Microsoft.EntityFrameworkCore;

namespace ArchiFlow.Infrastructure.Data;

public class ArchiFlowDbContext : DbContext
{
    public ArchiFlowDbContext(DbContextOptions<ArchiFlowDbContext> options) : base(options) { }

    public DbSet<Projeto> Projetos => Set<Projeto>();
    public DbSet<EtapaProjeto> EtapasProjeto => Set<EtapaProjeto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EtapaProjeto>()
            .HasOne(e => e.Projeto)
            .WithMany(p => p.Etapas)
            .HasForeignKey(e => e.ProjetoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
