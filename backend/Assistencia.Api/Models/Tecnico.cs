using Assistencia.Api.Enums;

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

    public ICollection<TecnicoEspecialidade> TecnicoEspecialidades
        { get; private set; } = new List<TecnicoEspecialidade>();

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
            throw new ArgumentException(
                "O usuário é obrigatório.");

        UsuarioId = usuarioId;

        AtualizarDados(
            nomeExibicao,
            descricaoProfissional,
            cidadeAtendimento,
            estadoAtendimento);

        Ativo = true;
        Disponivel = false;
        DataCadastro = DateTime.UtcNow;
    }

    public void AtualizarDados(
        string nomeExibicao,
        string descricaoProfissional,
        string cidadeAtendimento,
        string estadoAtendimento)
    {
        if (string.IsNullOrWhiteSpace(nomeExibicao))
            throw new ArgumentException(
                "O nome do técnico é obrigatório.");

        if (string.IsNullOrWhiteSpace(cidadeAtendimento))
            throw new ArgumentException(
                "A cidade de atendimento é obrigatória.");

        if (string.IsNullOrWhiteSpace(estadoAtendimento))
            throw new ArgumentException(
                "O estado de atendimento é obrigatório.");

        NomeExibicao = nomeExibicao.Trim();

        DescricaoProfissional =
            descricaoProfissional?.Trim() ?? string.Empty;

        CidadeAtendimento = cidadeAtendimento.Trim();

        EstadoAtendimento =
            estadoAtendimento.Trim().ToUpperInvariant();
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
        {
            throw new InvalidOperationException(
                "Um técnico inativo não pode ficar disponível.");
        }

        Disponivel = disponivel;
    }

    public void AdicionarEspecialidade(
        Especialidade especialidade,
        NivelExperiencia nivel,
        int anosExperiencia)
    {
        ArgumentNullException.ThrowIfNull(especialidade);

        if (!especialidade.Ativa)
        {
            throw new InvalidOperationException(
                "Não é possível adicionar uma especialidade inativa.");
        }

        bool jaPossui = TecnicoEspecialidades.Any(
            te => te.EspecialidadeId == especialidade.Id);

        if (jaPossui)
        {
            throw new InvalidOperationException(
                "O técnico já possui essa especialidade.");
        }

        var relacao = new TecnicoEspecialidade(
            this,
            especialidade,
            nivel,
            anosExperiencia);

        TecnicoEspecialidades.Add(relacao);
    }

    public void DesvincularEspecialidade(
        int especialidadeId)
    {
        var relacao = TecnicoEspecialidades
            .FirstOrDefault(
                te => te.EspecialidadeId == especialidadeId);

        if (relacao is null)
        {
            throw new InvalidOperationException(
                "Especialidade não vinculada ao técnico.");
        }

        TecnicoEspecialidades.Remove(relacao);
    }
}