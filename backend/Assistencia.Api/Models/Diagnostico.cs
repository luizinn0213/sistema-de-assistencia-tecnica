using Assistencia.Api.Enums;

namespace Assistencia.Api.Models;

public class Diagnostico
{
    public const int TamanhoMaximoDescricao = 2000;

    public int Id { get; private set; }

    public int SolicitacaoId { get; private set; }
    public Solicitacao? Solicitacao { get; private set; }

    public int TecnicoId { get; private set; }
    public Tecnico? Tecnico { get; private set; }

    public string DescricaoDefeito { get; private set; }
        = string.Empty;

    public string SolucaoRecomendada { get; private set; }
        = string.Empty;

    public StatusDiagnostico Status { get; private set; }

    public DateTime DataCriacao { get; private set; }
    public DateTime? DataConclusao { get; private set; }

    // Utilizado pelo Entity Framework.
    private Diagnostico()
    {
    }

    public Diagnostico(
        SelecaoTecnico selecao,
        string descricaoDefeito)
    {
        ArgumentNullException.ThrowIfNull(selecao);

        if (selecao.Status != StatusSelecaoTecnico.Aceita)
        {
            throw new InvalidOperationException(
                "O diagnóstico somente pode ser criado após " +
                "o técnico aceitar a solicitação.");
        }

        ValidarDescricaoDefeito(descricaoDefeito);

        SolicitacaoId = selecao.SolicitacaoId;
        Solicitacao = selecao.Solicitacao;

        TecnicoId = selecao.TecnicoId;
        Tecnico = selecao.Tecnico;

        DescricaoDefeito = descricaoDefeito.Trim();
        Status = StatusDiagnostico.EmElaboracao;
        DataCriacao = DateTime.UtcNow;
    }

    public void AlterarInformacoes(
        string descricaoDefeito,
        string solucaoRecomendada)
    {
        ValidarEmElaboracao();
        ValidarDescricaoDefeito(descricaoDefeito);
        ValidarSolucaoRecomendada(
            solucaoRecomendada,
            obrigatoria: false);

        DescricaoDefeito = descricaoDefeito.Trim();
        SolucaoRecomendada =
            solucaoRecomendada?.Trim() ?? string.Empty;
    }

    public void Concluir()
    {
        ValidarEmElaboracao();

        ValidarSolucaoRecomendada(
            SolucaoRecomendada,
            obrigatoria: true);

        Status = StatusDiagnostico.Concluido;
        DataConclusao = DateTime.UtcNow;
    }

    private void ValidarEmElaboracao()
    {
        if (Status != StatusDiagnostico.EmElaboracao)
        {
            throw new InvalidOperationException(
                "O diagnóstico concluído não pode ser alterado.");
        }
    }

    private static void ValidarDescricaoDefeito(
        string descricaoDefeito)
    {
        if (string.IsNullOrWhiteSpace(descricaoDefeito))
        {
            throw new ArgumentException(
                "A descrição do defeito é obrigatória.");
        }

        if (descricaoDefeito.Trim().Length >
            TamanhoMaximoDescricao)
        {
            throw new ArgumentException(
                $"A descrição do defeito deve possuir no máximo " +
                $"{TamanhoMaximoDescricao} caracteres.");
        }
    }

    private static void ValidarSolucaoRecomendada(
        string? solucaoRecomendada,
        bool obrigatoria)
    {
        if (obrigatoria &&
            string.IsNullOrWhiteSpace(solucaoRecomendada))
        {
            throw new InvalidOperationException(
                "A solução recomendada é obrigatória para " +
                "concluir o diagnóstico.");
        }

        if (!string.IsNullOrWhiteSpace(solucaoRecomendada) &&
            solucaoRecomendada.Trim().Length >
            TamanhoMaximoDescricao)
        {
            throw new ArgumentException(
                $"A solução recomendada deve possuir no máximo " +
                $"{TamanhoMaximoDescricao} caracteres.");
        }
    }
}