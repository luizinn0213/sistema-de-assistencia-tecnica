using Assistencia.Api.Dtos.Matching;
using Assistencia.Api.Repositories.Contracts;
using Assistencia.Api.Services.Contracts;

namespace Assistencia.Api.Services.Matching;

public class MatchingService : IMatchingService
{
    private readonly IMatchingRepository _matchingRepository;
    private readonly IEnumerable<ICriterioPontuacaoMatching> _criterios;

    public MatchingService(
        IMatchingRepository matchingRepository,
        IEnumerable<ICriterioPontuacaoMatching> criterios)
    {
        _matchingRepository = matchingRepository;
        _criterios = criterios;
    }

    public async Task<List<TecnicoRankingDto>> GerarRankingAsync(
        int especialidadeId,
        string? cidade = null,
        int? solicitacaoId = null,
        CancellationToken cancellationToken = default)
    {
        if (especialidadeId <= 0)
        {
            return new List<TecnicoRankingDto>();
        }

        var tecnicos = await _matchingRepository.BuscarTecnicosElegiveisAsync(
            especialidadeId,
            solicitacaoId,
            cancellationToken);

        var ranking = new List<TecnicoRankingDto>();

        foreach (var tecnico in tecnicos)
        {
            var vinculo = tecnico.TecnicoEspecialidades
                .First(te => te.EspecialidadeId == especialidadeId);

            var mesmaCidade = !string.IsNullOrWhiteSpace(cidade) &&
                              string.Equals(
                                  tecnico.CidadeAtendimento.Trim(),
                                  cidade.Trim(),
                                  StringComparison.OrdinalIgnoreCase);

            var contexto = new ContextoPontuacaoMatching
            {
                PossuiEspecialidade = true,
                NivelExperiencia = vinculo.NivelExperiencia,
                AnosExperiencia = vinculo.AnosExperiencia,
                MesmaCidade = mesmaCidade
            };

            var pontuacaoTotal = 0;
            var destaques = new List<string>();

            foreach (var criterio in _criterios)
            {
                var resultado = criterio.Calcular(contexto);
                pontuacaoTotal += resultado.Pontos;

                if (!string.IsNullOrWhiteSpace(resultado.Destaque))
                {
                    destaques.Add(resultado.Destaque);
                }
            }

            ranking.Add(new TecnicoRankingDto
            {
                Id = tecnico.Id,
                NomeExibicao = tecnico.NomeExibicao,
                DescricaoProfissional = tecnico.DescricaoProfissional,
                CidadeAtendimento = tecnico.CidadeAtendimento,
                EstadoAtendimento = tecnico.EstadoAtendimento,
                EspecialidadeId = vinculo.EspecialidadeId,
                Especialidade = vinculo.Especialidade!.Nome,
                NivelExperiencia = vinculo.NivelExperiencia,
                AnosExperiencia = vinculo.AnosExperiencia,
                MesmaCidade = mesmaCidade,
                Pontuacao = pontuacaoTotal,
                Destaques = destaques
            });
        }

        return ranking
            .OrderByDescending(r => r.Pontuacao)
            .ThenBy(r => r.NomeExibicao, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
