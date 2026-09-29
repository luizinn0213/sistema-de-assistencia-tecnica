using Assistencia.Api.Data;
using Assistencia.Api.Dtos;
using Assistencia.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Controllers;

[ApiController]
[Route("api/especialidades")]
public class EspecialidadesController : ControllerBase
{
    private readonly AppDbContext _context;

    public EspecialidadesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> Listar()
    {
        var especialidades = await _context.Especialidades
            .AsNoTracking()
            .OrderBy(e => e.Nome)
            .ToListAsync();

        return Ok(especialidades);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> BuscarPorId(int id)
    {
        var especialidade = await _context.Especialidades
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (especialidade is null)
            return NotFound("Especialidade não encontrada.");

        return Ok(especialidade);
    }

    [HttpPost]
    public async Task<ActionResult> Cadastrar(
        CriarEspecialidadeDto dados)
    {
        try
        {
            var especialidade = new Especialidade(
                dados.Nome,
                dados.Descricao);

            _context.Especialidades.Add(especialidade);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(BuscarPorId),
                new { id = especialidade.Id },
                especialidade);
        }
        catch (ArgumentException erro)
        {
            return BadRequest(erro.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Atualizar(
        int id,
        CriarEspecialidadeDto dados)
    {
        var especialidade = await _context.Especialidades
            .FirstOrDefaultAsync(e => e.Id == id);

        if (especialidade is null)
            return NotFound("Especialidade não encontrada.");

        try
        {
            especialidade.AtualizarDados(
                dados.Nome,
                dados.Descricao);

            await _context.SaveChangesAsync();

            return Ok(especialidade);
        }
        catch (ArgumentException erro)
        {
            return BadRequest(erro.Message);
        }
    }

    [HttpPatch("{id:int}/ativar")]
    public async Task<ActionResult> Ativar(int id)
    {
        var especialidade = await _context.Especialidades
            .FirstOrDefaultAsync(e => e.Id == id);

        if (especialidade is null)
            return NotFound("Especialidade não encontrada.");

        especialidade.Ativar();
        await _context.SaveChangesAsync();

        return Ok(especialidade);
    }

    [HttpPatch("{id:int}/inativar")]
    public async Task<ActionResult> Inativar(int id)
    {
        var especialidade = await _context.Especialidades
            .FirstOrDefaultAsync(e => e.Id == id);

        if (especialidade is null)
            return NotFound("Especialidade não encontrada.");

        especialidade.Inativar();
        await _context.SaveChangesAsync();

        return Ok(especialidade);
    }
}