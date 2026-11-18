using Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <summary>
/// Gestão de usuários e perfis. Todo o controller é exclusivo do Administrador
/// (a política também está declarada no controller para não depender de detalhe
/// de cada action).
/// </summary>
[ApiController]
[Route("api/usuarios")]
[Authorize(Policy = Politicas.SomenteAdministrador)]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IUsuarioAtual _usuarioAtual;
    private readonly IAutenticacaoService _autenticacao;

    public UsuariosController(AppDbContext db, IUsuarioAtual usuarioAtual, IAutenticacaoService autenticacao)
    {
        _db = db;
        _usuarioAtual = usuarioAtual;
        _autenticacao = autenticacao;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<UsuarioListaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaDto<UsuarioListaDto>>> Listar(
        [FromQuery] TipoUsuario? tipo,
        [FromQuery] bool? ativo,
        [FromQuery] string? busca,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken ct = default)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanho = Math.Clamp(tamanho, 1, 100);

        var consulta = _db.Usuarios.AsNoTracking();

        if (tipo is TipoUsuario perfil)
            consulta = consulta.Where(u => u.Tipo == perfil);

        if (ativo is bool estaAtivo)
            consulta = consulta.Where(u => u.Ativo == estaAtivo);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            consulta = consulta.Where(u =>
                EF.Functions.Like(u.Nome, "%" + termo + "%") ||
                EF.Functions.Like(u.Email, "%" + termo + "%"));
        }

        var total = await consulta.CountAsync(ct);

        // A coluna Tipo é gravada como texto; projeto o enum e converte em memória
        // (evita depender da tradução de ToString() para SQL).
        var registros = await consulta
            .OrderBy(u => u.Nome)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .Select(u => new { u.Id, u.Nome, u.Email, u.Telefone, u.Tipo, u.Ativo, u.CriadoEm })
            .ToListAsync(ct);

        var itens = registros
            .Select(u => new UsuarioListaDto(
                u.Id,
                u.Nome,
                u.Email,
                u.Telefone,
                Perfis.De(u.Tipo),
                u.Ativo,
                u.CriadoEm))
            .ToList();

        var totalPaginas = (int)Math.Ceiling(total / (double)tamanho);

        return Ok(new PaginaDto<UsuarioListaDto>(itens, pagina, tamanho, totalPaginas, total));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UsuarioListaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioListaDto>> Obter(int id, CancellationToken ct)
    {
        var usuario = await _db.Usuarios
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new { u.Id, u.Nome, u.Email, u.Telefone, u.Tipo, u.Ativo, u.CriadoEm })
            .FirstOrDefaultAsync(ct);

        if (usuario is null)
            return NotFound(new { erro = "Usuário não encontrado." });

        return Ok(new UsuarioListaDto(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Telefone,
            Perfis.De(usuario.Tipo),
            usuario.Ativo,
            usuario.CriadoEm));
    }

    /// <summary>Cria Técnico ou Administrador. Cliente é criado por /api/clientes.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioListaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsuarioListaDto>> Criar([FromBody] NovoUsuarioEquipeDto dto, CancellationToken ct)
    {
        if (dto.Tipo == TipoUsuario.Cliente)
            return BadRequest(new { erro = "Use POST /api/clientes para cadastrar clientes." });

        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _db.Usuarios.AnyAsync(u => u.Email == email, ct))
            return BadRequest(new { erro = "E-mail já cadastrado." });

        var usuario = new Usuario
        {
            Nome = dto.Nome.Trim(),
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            Telefone = string.IsNullOrWhiteSpace(dto.Telefone) ? null : dto.Telefone.Trim(),
            Tipo = dto.Tipo,
            Ativo = true
        };

        _db.Usuarios.Add(usuario);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { erro = "E-mail já cadastrado." });
        }

        var resposta = new UsuarioListaDto(
            usuario.Id, usuario.Nome, usuario.Email, usuario.Telefone,
            Perfis.De(usuario.Tipo), usuario.Ativo, usuario.CriadoEm);

        return CreatedAtAction(nameof(Obter), new { id = usuario.Id }, resposta);
    }

    /// <summary>Troca o perfil (papel) do usuário. Revoga as sessões dele.</summary>
    [HttpPut("{id:int}/perfil")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarPerfil(int id, [FromBody] AlterarPerfilDto dto, CancellationToken ct)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (usuario is null)
            return NotFound(new { erro = "Usuário não encontrado." });

        if (usuario.Id == _usuarioAtual.Id)
            return BadRequest(new { erro = "Você não pode alterar o seu próprio perfil." });

        if (usuario.Tipo == dto.Tipo)
            return Ok(new { mensagem = "O usuário já possui este perfil." });

        // Promover cliente para Técnico/Administrador é válido; o inverso não,
        // porque o cadastro de cliente exige CPF/CNPJ e endereço.
        if (dto.Tipo == TipoUsuario.Cliente)
            return BadRequest(new
            {
                erro = "Não é possível transformar um usuário da equipe em cliente. " +
                        "Cadastre o cliente por POST /api/clientes."
            });

        if (usuario.Tipo == TipoUsuario.Administrador)
        {
            var admins = await ContarAdminsAtivosAsync(ct);
            if (admins <= 1)
                return BadRequest(new { erro = "É necessário manter pelo menos um administrador ativo." });
        }

        usuario.Tipo = dto.Tipo;
        usuario.AtualizadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        // O perfil fica gravado no token: sem revogar, o token antigo manteria o papel velho.
        await _autenticacao.RevogarTodosAsync(usuario.Id, ct);

        return Ok(new { mensagem = "Perfil atualizado. O usuário deve entrar novamente." });
    }

    /// <summary>Ativa/desativa o usuário. Desativado perde acesso e sessões.</summary>
    [HttpPut("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarStatus(int id, [FromBody] AlterarStatusDto dto, CancellationToken ct)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (usuario is null)
            return NotFound(new { erro = "Usuário não encontrado." });

        if (usuario.Id == _usuarioAtual.Id && dto.Ativo is false)
            return BadRequest(new { erro = "Você não pode desativar o seu próprio usuário." });

        var ativo = dto.Ativo!.Value;

        if (!ativo && usuario.Tipo == TipoUsuario.Administrador)
        {
            var admins = await ContarAdminsAtivosAsync(ct);
            if (admins <= 1)
                return BadRequest(new { erro = "É necessário manter pelo menos um administrador ativo." });
        }

        usuario.Ativo = ativo;
        usuario.AtualizadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        if (!ativo)
            await _autenticacao.RevogarTodosAsync(usuario.Id, ct);

        return Ok(new { mensagem = ativo ? "Usuário ativado." : "Usuário desativado." });
    }

    /// <summary>Exclusão definitiva. Clientes devem ser removidos por /api/clientes/{id}.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (usuario is null)
            return NotFound(new { erro = "Usuário não encontrado." });

        if (usuario.Id == _usuarioAtual.Id)
            return BadRequest(new { erro = "Você não pode remover o seu próprio usuário." });

        if (usuario.Tipo == TipoUsuario.Cliente)
            return BadRequest(new { erro = "Use DELETE /api/clientes/{id} para remover clientes." });

        if (usuario.Tipo == TipoUsuario.Administrador)
        {
            var admins = await ContarAdminsAtivosAsync(ct);
            if (admins <= 1)
                return BadRequest(new { erro = "É necessário manter pelo menos um administrador ativo." });
        }

        await _db.RefreshTokens.Where(t => t.UsuarioId == id).ExecuteDeleteAsync(ct);
        _db.Usuarios.Remove(usuario);
        await _db.SaveChangesAsync(ct);

        return Ok(new { mensagem = "Usuário removido." });
    }

    private Task<int> ContarAdminsAtivosAsync(CancellationToken ct) =>
        _db.Usuarios.CountAsync(u => u.Tipo == TipoUsuario.Administrador && u.Ativo, ct);
}
