using Assistencia.Api.Dtos;
using Assistencia.Api.Models;
using Assistencia.Api.Repositories.Contracts;
using Assistencia.Api.Security;
using Assistencia.Api.Services.Contracts;

namespace Assistencia.Api.Services;

public class AutenticacaoService : IAutenticacaoService
{
    private readonly IAutenticacaoRepository _repository;
    private readonly ITokenService _tokenService;

    public AutenticacaoService(
        IAutenticacaoRepository repository,
        ITokenService tokenService)
    {
        _repository = repository;
        _tokenService = tokenService;
    }

    public async Task<AutenticacaoRespostaDto> LoginAsync(
        LoginDto dados,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dados);

        var email = dados.Email.Trim().ToLowerInvariant();
        var usuario = await _repository.ObterUsuarioPorEmailAsync(
            email,
            cancellationToken);

        if (usuario is null || !SenhaValida(dados.Senha, usuario.SenhaHash))
        {
            throw new ArgumentException("E-mail ou senha inválidos.");
        }

        return await EmitirTokensAsync(usuario, cancellationToken);
    }

    public async Task<AutenticacaoRespostaDto> RenovarAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("O refresh token é obrigatório.");
        }

        var hash = _tokenService.CalcularHash(refreshToken);
        var tokenAtual = await _repository.ObterRefreshTokenAsync(
            hash,
            cancellationToken);

        if (tokenAtual is null || !tokenAtual.Ativo || tokenAtual.Usuario is null)
        {
            throw new UnauthorizedAccessException(
                "Refresh token inválido, expirado ou já utilizado.");
        }

        var clienteId = await _repository.ObterClienteIdAsync(
            tokenAtual.UsuarioId,
            cancellationToken);
        var access = _tokenService.GerarAccessToken(
            tokenAtual.Usuario,
            clienteId);
        var novoRefresh = _tokenService.GerarRefreshToken(
            tokenAtual.UsuarioId);

        tokenAtual.Revogar(novoRefresh.Registro.TokenHash);
        await _repository.AdicionarRefreshTokenAsync(
            novoRefresh.Registro,
            cancellationToken);
        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return CriarResposta(
            tokenAtual.Usuario,
            clienteId,
            access,
            novoRefresh);
    }

    public async Task<SessaoUsuarioDto> ObterSessaoAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _repository.ObterUsuarioPorIdAsync(
            usuarioId,
            cancellationToken)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");
        var clienteId = await _repository.ObterClienteIdAsync(
            usuarioId,
            cancellationToken);
        return CriarUsuario(usuario, clienteId);
    }

    public async Task LogoutAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        await _repository.RevogarTokensAtivosAsync(
            usuarioId,
            cancellationToken);
        await _repository.SalvarAlteracoesAsync(cancellationToken);
    }

    public async Task TrocarSenhaAsync(
        int usuarioId,
        TrocarSenhaDto dados,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dados);
        var usuario = await _repository.ObterUsuarioPorIdAsync(
            usuarioId,
            cancellationToken)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        if (!SenhaValida(dados.SenhaAtual, usuario.SenhaHash))
        {
            throw new ArgumentException("A senha atual está incorreta.");
        }

        if (dados.SenhaAtual == dados.NovaSenha)
        {
            throw new ArgumentException(
                "A nova senha deve ser diferente da senha atual.");
        }

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dados.NovaSenha);
        await _repository.RevogarTokensAtivosAsync(
            usuarioId,
            cancellationToken);
        await _repository.SalvarAlteracoesAsync(cancellationToken);
    }

    private async Task<AutenticacaoRespostaDto> EmitirTokensAsync(
        Usuario usuario,
        CancellationToken cancellationToken)
    {
        var clienteId = await _repository.ObterClienteIdAsync(
            usuario.Id,
            cancellationToken);
        var access = _tokenService.GerarAccessToken(usuario, clienteId);
        var refresh = _tokenService.GerarRefreshToken(usuario.Id);

        await _repository.AdicionarRefreshTokenAsync(
            refresh.Registro,
            cancellationToken);
        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return CriarResposta(usuario, clienteId, access, refresh);
    }

    private static AutenticacaoRespostaDto CriarResposta(
        Usuario usuario,
        int? clienteId,
        (string Token, DateTime ExpiraEm) access,
        (RefreshToken Registro, string Token) refresh)
    {
        return new AutenticacaoRespostaDto
        {
            AccessToken = access.Token,
            AccessTokenExpiraEm = access.ExpiraEm,
            RefreshToken = refresh.Token,
            RefreshTokenExpiraEm = refresh.Registro.DataExpiracao,
            Usuario = CriarUsuario(usuario, clienteId)
        };
    }

    private static SessaoUsuarioDto CriarUsuario(
        Usuario usuario,
        int? clienteId)
    {
        return new SessaoUsuarioDto
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Tipo = usuario.Tipo.ToString(),
            ClienteId = clienteId
        };
    }

    private static bool SenhaValida(string senha, string hash)
    {
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
