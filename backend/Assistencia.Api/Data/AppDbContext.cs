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

    public DbSet<Diagnostico> Diagnosticos
        => Set<Diagnostico>();

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

    public DbSet<Orcamento> Orcamentos
    => Set<Orcamento>();

    public DbSet<ItemOrcamento> ItensOrcamento
        => Set<ItemOrcamento>();

    public DbSet<HistoricoOrcamento> HistoricosOrcamento
        => Set<HistoricoOrcamento>();
        
    public DbSet<ExecucaoServico> ExecucoesServicos
        => Set<ExecucaoServico>();

    public DbSet<HistoricoExecucaoServico>
        HistoricosExecucoesServicos
        => Set<HistoricoExecucaoServico>();

    public DbSet<Avaliacao> Avaliacoes
        => Set<Avaliacao>();

    public DbSet<RefreshToken> RefreshTokens
        => Set<RefreshToken>();

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

        modelBuilder.Entity<Diagnostico>()
            .HasOne(d => d.Solicitacao)
            .WithMany()
            .HasForeignKey(d => d.SolicitacaoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Diagnostico>()
            .HasOne(d => d.Tecnico)
            .WithMany()
            .HasForeignKey(d => d.TecnicoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Diagnostico>()
            .HasIndex(d => d.SolicitacaoId)
            .IsUnique();

        modelBuilder.Entity<Diagnostico>()
            .Property(d => d.DescricaoDefeito)
            .HasMaxLength(Diagnostico.TamanhoMaximoDescricao)
            .IsRequired();

        modelBuilder.Entity<Diagnostico>()
            .Property(d => d.SolucaoRecomendada)
            .HasMaxLength(Diagnostico.TamanhoMaximoDescricao)
            .IsRequired();

        modelBuilder.Entity<Orcamento>()
            .HasOne(o => o.Diagnostico)
            .WithOne()
            .HasForeignKey<Orcamento>(
                o => o.DiagnosticoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Orcamento>()
            .HasIndex(o => o.DiagnosticoId)
            .IsUnique();

        modelBuilder.Entity<Orcamento>()
            .HasMany(o => o.Itens)
            .WithOne(i => i.Orcamento)
            .HasForeignKey(i => i.OrcamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Orcamento>()
            .Navigation(o => o.Itens)
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);

        modelBuilder.Entity<Orcamento>()
            .HasMany(o => o.Historico)
            .WithOne(h => h.Orcamento)
            .HasForeignKey(h => h.OrcamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Orcamento>()
            .Navigation(o => o.Historico)
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);

        modelBuilder.Entity<Orcamento>()
            .Ignore(o => o.ValorTotal);

        modelBuilder.Entity<ItemOrcamento>()
            .Property(i => i.Descricao)
            .HasMaxLength(
                ItemOrcamento.TamanhoMaximoDescricao)
            .IsRequired();

        modelBuilder.Entity<ItemOrcamento>()
            .Property(i => i.ValorUnitario)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ItemOrcamento>()
            .Ignore(i => i.ValorTotal);

        modelBuilder.Entity<HistoricoOrcamento>()
            .Property(h => h.Observacao)
            .HasMaxLength(
                HistoricoOrcamento
                    .TamanhoMaximoObservacao)
            .IsRequired();

        modelBuilder.Entity<HistoricoOrcamento>()
            .HasIndex(h => new
            {
                h.OrcamentoId,
                h.DataRegistro
            });
            
        modelBuilder.Entity<ExecucaoServico>()
            .HasOne(e => e.Orcamento)
            .WithOne()
            .HasForeignKey<ExecucaoServico>(
                e => e.OrcamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExecucaoServico>()
            .HasIndex(e => e.OrcamentoId)
            .IsUnique();

        modelBuilder.Entity<ExecucaoServico>()
            .HasMany(e => e.Historico)
            .WithOne(h => h.ExecucaoServico)
            .HasForeignKey(h => h.ExecucaoServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExecucaoServico>()
            .Navigation(e => e.Historico)
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);

        modelBuilder.Entity<ExecucaoServico>()
            .Property(e => e.ObservacaoInicio)
            .HasMaxLength(
                ExecucaoServico.TamanhoMaximoObservacao);

        modelBuilder.Entity<ExecucaoServico>()
            .Property(e => e.ObservacaoFinal)
            .HasMaxLength(
                ExecucaoServico.TamanhoMaximoObservacao);

        modelBuilder.Entity<HistoricoExecucaoServico>()
            .Property(h => h.Observacao)
            .HasMaxLength(
                HistoricoExecucaoServico
                    .TamanhoMaximoObservacao)
            .IsRequired();

        modelBuilder.Entity<HistoricoExecucaoServico>()
            .HasIndex(h => new
            {
                h.ExecucaoServicoId,
                h.DataRegistro
            });

        modelBuilder.Entity<Avaliacao>()
            .HasOne(a => a.ExecucaoServico)
            .WithOne()
            .HasForeignKey<Avaliacao>(
                a => a.ExecucaoServicoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Avaliacao>()
            .HasIndex(a => a.ExecucaoServicoId)
            .IsUnique();

        modelBuilder.Entity<Avaliacao>()
            .HasOne(a => a.Cliente)
            .WithMany()
            .HasForeignKey(a => a.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Avaliacao>()
            .Property(a => a.Comentario)
            .HasMaxLength(
                Avaliacao.TamanhoMaximoComentario);

        var refreshToken = modelBuilder.Entity<RefreshToken>();

        refreshToken.HasOne(t => t.Usuario)
            .WithMany()
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        refreshToken.HasIndex(t => t.TokenHash).IsUnique();
        refreshToken.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
        refreshToken.Property(t => t.SubstituidoPorHash).HasMaxLength(64);
        refreshToken.Ignore(t => t.Ativo);

    }
}