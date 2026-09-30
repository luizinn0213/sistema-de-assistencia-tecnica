using Assistencia.Api.Data;
using Assistencia.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginDto dados)
    {
        var email = dados.Email
            .Trim()
            .ToLowerInvariant();

        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (usuario is null ||
            !BCrypt.Net.BCrypt.Verify(
                dados.Senha,
                usuario.SenhaHash))
        {
            return BadRequest(new
            {
                erro = "E-mail ou senha inválidos."
            });
        }

        var clienteId = await _context.Clientes
            .Where(c => c.UsuarioId == usuario.Id)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();

        return Ok(new
        {
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            Tipo = usuario.Tipo.ToString(),
            ClienteId = clienteId
        });
    }
}
