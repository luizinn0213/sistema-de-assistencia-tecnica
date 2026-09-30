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

    public DbSet<SelecaoTecnico> SelecoesTecnicos
        => Set<SelecaoTecnico>();

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

        modelBuilder.Entity<SelecaoTecnico>()
            .HasOne(s => s.Tecnico)
            .WithMany()
            .HasForeignKey(s => s.TecnicoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SelecaoTecnico>()
            .HasOne(s => s.Especialidade)
            .WithMany()
            .HasForeignKey(s => s.EspecialidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SelecaoTecnico>()
            .HasIndex(s => new
            {
                s.SolicitacaoId,
                s.Status
            });
    }
}