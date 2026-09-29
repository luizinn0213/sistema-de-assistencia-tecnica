using Microsoft.EntityFrameworkCore;

namespace Api;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<Equipamento> Equipamentos => Set<Equipamento>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        mb.Entity<Cliente>().HasIndex(c => c.CpfCnpj).IsUnique();
        mb.Entity<Equipamento>().HasIndex(e => e.NumeroSerie).IsUnique();
    }
}