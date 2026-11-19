using Assistencia.Api.Data;
using Assistencia.Api.Dtos.Matching;
using Assistencia.Api.Enums;
using Assistencia.Api.Models;
using Assistencia.Api.Repositories;
using Assistencia.Api.Services.Contracts;
using Assistencia.Api.Services.Matching;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Tests;

public class MatchingRankingTests : IDisposable
{
    private const string Cidade = "Teresópolis";
    private const int SolicitacaoId = 1;

    private readonly SqliteConnection _conexao;
    private readonly AppDbContext _context;
    private int _proximoUsuarioId = 1;

    public MatchingRankingTests()
    {
        // Foreign Keys=False: o ranking lê apenas técnicos, especialidades
        // e seleções. Sem as chaves estrangeiras, o teste não precisa
        // montar usuário, cliente, equipamento e solicitação.
        _conexao = new SqliteConnection(
            "DataSource=:memory:;Foreign Keys=False");
        _conexao.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_conexao)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _conexao.Dispose();
    }

    // 1. Especialidade adiciona 50 pontos
    [Fact]
    public void CriterioEspecialidade_DeveAdicionar50Pontos()
    {
        var criterio = new CriterioEspecialidade();
        var contexto = new ContextoPontuacaoMatching
        {
            PossuiEspecialidade = true
        };

        var resultado = criterio.Calcular(contexto);

        Assert.Equal(50, resultado.Pontos);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Destaque));
    }

    // 2. Mesma cidade adiciona 30 pontos
    [Fact]
    public void CriterioLocalizacao_DeveAdicionar30PontosNaMesmaCidade()
    {
        var criterio = new CriterioLocalizacao();
        var contexto = new ContextoPontuacaoMatching
        {
            MesmaCidade = true
        };

        var resultado = criterio.Calcular(contexto);

        Assert.Equal(30, resultado.Pontos);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Destaque));
    }

    // 3. Experiência usa nível (10 x valor do enum) e anos (até 10)
    [Theory]
    [InlineData(NivelExperiencia.Iniciante, 0, 0)]
    [InlineData(NivelExperiencia.Intermediario, 3, 13)]
    [InlineData(NivelExperiencia.Avancado, 15, 30)]
    [InlineData(NivelExperiencia.Especialista, 10, 40)]
    public void CriterioExperiencia_DeveUsarNivelEAnos(
        NivelExperiencia nivel,
        int anos,
        int pontosEsperados)
    {
        var criterio = new CriterioExperiencia();
        var contexto = new ContextoPontuacaoMatching
        {
            NivelExperiencia = nivel,
            AnosExperiencia = anos
        };

        var resultado = criterio.Calcular(contexto);

        Assert.Equal(pontosEsperados, resultado.Pontos);
    }

    // 4. Serviço soma todos os critérios
    [Fact]
    public async Task MatchingService_DeveSomarTodosOsCriterios()
    {
        var especialidade = await CriarEspecialidadeAsync("Redes");
        await CriarTecnicoAsync(
            "Amanda Costa", Cidade, especialidade,
            NivelExperiencia.Intermediario, 5);

        var ranking = await CriarServico()
            .GerarRankingAsync(especialidade.Id, Cidade);

        var tecnico = Assert.Single(ranking);
        // 50 (especialidade) + 10 (Intermediario) + 5 (anos) + 30 (cidade)
        Assert.Equal(95, tecnico.Pontuacao);
        Assert.Equal(3, tecnico.Destaques.Count);
        Assert.True(tecnico.MesmaCidade);
    }

    // 5. Ranking fica em ordem decrescente de pontuação
    [Fact]
    public async Task Ranking_DeveFicarEmOrdemDecrescenteDePontuacao()
    {
        var especialidade = await CriarEspecialidadeAsync("Redes");
        // 50 + 0 + 1 + 30 = 81
        await CriarTecnicoAsync(
            "Ana Lima", Cidade, especialidade,
            NivelExperiencia.Iniciante, 1);
        // 50 + 30 + 10 + 0 = 90
        await CriarTecnicoAsync(
            "Bruno Silva", "Petrópolis", especialidade,
            NivelExperiencia.Especialista, 10);
        // 50 + 10 + 5 + 30 = 95
        await CriarTecnicoAsync(
            "Carlos Souza", Cidade, especialidade,
            NivelExperiencia.Intermediario, 5);

        var ranking = await CriarServico()
            .GerarRankingAsync(especialidade.Id, Cidade);

        Assert.Equal(
            new[] { "Carlos Souza", "Bruno Silva", "Ana Lima" },
            ranking.Select(t => t.NomeExibicao));
        Assert.Equal(
            new[] { 95, 90, 81 },
            ranking.Select(t => t.Pontuacao));
    }

    // 6. Empate é ordenado pelo nome
    [Fact]
    public async Task Ranking_DeveDesempatarPeloNome()
    {
        var especialidade = await CriarEspecialidadeAsync("Redes");
        await CriarTecnicoAsync(
            "Bruno Silva", Cidade, especialidade,
            NivelExperiencia.Intermediario, 5);
        await CriarTecnicoAsync(
            "Amanda Costa", Cidade, especialidade,
            NivelExperiencia.Intermediario, 5);

        var ranking = await CriarServico()
            .GerarRankingAsync(especialidade.Id, Cidade);

        Assert.Equal(2, ranking.Count);
        Assert.Equal(ranking[0].Pontuacao, ranking[1].Pontuacao);
        Assert.Equal("Amanda Costa", ranking[0].NomeExibicao);
        Assert.Equal("Bruno Silva", ranking[1].NomeExibicao);
    }

    // 7. Técnico que recusou a solicitação não aparece novamente
    [Fact]
    public async Task Ranking_NaoDeveMostrarTecnicoQueRecusouASolicitacao()
    {
        var especialidade = await CriarEspecialidadeAsync("Hardware");
        await CriarTecnicoAsync(
            "Técnico Válido", Cidade, especialidade,
            NivelExperiencia.Intermediario, 3);
        var recusou = await CriarTecnicoAsync(
            "Técnico Recusou", Cidade, especialidade,
            NivelExperiencia.Especialista, 8);

        var selecao = new SelecaoTecnico(
            SolicitacaoId, recusou, especialidade);
        selecao.Recusar("Sem agenda para o atendimento.");
        _context.SelecoesTecnicos.Add(selecao);
        await _context.SaveChangesAsync();

        var servico = CriarServico();
        var rankingDaSolicitacao = await servico.GerarRankingAsync(
            especialidade.Id, Cidade, SolicitacaoId);
        var rankingDeOutraSolicitacao = await servico.GerarRankingAsync(
            especialidade.Id, Cidade, SolicitacaoId + 1);

        var tecnico = Assert.Single(rankingDaSolicitacao);
        Assert.Equal("Técnico Válido", tecnico.NomeExibicao);
        // A recusa vale apenas para a solicitação recusada.
        Assert.Equal(2, rankingDeOutraSolicitacao.Count);
    }

    // 8. Técnico inativo não aparece
    [Fact]
    public async Task Ranking_NaoDeveMostrarTecnicoInativo()
    {
        var especialidade = await CriarEspecialidadeAsync("Hardware");
        await CriarTecnicoAsync(
            "Técnico Válido", Cidade, especialidade,
            NivelExperiencia.Intermediario, 3);
        await CriarTecnicoAsync(
            "Técnico Inativo", Cidade, especialidade,
            NivelExperiencia.Avancado, 4,
            ativo: false);

        var ranking = await CriarServico()
            .GerarRankingAsync(especialidade.Id, Cidade);

        var tecnico = Assert.Single(ranking);
        Assert.Equal("Técnico Válido", tecnico.NomeExibicao);
    }

    // 9. Técnico indisponível não aparece
    [Fact]
    public async Task Ranking_NaoDeveMostrarTecnicoIndisponivel()
    {
        var especialidade = await CriarEspecialidadeAsync("Hardware");
        await CriarTecnicoAsync(
            "Técnico Válido", Cidade, especialidade,
            NivelExperiencia.Intermediario, 3);
        await CriarTecnicoAsync(
            "Técnico Indisponível", Cidade, especialidade,
            NivelExperiencia.Avancado, 4,
            disponivel: false);

        var ranking = await CriarServico()
            .GerarRankingAsync(especialidade.Id, Cidade);

        var tecnico = Assert.Single(ranking);
        Assert.Equal("Técnico Válido", tecnico.NomeExibicao);
    }

    // 10. Técnico sem a especialidade não aparece
    [Fact]
    public async Task Ranking_NaoDeveMostrarTecnicoSemAEspecialidade()
    {
        var especialidade = await CriarEspecialidadeAsync("Hardware");
        var outraEspecialidade = await CriarEspecialidadeAsync("Software");
        await CriarTecnicoAsync(
            "Técnico Válido", Cidade, especialidade,
            NivelExperiencia.Intermediario, 3);
        await CriarTecnicoAsync(
            "Técnico Outra Especialidade", Cidade, outraEspecialidade,
            NivelExperiencia.Avancado, 4);

        var ranking = await CriarServico()
            .GerarRankingAsync(especialidade.Id, Cidade);

        var tecnico = Assert.Single(ranking);
        Assert.Equal("Técnico Válido", tecnico.NomeExibicao);
    }

    // 11. Especialidade inativa não habilita o técnico no ranking
    [Fact]
    public async Task Ranking_NaoDeveMostrarTecnicoComEspecialidadeInativa()
    {
        var especialidade = await CriarEspecialidadeAsync("Hardware");
        await CriarTecnicoAsync(
            "Técnico Válido", Cidade, especialidade,
            NivelExperiencia.Intermediario, 3);

        especialidade.Inativar();
        await _context.SaveChangesAsync();

        var ranking = await CriarServico()
            .GerarRankingAsync(especialidade.Id, Cidade);

        Assert.Empty(ranking);
    }

    private async Task<Especialidade> CriarEspecialidadeAsync(string nome)
    {
        var especialidade = new Especialidade(
            nome, $"Atendimento de {nome}");
        _context.Especialidades.Add(especialidade);
        await _context.SaveChangesAsync();
        return especialidade;
    }

    private async Task<Tecnico> CriarTecnicoAsync(
        string nome,
        string cidade,
        Especialidade especialidade,
        NivelExperiencia nivel,
        int anos,
        bool ativo = true,
        bool disponivel = true)
    {
        var tecnico = new Tecnico(
            _proximoUsuarioId++,
            nome,
            "Técnico de testes",
            cidade,
            "RJ");
        tecnico.AdicionarEspecialidade(especialidade, nivel, anos);

        if (ativo)
        {
            tecnico.AlterarDisponibilidade(disponivel);
        }
        else
        {
            tecnico.Inativar();
        }

        _context.Tecnicos.Add(tecnico);
        await _context.SaveChangesAsync();
        return tecnico;
    }

    private IMatchingService CriarServico()
    {
        var criterios = new ICriterioPontuacaoMatching[]
        {
            new CriterioEspecialidade(),
            new CriterioExperiencia(),
            new CriterioLocalizacao()
        };

        return new MatchingService(
            new MatchingRepository(_context),
            criterios);
    }
}
