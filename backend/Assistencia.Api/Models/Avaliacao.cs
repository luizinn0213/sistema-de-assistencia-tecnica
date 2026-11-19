using Assistencia.Api.Enums;

namespace Assistencia.Api.Models;

public class Avaliacao
{
    public const int TamanhoMaximoComentario = 1000;

    public int Id { get; private set; }
    public int ExecucaoServicoId { get; private set; }
    public ExecucaoServico? ExecucaoServico { get; private set; }
    public int ClienteId { get; private set; }
    public Cliente? Cliente { get; private set; }
    public int Nota { get; private set; }
    public string? Comentario { get; private set; }
    public DateTime DataAvaliacao { get; private set; }

    private Avaliacao()
    {
    }

    public Avaliacao(
        ExecucaoServico execucaoServico,
        int clienteId,
        int nota,
        string? comentario = null)
    {
        ArgumentNullException.ThrowIfNull(execucaoServico);

        if (execucaoServico.Status != StatusExecucaoServico.Concluida)
        {
            throw new InvalidOperationException(
                "Somente uma execução concluída pode ser avaliada.");
        }

        if (clienteId <= 0)
        {
            throw new ArgumentException(
                "O cliente é obrigatório.");
        }

        if (nota < 1 || nota > 5)
        {
            throw new ArgumentException(
                "A nota deve estar entre 1 e 5.");
        }

        var texto = string.IsNullOrWhiteSpace(comentario)
            ? null
            : comentario.Trim();

        if (texto?.Length > TamanhoMaximoComentario)
        {
            throw new ArgumentException(
                $"O comentário deve possuir no máximo " +
                $"{TamanhoMaximoComentario} caracteres.");
        }

        ExecucaoServico = execucaoServico;
        ExecucaoServicoId = execucaoServico.Id;
        ClienteId = clienteId;
        Nota = nota;
        Comentario = texto;
        DataAvaliacao = DateTime.UtcNow;
    }
}
