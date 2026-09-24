using Assistencia.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tecnico> Tecnicos => Set<Tecnico>();
    public DbSet<Especialidade> Especialidades => Set<Especialidade>();
    public DbSet<TecnicoEspecialidade> TecnicosEspecialidades
        => Set<TecnicoEspecialidade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
    }
}