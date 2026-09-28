using Assistencia.Api.Data;
using Assistencia.Api.Dtos;
using Assistencia.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Controllers;

[ApiController]
[Route("api/tecnicos")]
public class TecnicosController : ControllerBase
{
    private readonly AppDbContext _context;

    public TecnicosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> Listar()
    {
        var tecnicos = await _context.Tecnicos
            .AsNoTracking()
            .OrderBy(t => t.NomeExibicao)
            .ToListAsync();

        return Ok(tecnicos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> BuscarPorId(int id)
    {
        var tecnico = await _context.Tecnicos
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tecnico is null)
            return NotFound("Técnico não encontrado.");

        return Ok(tecnico);
    }

    [HttpPost]
    public async Task<ActionResult> Cadastrar(CriarTecnicoDto dados)
    {
        bool usuarioJaPossuiTecnico = await _context.Tecnicos
            .AnyAsync(t => t.UsuarioId == dados.UsuarioId);

        if (usuarioJaPossuiTecnico)
            return Conflict(
                "Esse usuário já possui um perfil de técnico.");

        try
        {
            var tecnico = new Tecnico(
                dados.UsuarioId,
                dados.NomeExibicao,
                dados.DescricaoProfissional,
                dados.CidadeAtendimento,
                dados.EstadoAtendimento);

            _context.Tecnicos.Add(tecnico);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(BuscarPorId),
                new { id = tecnico.Id },
                tecnico);
        }
        catch (ArgumentException erro)
        {
            return BadRequest(erro.Message);
        }
    }
}