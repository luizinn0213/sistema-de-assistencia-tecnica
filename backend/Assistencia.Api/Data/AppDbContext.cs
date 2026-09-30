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

    public DbSet<TecnicoEspecialidade> TecnicosEspecialidades
        => Set<TecnicoEspecialidade>();

    public DbSet<Usuario> Usuarios
        => Set<Usuario>();

    public DbSet<Cliente> Clientes
        => Set<Cliente>();

    public DbSet<Endereco> Enderecos
        => Set<Endereco>();

    public DbSet<Equipamento> Equipamentos
        => Set<Equipamento>();

    public DbSet<Solicitacao> Solicitacoes
        => Set<Solicitacao>();

    public DbSet<SelecaoTecnico> SelecoesTecnicos
        => Set<SelecaoTecnico>();

    public DbSet<HistoricoSelecaoTecnico> HistoricosSelecoesTecnicos
        => Set<HistoricoSelecaoTecnico>();

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

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.CpfCnpj)
            .IsUnique();

        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Usuario)
            .WithOne()
            .HasForeignKey<Cliente>(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Endereco)
            .WithOne(e => e.Cliente)
            .HasForeignKey<Endereco>(e => e.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Cliente>()
            .HasMany(c => c.Equipamentos)
            .WithOne(e => e.Cliente)
            .HasForeignKey(e => e.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Equipamento>()
            .HasIndex(e => e.NumeroSerie)
            .IsUnique();

        modelBuilder.Entity<Solicitacao>()
            .HasIndex(s => s.Numero)
            .IsUnique();

        modelBuilder.Entity<Solicitacao>()
            .HasOne(s => s.Cliente)
            .WithMany()
            .HasForeignKey(s => s.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Solicitacao>()
            .HasOne(s => s.Equipamento)
            .WithMany()
            .HasForeignKey(s => s.EquipamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Solicitacao>()
            .HasOne(s => s.Especialidade)
            .WithMany()
            .HasForeignKey(s => s.EspecialidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SelecaoTecnico>()
            .HasOne(s => s.Solicitacao)
            .WithMany()
            .HasForeignKey(s => s.SolicitacaoId)
            .OnDelete(DeleteBehavior.Restrict);
            
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

        modelBuilder.Entity<HistoricoSelecaoTecnico>()
            .HasOne(h => h.SelecaoTecnico)
            .WithMany(s => s.Historico)
            .HasForeignKey(h => h.SelecaoTecnicoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HistoricoSelecaoTecnico>()
            .HasIndex(h => new
            {
                h.SelecaoTecnicoId,
                h.DataRegistro
            });
    }
}