using Assistencia.Api.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assistencia.Api.Models;

public class Tecnico
{
    public int Id { get; private set; }
    public int UsuarioId { get; private set; }
    public string NomeExibicao { get; private set; } = string.Empty;
    public string DescricaoProfissional { get; private set; } = string.Empty;
    public string CidadeAtendimento { get; private set; } = string.Empty;
    public string EstadoAtendimento { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }
    public bool Disponivel { get; private set; }
    public DateTime DataCadastro { get; private set; }

    public ICollection<TecnicoEspecialidade> TecnicoEspecialidades { get; private set; }
        = new List<TecnicoEspecialidade>();

    // Usado pelo Entity Framework
    public Tecnico()
    {
    }

    public Tecnico(
        int usuarioId,
        string nomeExibicao,
        string descricaoProfissional,
        string cidadeAtendimento,
        string estadoAtendimento)
    {
        if (usuarioId <= 0)
            throw new ArgumentException("O usuário é obrigatório.");

        if (string.IsNullOrWhiteSpace(nomeExibicao))
            throw new ArgumentException("O nome do técnico é obrigatório.");

        if (string.IsNullOrWhiteSpace(cidadeAtendimento))
            throw new ArgumentException("A cidade de atendimento é obrigatória.");

        if (string.IsNullOrWhiteSpace(estadoAtendimento))
            throw new ArgumentException("O estado de atendimento é obrigatório.");

        UsuarioId = usuarioId;
        NomeExibicao = nomeExibicao;
        DescricaoProfissional = descricaoProfissional;
        CidadeAtendimento = cidadeAtendimento;
        EstadoAtendimento = estadoAtendimento;

        Ativo = true;
        Disponivel = false;
        DataCadastro = DateTime.UtcNow;
    }

    public void Ativar()
    {
        Ativo = true;
    }

    public void Inativar()
    {
        Ativo = false;
        Disponivel = false;
    }

    public void AlterarDisponibilidade(bool disponivel)
    {
        if (!Ativo && disponivel)
            throw new InvalidOperationException(
                "Um técnico inativo não pode ficar disponível.");

        Disponivel = disponivel;
    }

    public void AdicionarEspecialidade(
        Especialidade especialidade,
        NivelExperiencia nivel,
        int anosExperiencia)
    {
        if (especialidade is null)
            throw new ArgumentNullException(nameof(especialidade));

        if (!especialidade.Ativa)
            throw new InvalidOperationException(
                "Não é possível adicionar uma especialidade inativa.");

        bool jaPossui = TecnicoEspecialidades.Any(
            te => te.EspecialidadeId == especialidade.Id);

        if (jaPossui)
            throw new InvalidOperationException(
                "O técnico já possui essa especialidade.");

        var relacao = new TecnicoEspecialidade(
            this,
            especialidade,
            nivel,
            anosExperiencia);

        TecnicoEspecialidades.Add(relacao);
    }
}