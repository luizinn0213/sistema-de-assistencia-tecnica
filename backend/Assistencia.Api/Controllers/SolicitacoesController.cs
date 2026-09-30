using Assistencia.Api.Data;
using Assistencia.Api.Dtos;
using Assistencia.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Controllers;

[ApiController]
[Route("api/solicitacoes")]
public class SolicitacoesController : ControllerBase
{
    private readonly AppDbContext _context;

    public SolicitacoesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("cliente/{clienteId:int}")]
    public async Task<ActionResult> Listar(int clienteId)
    {
        var clienteExiste = await _context.Clientes
            .AnyAsync(c => c.Id == clienteId);

        if (!clienteExiste)
        {
            return NotFound(
                new { erro = "Cliente não encontrado." });
        }

        var solicitacoes = await _context.Solicitacoes
            .Include(s => s.Equipamento)
            .Include(s => s.Especialidade)
            .AsNoTracking()
            .Where(s => s.ClienteId == clienteId)
            .OrderByDescending(s => s.DataCriacao)
            .ToListAsync();

        return Ok(solicitacoes.Select(CriarResposta).ToList());
    }

    [HttpGet("cliente/{clienteId:int}/{id:int}")]
    public async Task<ActionResult> Obter(
        int clienteId,
        int id)
    {
        var solicitacao = await _context.Solicitacoes
            .Include(s => s.Equipamento)
            .Include(s => s.Especialidade)
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == id &&
                s.ClienteId == clienteId);

        if (solicitacao is null)
        {
            return NotFound(
                new { erro = "Solicitação não encontrada." });
        }

        return Ok(CriarResposta(solicitacao));
    }

    [HttpPost("cliente/{clienteId:int}")]
    public async Task<ActionResult> Abrir(
        int clienteId,
        CriarSolicitacaoDto dados)
    {
        var clienteExiste = await _context.Clientes
            .AnyAsync(c => c.Id == clienteId);

        if (!clienteExiste)
        {
            return NotFound(
                new { erro = "Cliente não encontrado." });
        }

        var equipamento = await _context.Equipamentos
            .FirstOrDefaultAsync(e =>
                e.Id == dados.EquipamentoId);

        if (equipamento is null)
        {
            return NotFound(
                new { erro = "Equipamento não encontrado." });
        }

        var especialidade = await _context.Especialidades
            .FirstOrDefaultAsync(e =>
                e.Id == dados.EspecialidadeId);

        if (especialidade is null)
        {
            return NotFound(
                new { erro = "Especialidade não encontrada." });
        }

        try
        {
            var solicitacao = new Solicitacao(
                clienteId,
                equipamento,
                especialidade,
                dados.DescricaoProblema);

            _context.Solicitacoes.Add(solicitacao);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Obter),
                new
                {
                    clienteId,
                    id = solicitacao.Id
                },
                CriarResposta(solicitacao));
        }
        catch (InvalidOperationException erro)
        {
            return BadRequest(new { erro = erro.Message });
        }
        catch (ArgumentException erro)
        {
            return BadRequest(new { erro = erro.Message });
        }
    }

    private static object CriarResposta(
        Solicitacao solicitacao)
    {
        return new
        {
            solicitacao.Id,
            solicitacao.Numero,
            solicitacao.DescricaoProblema,
            Status = solicitacao.Status.ToString(),

            DataCriacao = DateTime.SpecifyKind(
                solicitacao.DataCriacao,
                DateTimeKind.Utc),

            solicitacao.ClienteId,

            Equipamento = new
            {
                solicitacao.Equipamento!.Id,
                solicitacao.Equipamento.Tipo,
                solicitacao.Equipamento.Marca,
                solicitacao.Equipamento.Modelo
            },

            Especialidade = new
            {
                solicitacao.Especialidade!.Id,
                solicitacao.Especialidade.Nome
            }
        };
    }
}
