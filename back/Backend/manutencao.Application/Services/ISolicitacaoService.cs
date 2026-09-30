using Manutencao.Application.DTOs.Requests;
using Manutencao.Application.DTOs.Responses;

namespace Manutencao.Application.Services;

public interface ISolicitacaoService
{
    Task<HistoricoResponseDto> RegistrarHistoricoAsync(RegistrarHistoricoRequestDto dto);
    Task<SolicitacaoResponseDto> FinalizarServicoAsync(FinalizarServicoRequestDto dto);
    Task<IEnumerable<HistoricoResponseDto>> ObterHistoricoAsync(int solicitacaoId);
}