using Assistencia.Api.Enums;
using System;

namespace Assistencia.Api.Models;

public class TecnicoEspecialidade
{
    public int TecnicoId { get; private set; }
    public Tecnico? Tecnico { get; private set; }

    public int EspecialidadeId { get; private set; }
    public Especialidade? Especialidade { get; private set; }

    public NivelExperiencia NivelExperiencia { get; private set; }
    public int AnosExperiencia { get; private set; }
    public string Observacao { get; private set; } = string.Empty;

    // Usado pelo Entity Framework
    public TecnicoEspecialidade()
    {
    }

    public TecnicoEspecialidade(
        Tecnico tecnico,
        Especialidade especialidade,
        NivelExperiencia nivel,
        int anosExperiencia)
    {
        Tecnico = tecnico;
        TecnicoId = tecnico.Id;

        Especialidade = especialidade;
        EspecialidadeId = especialidade.Id;

        AlterarNivel(nivel);
        AlterarAnosExperiencia(anosExperiencia);
    }

    public void AlterarNivel(NivelExperiencia nivel)
    {
        NivelExperiencia = nivel;
    }

    public void AlterarAnosExperiencia(int anos)
    {
        if (anos < 0)
            throw new ArgumentException(
                "Os anos de experiência não podem ser negativos.");

        AnosExperiencia = anos;
    }

    public void AlterarObservacao(string observacao)
    {
        Observacao = observacao;
    }
}