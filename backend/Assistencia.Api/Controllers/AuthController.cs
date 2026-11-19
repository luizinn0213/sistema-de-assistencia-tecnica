using Assistencia.Api.Dtos;
using Assistencia.Api.Security;
using Assistencia.Api.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assistencia.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAutenticacaoService _autenticacaoService;
    private readonly IUsuarioAtual _usuarioAtual;

    public AuthController(
        IAutenticacaoService autenticacaoService,
        IUsuarioAtual usuarioAtual)
    {
        _autenticacaoService = autenticacaoService;
        _usuarioAtual = usuarioAtual;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult> Login(
        LoginDto dados,
        CancellationToken cancellationToken)
    {
        var resposta = await _autenticacaoService.LoginAsync(
            dados,
            cancellationToken);
        return Ok(resposta);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult> Refresh(
        RefreshTokenDto dados,
        CancellationToken cancellationToken)
    {
        var resposta = await _autenticacaoService.RenovarAsync(
            dados.RefreshToken,
            cancellationToken);
        return Ok(resposta);
    }

    [Authorize]
    [HttpGet("eu")]
    public async Task<ActionResult> Eu(
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        var resposta = await _autenticacaoService.ObterSessaoAsync(
            usuarioId,
            cancellationToken);
        return Ok(resposta);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout(
        CancellationToken cancellationToken)
    {
        await _autenticacaoService.LogoutAsync(
            ObterUsuarioId(),
            cancellationToken);
        return Ok(new { mensagem = "Sessão encerrada com sucesso." });
    }

    [Authorize]
    [HttpPut("senha")]
    public async Task<ActionResult> TrocarSenha(
        TrocarSenhaDto dados,
        CancellationToken cancellationToken)
    {
        await _autenticacaoService.TrocarSenhaAsync(
            ObterUsuarioId(),
            dados,
            cancellationToken);
        return Ok(new
        {
            mensagem = "Senha alterada. Entre novamente com a nova senha."
        });
    }

    private int ObterUsuarioId()
    {
        return _usuarioAtual.UsuarioId
            ?? throw new UnauthorizedAccessException(
                "Token sem identificação de usuário.");
    }
}
