using Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAutenticacaoService _autenticacao;
    private readonly IUsuarioAtual _usuarioAtual;

    public AuthController(AppDbContext db, IAutenticacaoService autenticacao, IUsuarioAtual usuarioAtual)
    {
        _db = db;
        _autenticacao = autenticacao;
        _usuarioAtual = usuarioAtual;
    }

    /// <summary>
    /// Login: devolve access token (JWT) + refresh token + dados do usuário.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(Politicas.RateLimitAuth)]
    [ProducesResponseType(typeof(TokenRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TokenRespostaDto>> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        // Resposta genérica: não revela se o e-mail existe ou não.
        if (usuario is null || !SenhaConfere(dto.Senha, usuario.SenhaHash))
            return BadRequest(new { erro = "E-mail ou senha inválidos." });

        if (!usuario.Ativo)
            return BadRequest(new { erro = "Usuário desativado. Procure o administrador." });

        var tokens = await _autenticacao.EmitirTokensAsync(usuario, IpDoCliente(), ct);
        return Ok(tokens);
    }

    /// <summary>
    /// Renova o access token. O refresh token usado é revogado (rotação):
    /// cada refresh token só pode ser utilizado uma vez.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(Politicas.RateLimitAuth)]
    [ProducesResponseType(typeof(TokenRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TokenRespostaDto>> Refresh([FromBody] RefreshDto dto, CancellationToken ct)
    {
        var resposta = await _autenticacao.RenovarAsync(dto.RefreshToken, IpDoCliente(), ct);

        if (resposta is null)
            return Unauthorized(new
            {
                erro = "Refresh token inválido, expirado ou já utilizado. Faça login novamente."
            });

        return Ok(resposta);
    }

    /// <summary>Encerra a sessão revogando todos os refresh tokens do usuário.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var usuarioId = _usuarioAtual.Id;
        if (usuarioId is null)
            return Unauthorized(new { erro = "Token sem identificação de usuário." });

        await _autenticacao.RevogarTodosAsync(usuarioId.Value, ct);
        return Ok(new { mensagem = "Sessão encerrada em todos os dispositivos." });
    }

    /// <summary>
    /// Usuário identificado automaticamente pelo token — não recebe id algum.
    /// </summary>
    [HttpGet("eu")]
    [Authorize]
    [ProducesResponseType(typeof(UsuarioRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UsuarioRespostaDto>> Eu(CancellationToken ct)
    {
        var usuarioId = _usuarioAtual.Id;
        if (usuarioId is null)
            return Unauthorized(new { erro = "Token sem identificação de usuário." });

        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == usuarioId.Value, ct);

        if (usuario is null)
            return NotFound(new { erro = "Usuário não encontrado." });

        if (!usuario.Ativo)
            return Unauthorized(new { erro = "Usuário desativado." });

        var clienteId = await _autenticacao.ObterClienteIdAsync(usuario.Id, ct);

        return Ok(new UsuarioRespostaDto(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Telefone,
            Perfis.De(usuario.Tipo),
            clienteId));
    }

    /// <summary>Troca a própria senha e derruba as sessões antigas.</summary>
    [HttpPut("senha")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TrocarSenha([FromBody] TrocaSenhaDto dto, CancellationToken ct)
    {
        var usuarioId = _usuarioAtual.Id;
        if (usuarioId is null)
            return Unauthorized(new { erro = "Token sem identificação de usuário." });

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId.Value, ct);
        if (usuario is null)
            return NotFound(new { erro = "Usuário não encontrado." });

        if (!SenhaConfere(dto.SenhaAtual, usuario.SenhaHash))
            return BadRequest(new { erro = "Senha atual incorreta." });

        if (dto.SenhaAtual == dto.NovaSenha)
            return BadRequest(new { erro = "A nova senha deve ser diferente da atual." });

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha);
        usuario.AtualizadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        // Sessões antigas não podem mais renovar o token.
        await _autenticacao.RevogarTodosAsync(usuario.Id, ct);

        return Ok(new { mensagem = "Senha alterada. Entre novamente com a nova senha." });
    }

    private string? IpDoCliente() => HttpContext.Connection.RemoteIpAddress?.ToString();

    /// <summary>BCrypt.Verify lança exceção com hash corrompido — tratamos como senha errada.</summary>
    private static bool SenhaConfere(string senha, string hash)
    {
        if (string.IsNullOrEmpty(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(senha, hash);
        }
        catch
        {
            return false;
        }
    }
}
