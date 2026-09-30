using Microsoft.AspNetCore.Mvc;
using Manutencao.Application.DTOs.Requests;
using Manutencao.Application.Services;

namespace Manutencao.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SolicitacoesController : ControllerBase
{
    private readonly ISolicitacaoService _service;

    public SolicitacoesController(ISolicitacaoService service)
    {
        _service = service;
    }

    /// Registra uma entrada no histórico da solicitação.
   
    [HttpPost("historico")]
    public async Task<IActionResult> RegistrarHistorico([FromBody] RegistrarHistoricoRequestDto dto)
    {
        var resultado = await _service.RegistrarHistoricoAsync(dto);
        return CreatedAtAction(nameof(ObterHistorico), new { solicitacaoId = dto.SolicitacaoId }, resultado);
    }

 
    /// Retorna todo o histórico de uma solicitação.

    [HttpGet("{solicitacaoId:int}/historico")]
    public async Task<IActionResult> ObterHistorico(int solicitacaoId)
    {
        var historico = await _service.ObterHistoricoAsync(solicitacaoId);
        return Ok(historico);
    }

    /// Finaliza o serviço, registrando a solução aplicada.
    
    [HttpPost("finalizar")]
    public async Task<IActionResult> FinalizarServico([FromBody] FinalizarServicoRequestDto dto)
    {
        var resultado = await _service.FinalizarServicoAsync(dto);
        return Ok(resultado);
    }
}