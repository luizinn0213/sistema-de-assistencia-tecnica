using Assistencia.Api.Data;
using Assistencia.Api.Dtos;
using Assistencia.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Controllers;

[ApiController]
[Route("api/equipamentos")]
public class EquipamentosController : ControllerBase
{
    private readonly AppDbContext _context;

    public EquipamentosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("cliente/{clienteId:int}")]
    public async Task<ActionResult> ListarPorCliente(
        int clienteId)
    {
        var clienteExiste = await _context.Clientes
            .AnyAsync(c => c.Id == clienteId);

        if (!clienteExiste)
        {
            return NotFound(
                new { erro = "Cliente nÃ£o encontrado." });
        }

        var equipamentos = await _context.Equipamentos
            .AsNoTracking()
            .Where(e => e.ClienteId == clienteId)
            .OrderBy(e => e.Tipo)
            .ThenBy(e => e.Marca)
            .ToListAsync();

        return Ok(equipamentos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> Obter(int id)
    {
        var equipamento = await _context.Equipamentos
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (equipamento is null)
        {
            return NotFound(
                new { erro = "Equipamento nÃ£o encontrado." });
        }

        return Ok(equipamento);
    }

    [HttpPost("cliente/{clienteId:int}")]
    public async Task<ActionResult> Cadastrar(
        int clienteId,
        CriarEquipamentoDto dados)
    {
        var clienteExiste = await _context.Clientes
            .AnyAsync(c => c.Id == clienteId);

        if (!clienteExiste)
        {
            return NotFound(
                new { erro = "Cliente nÃ£o encontrado." });
        }

        var erro = Validar(dados);

        if (erro is not null)
        {
            return BadRequest(new { erro });
        }

        var numeroSerie = dados.NumeroSerie.Trim();

        var numeroSerieCadastrado =
            await _context.Equipamentos.AnyAsync(e =>
                e.NumeroSerie == numeroSerie);

        if (numeroSerieCadastrado)
        {
            return Conflict(new
            {
                erro =
                    "NÃºmero de sÃ©rie jÃ¡ cadastrado."
            });
        }

        var equipamento = new Equipamento
        {
            ClienteId = clienteId,
            Tipo = dados.Tipo.Trim(),
            Marca = dados.Marca.Trim(),
            Modelo = dados.Modelo.Trim(),
            NumeroSerie = numeroSerie
        };

        _context.Equipamentos.Add(equipamento);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(Obter),
            new { id = equipamento.Id },
            equipamento);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Atualizar(
        int id,
        CriarEquipamentoDto dados)
    {
        var equipamento = await _context.Equipamentos
            .FirstOrDefaultAsync(e => e.Id == id);

        if (equipamento is null)
        {
            return NotFound(
                new { erro = "Equipamento nÃ£o encontrado." });
        }

        var erro = Validar(dados);

        if (erro is not null)
        {
            return BadRequest(new { erro });
        }

        var numeroSerie = dados.NumeroSerie.Trim();

        var numeroSerieUtilizado =
            await _context.Equipamentos.AnyAsync(e =>
                e.Id != id &&
                e.NumeroSerie == numeroSerie);

        if (numeroSerieUtilizado)
        {
            return Conflict(new
            {
                erro =
                    "NÃºmero de sÃ©rie jÃ¡ cadastrado."
            });
        }

        equipamento.Tipo = dados.Tipo.Trim();
        equipamento.Marca = dados.Marca.Trim();
        equipamento.Modelo = dados.Modelo.Trim();
        equipamento.NumeroSerie = numeroSerie;

        await _context.SaveChangesAsync();

        return Ok(equipamento);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Excluir(int id)
    {
        var equipamento = await _context.Equipamentos
            .FirstOrDefaultAsync(e => e.Id == id);

        if (equipamento is null)
        {
            return NotFound(
                new { erro = "Equipamento nÃ£o encontrado." });
        }

        _context.Equipamentos.Remove(equipamento);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static string? Validar(
        CriarEquipamentoDto dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Tipo))
        {
            return "O tipo do equipamento Ã© obrigatÃ³rio.";
        }

        if (string.IsNullOrWhiteSpace(dados.Marca))
        {
            return "A marca Ã© obrigatÃ³ria.";
        }

        if (string.IsNullOrWhiteSpace(dados.Modelo))
        {
            return "O modelo Ã© obrigatÃ³rio.";
        }

        if (string.IsNullOrWhiteSpace(
                dados.NumeroSerie) ||
            dados.NumeroSerie.Trim().Length < 4)
        {
            return "NÃºmero de sÃ©rie invÃ¡lido.";
        }

        return null;
    }
}
