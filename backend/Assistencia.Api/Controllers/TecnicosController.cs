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
            .Include(t => t.TecnicoEspecialidades)
            .ThenInclude(te => te.Especialidade)
            .AsNoTracking()
            .OrderBy(t => t.NomeExibicao)
            .ToListAsync();

        var resposta = tecnicos.Select(t => new
        {
            t.Id,
            t.UsuarioId,
            t.NomeExibicao,
            t.DescricaoProfissional,
            t.CidadeAtendimento,
            t.EstadoAtendimento,
            t.Ativo,
            t.Disponivel,
            t.DataCadastro,

            Especialidades = t.TecnicoEspecialidades.Select(te => new
            {
                te.EspecialidadeId,
                Nome = te.Especialidade!.Nome,
                NivelExperiencia = te.NivelExperiencia.ToString(),
                te.AnosExperiencia
            })
        });

        return Ok(resposta);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> BuscarPorId(int id)
    {
        var tecnico = await _context.Tecnicos
            .Include(t => t.TecnicoEspecialidades)
            .ThenInclude(te => te.Especialidade)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tecnico is null)
            return NotFound("Técnico não encontrado.");

        var resposta = new
        {
            tecnico.Id,
            tecnico.UsuarioId,
            tecnico.NomeExibicao,
            tecnico.DescricaoProfissional,
            tecnico.CidadeAtendimento,
            tecnico.EstadoAtendimento,
            tecnico.Ativo,
            tecnico.Disponivel,
            tecnico.DataCadastro,

            Especialidades = tecnico.TecnicoEspecialidades.Select(te => new
            {
                te.EspecialidadeId,
                Nome = te.Especialidade!.Nome,
                NivelExperiencia = te.NivelExperiencia.ToString(),
                te.AnosExperiencia
            })
        };

        return Ok(resposta);
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

    [HttpPost("{id:int}/especialidades")]
    public async Task<ActionResult> AdicionarEspecialidade(
        int id,
        AdicionarEspecialidadeTecnicoDto dados)
    {
        var tecnico = await _context.Tecnicos
            .Include(t => t.TecnicoEspecialidades)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tecnico is null)
            return NotFound("Técnico não encontrado.");

        var especialidade = await _context.Especialidades
            .FirstOrDefaultAsync(e => e.Id == dados.EspecialidadeId);

        if (especialidade is null)
            return NotFound("Especialidade não encontrada.");

        try
        {
            tecnico.AdicionarEspecialidade(
                especialidade,
                dados.NivelExperiencia,
                dados.AnosExperiencia);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensagem = "Especialidade adicionada ao técnico.",
                TecnicoId = tecnico.Id,
                Tecnico = tecnico.NomeExibicao,
                EspecialidadeId = especialidade.Id,
                Especialidade = especialidade.Nome,
                NivelExperiencia = dados.NivelExperiencia.ToString(),
                dados.AnosExperiencia
            });
        }
        catch (InvalidOperationException erro)
        {
            return BadRequest(erro.Message);
        }
        catch (ArgumentException erro)
        {
            return BadRequest(erro.Message);
        }
    }

    [HttpPatch("{id:int}/disponibilidade")]
    public async Task<ActionResult> AlterarDisponibilidade(
        int id,
        AlterarDisponibilidadeDto dados)
    {
        var tecnico = await _context.Tecnicos
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tecnico is null)
            return NotFound("Técnico não encontrado.");

        try
        {
            tecnico.AlterarDisponibilidade(dados.Disponivel);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensagem = "Disponibilidade alterada.",
                tecnico.Id,
                tecnico.NomeExibicao,
                tecnico.Ativo,
                tecnico.Disponivel
            });
        }
        catch (InvalidOperationException erro)
        {
            return BadRequest(erro.Message);
        }
    }
    
}