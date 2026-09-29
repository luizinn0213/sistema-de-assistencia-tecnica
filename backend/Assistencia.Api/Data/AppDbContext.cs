using Assistencia.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tecnico> Tecnicos
        => Set<Tecnico>();

    public DbSet<Especialidade> Especialidades
        => Set<Especialidade>();

    public DbSet<TecnicoEspecialidade>
        TecnicosEspecialidades
        => Set<TecnicoEspecialidade>();

    public DbSet<Orcamento> Orcamentos
        => Set<Orcamento>();

    public DbSet<ItemOrcamento> ItensOrcamento
        => Set<ItemOrcamento>();

    public DbSet<HistoricoOrcamento>
        HistoricosOrcamento
        => Set<HistoricoOrcamento>();

    public DbSet<Pagamento> Pagamentos
        => Set<Pagamento>();

    public DbSet<Comissao> Comissoes
        => Set<Comissao>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TecnicoEspecialidade>()
            .HasKey(te => new
            {
                te.TecnicoId,
                te.EspecialidadeId
            });

        modelBuilder.Entity<TecnicoEspecialidade>()
            .HasOne(te => te.Tecnico)
            .WithMany(t => t.TecnicoEspecialidades)
            .HasForeignKey(te => te.TecnicoId);

        modelBuilder.Entity<TecnicoEspecialidade>()
            .HasOne(te => te.Especialidade)
            .WithMany(e => e.TecnicoEspecialidades)
            .HasForeignKey(te => te.EspecialidadeId);

        modelBuilder.Entity<Orcamento>()
            .HasMany(o => o.Itens)
            .WithOne(i => i.Orcamento)
            .HasForeignKey(i => i.OrcamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Orcamento>()
            .HasMany(o => o.Historico)
            .WithOne(h => h.Orcamento)
            .HasForeignKey(h => h.OrcamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Orcamento>()
            .HasOne(o => o.Pagamento)
            .WithOne(p => p.Orcamento)
            .HasForeignKey<Pagamento>(
                p => p.OrcamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Orcamento>()
            .HasOne(o => o.Comissao)
            .WithOne(c => c.Orcamento)
            .HasForeignKey<Comissao>(
                c => c.OrcamentoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}