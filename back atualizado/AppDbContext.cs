using Microsoft.EntityFrameworkCore;

namespace Api;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<Equipamento> Equipamentos => Set<Equipamento>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ---------------- Usuário ----------------
        mb.Entity<Usuario>(e =>
        {
            e.Property(u => u.Nome).HasMaxLength(120).IsRequired();
            e.Property(u => u.Email).HasMaxLength(160).IsRequired();
            e.Property(u => u.SenhaHash).HasMaxLength(100).IsRequired();
            e.Property(u => u.Telefone).HasMaxLength(20);

            // Grava "Cliente"/"Tecnico"/"Administrador" no lugar de 1/2/3:
            // o banco fica legível e não quebra se a ordem do enum mudar.
            e.Property(u => u.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();

            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Tipo);

            e.HasOne(u => u.Cliente)
                .WithOne(c => c.Usuario)
                .HasForeignKey<Cliente>(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- Cliente ----------------
        mb.Entity<Cliente>(e =>
        {
            e.Property(c => c.CpfCnpj).HasMaxLength(14).IsRequired();
            e.HasIndex(c => c.CpfCnpj).IsUnique();
            e.HasIndex(c => c.UsuarioId).IsUnique();

            e.HasOne(c => c.Endereco)
                .WithOne()
                .HasForeignKey<Endereco>(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- Endereço ----------------
        mb.Entity<Endereco>(e =>
        {
            e.Property(x => x.Cep).HasMaxLength(8).IsRequired();
            e.Property(x => x.Logradouro).HasMaxLength(120).IsRequired();
            e.Property(x => x.Numero).HasMaxLength(10).IsRequired();
            e.Property(x => x.Bairro).HasMaxLength(80).IsRequired();
            e.Property(x => x.Cidade).HasMaxLength(80).IsRequired();
            e.Property(x => x.Estado).HasMaxLength(2).IsRequired();
        });

        // ---------------- Equipamento ----------------
        mb.Entity<Equipamento>(e =>
        {
            e.Property(x => x.Tipo).HasMaxLength(40).IsRequired();
            e.Property(x => x.Marca).HasMaxLength(60).IsRequired();
            e.Property(x => x.Modelo).HasMaxLength(60).IsRequired();
            e.Property(x => x.NumeroSerie).HasMaxLength(60).IsRequired();

            e.HasIndex(x => x.NumeroSerie).IsUnique();
            e.HasIndex(x => x.ClienteId);

            e.HasOne(x => x.Cliente)
                .WithMany(c => c.Equipamentos)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- Refresh token ----------------
        mb.Entity<RefreshToken>(e =>
        {
            e.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
            e.Property(t => t.SubstituidoPor).HasMaxLength(64);
            e.Property(t => t.CriadoPorIp).HasMaxLength(45);

            e.HasIndex(t => t.TokenHash).IsUnique();
            e.HasIndex(t => t.UsuarioId);

            // Propriedade calculada: nunca vira coluna.
            e.Ignore(t => t.Ativo);

            e.HasOne(t => t.Usuario)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
