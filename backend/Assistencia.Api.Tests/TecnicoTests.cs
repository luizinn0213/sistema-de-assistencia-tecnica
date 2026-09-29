using Assistencia.Api.Enums;
using Assistencia.Api.Models;
using Xunit;

namespace Assistencia.Api.Tests;

public class TecnicoTests
{
    private static Tecnico CriarTecnico()
    {
        return new Tecnico(
            1,
            "Marina Costa",
            "Especialista em manutenção",
            "Teresópolis",
            "RJ");
    }

    [Fact]
    public void Inativar_DeveDeixarTecnicoIndisponivel()
    {
        var tecnico = CriarTecnico();

        tecnico.AlterarDisponibilidade(true);
        tecnico.Inativar();

        Assert.False(tecnico.Ativo);
        Assert.False(tecnico.Disponivel);
    }

    [Fact]
    public void TecnicoInativo_NaoPodeFicarDisponivel()
    {
        var tecnico = CriarTecnico();

        tecnico.Inativar();

        Assert.Throws<InvalidOperationException>(() =>
            tecnico.AlterarDisponibilidade(true));
    }

    [Fact]
    public void NaoDevePermitirEspecialidadeDuplicada()
    {
        var tecnico = CriarTecnico();

        var especialidade = new Especialidade(
            "Celulares",
            "Manutenção de celulares");

        tecnico.AdicionarEspecialidade(
            especialidade,
            NivelExperiencia.Intermediario,
            3);

        Assert.Throws<InvalidOperationException>(() =>
            tecnico.AdicionarEspecialidade(
                especialidade,
                NivelExperiencia.Avancado,
                5));
    }

    [Fact]
    public void NaoDeveAdicionarEspecialidadeInativa()
    {
        var tecnico = CriarTecnico();

        var especialidade = new Especialidade(
            "Notebooks",
            "Manutenção de notebooks");

        especialidade.Inativar();

        Assert.Throws<InvalidOperationException>(() =>
            tecnico.AdicionarEspecialidade(
                especialidade,
                NivelExperiencia.Iniciante,
                1));
    }

    [Fact]
    public void AnosExperiencia_NaoPodemSerNegativos()
    {
        var tecnico = CriarTecnico();

        var especialidade = new Especialidade(
            "Computadores",
            "Manutenção de computadores");

        var vinculo = new TecnicoEspecialidade(
            tecnico,
            especialidade,
            NivelExperiencia.Intermediario,
            2);

        Assert.Throws<ArgumentException>(() =>
            vinculo.AlterarAnosExperiencia(-1));
    }

    [Fact]
    public void DeveDesvincularEspecialidade()
    {
        var tecnico = CriarTecnico();

        var especialidade = new Especialidade(
            "Celulares",
            "Manutenção de celulares");

        tecnico.AdicionarEspecialidade(
            especialidade,
            NivelExperiencia.Avancado,
            4);

        tecnico.DesvincularEspecialidade(
            especialidade.Id);

        Assert.Empty(
            tecnico.TecnicoEspecialidades);
    }
}