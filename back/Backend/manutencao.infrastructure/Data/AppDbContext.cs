using Manutencao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Manutencao.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Solicitacao> Solicitacoes => Set<Solicitacao>();
    public DbSet<Tecnico> Tecnicos => Set<Tecnico>();
    public DbSet<HistoricoSolicitacao> Historicos => Set<HistoricoSolicitacao>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<HistoricoSolicitacao>()
            .HasIndex(h => new { h.SolicitacaoId, h.DataRegistro });

        builder.Entity<Solicitacao>()
            .HasOne(s => s.Tecnico)
            .WithMany(t => t.Solicitacoes)
            .HasForeignKey(s => s.TecnicoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}