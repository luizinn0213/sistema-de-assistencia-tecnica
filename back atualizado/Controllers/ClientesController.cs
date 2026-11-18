using Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/clientes")]
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IUsuarioAtual _usuarioAtual;
    private readonly IAutenticacaoService _autenticacao;

    public ClientesController(AppDbContext db, IUsuarioAtual usuarioAtual, IAutenticacaoService autenticacao)
    {
        _db = db;
        _usuarioAtual = usuarioAtual;
        _autenticacao = autenticacao;
    }

    // ============================================================
    // CADASTRO PÚBLICO (auto-serviço do cliente)
    // ============================================================
    /// <summary>
    /// Cadastra usuário + cliente + endereço em uma única transação e já
    /// devolve os tokens (o cliente entra logado).
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(Politicas.RateLimitAuth)]
    [ProducesResponseType(typeof(RegistroClienteRespostaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegistroClienteRespostaDto>> Cadastrar(
        [FromBody] RegistroClienteDto dto, CancellationToken ct)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var documento = SomenteDigitos(dto.CpfCnpj);

        if (documento.Length is not (11 or 14))
            return BadRequest(new { erro = "CPF deve ter 11 dígitos ou CNPJ 14 dígitos." });

        if (await _db.Usuarios.AnyAsync(u => u.Email == email, ct))
            return BadRequest(new { erro = "E-mail já cadastrado." });

        if (await _db.Clientes.AnyAsync(c => c.CpfCnpj == documento, ct))
            return BadRequest(new { erro = "CPF/CNPJ já cadastrado." });

        var usuario = new Usuario
        {
            Nome = dto.Nome.Trim(),
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            Telefone = NormalizarOpcional(dto.Telefone),
            Tipo = TipoUsuario.Cliente,
            Ativo = true
        };

        var cliente = new Cliente
        {
            Usuario = usuario,
            CpfCnpj = documento,
            Endereco = dto.Endereco is null ? null : MontarEndereco(dto.Endereco)
        };

        _db.Clientes.Add(cliente);

        try
        {
            // Um único SaveChanges = usuário + cliente + endereço na mesma transação.
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Corrida entre duas requisições simultâneas com o mesmo e-mail/documento.
            return Conflict(new { erro = "E-mail ou CPF/CNPJ já cadastrado." });
        }

        var tokens = await _autenticacao.EmitirTokensAsync(usuario, IpDoCliente(), ct);

        return CreatedAtAction(
            nameof(Obter),
            new { id = cliente.Id },
            new RegistroClienteRespostaDto(cliente.Id, "Cliente cadastrado.", tokens));
    }

    // ============================================================
    // PERFIL DO PRÓPRIO CLIENTE (id vem do token)
    // ============================================================
    [HttpGet("me")]
    [Authorize(Policy = Politicas.SomenteCliente)]
    [ProducesResponseType(typeof(ClienteDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDetalheDto>> MeuPerfil(CancellationToken ct)
    {
        var clienteId = await ClienteDoTokenAsync(ct);
        if (clienteId is null)
            return NotFound(new { erro = "Nenhum cliente vinculado ao usuário autenticado." });

        var cliente = await DetalheAsync(clienteId.Value, ct);
        if (cliente is null)
            return NotFound(new { erro = "Cliente não encontrado." });

        return Ok(cliente);
    }

    [HttpPut("me")]
    [Authorize(Policy = Politicas.SomenteCliente)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarMeuPerfil([FromBody] ClienteUpdateDto dto, CancellationToken ct)
    {
        var clienteId = await ClienteDoTokenAsync(ct);
        if (clienteId is null)
            return NotFound(new { erro = "Nenhum cliente vinculado ao usuário autenticado." });

        return await AtualizarAsync(clienteId.Value, dto, ct);
    }

    // ============================================================
    // EQUIPE INTERNA (Técnico / Administrador)
    // ============================================================
    [HttpGet]
    [Authorize(Policy = Politicas.EquipeTecnica)]
    [ProducesResponseType(typeof(PaginaDto<ClienteResumoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaDto<ClienteResumoDto>>> Listar(
        [FromQuery] string? busca,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken ct = default)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanho = Math.Clamp(tamanho, 1, 100);

        var consulta = _db.Clientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            var digitos = SomenteDigitos(termo);

            // EF.Functions.Like aproveita o índice e ignora maiúsculas/minúsculas.
            consulta = consulta.Where(c =>
                EF.Functions.Like(c.Usuario.Nome, "%" + termo + "%") ||
                EF.Functions.Like(c.Usuario.Email, "%" + termo + "%") ||
                (digitos != "" && EF.Functions.Like(c.CpfCnpj, "%" + digitos + "%")));
        }

        var total = await consulta.CountAsync(ct);

        var itens = await consulta
            .OrderBy(c => c.Usuario.Nome)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .Select(c => new ClienteResumoDto(
                c.Id,
                c.Usuario.Nome,
                c.Usuario.Email,
                c.Usuario.Telefone,
                c.CpfCnpj,
                c.Usuario.Ativo))
            .ToListAsync(ct);

        var totalPaginas = (int)Math.Ceiling(total / (double)tamanho);

        return Ok(new PaginaDto<ClienteResumoDto>(itens, pagina, tamanho, totalPaginas, total));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Politicas.EquipeTecnica)]
    [ProducesResponseType(typeof(ClienteDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDetalheDto>> Obter(int id, CancellationToken ct)
    {
        var cliente = await DetalheAsync(id, ct);
        if (cliente is null)
            return NotFound(new { erro = "Cliente não encontrado." });

        return Ok(cliente);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.EquipeTecnica)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ClienteUpdateDto dto, CancellationToken ct) =>
        await AtualizarAsync(id, dto, ct);

    /// <summary>Remove o cliente e todos os dados vinculados (somente Admin).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.SomenteAdministrador)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        var usuarioId = await _db.Clientes
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => (int?)c.UsuarioId)
            .FirstOrDefaultAsync(ct);

        if (usuarioId is null)
            return NotFound(new { erro = "Cliente não encontrado." });

        if (usuarioId == _usuarioAtual.Id)
            return BadRequest(new { erro = "Não é possível remover o próprio usuário." });

        // Deletes diretos no banco: sem carregar entidades, sem N+1.
        await using var transacao = await _db.Database.BeginTransactionAsync(ct);

        await _db.Equipamentos.Where(e => e.ClienteId == id).ExecuteDeleteAsync(ct);
        await _db.Enderecos.Where(e => e.ClienteId == id).ExecuteDeleteAsync(ct);
        await _db.Clientes.Where(c => c.Id == id).ExecuteDeleteAsync(ct);
        await _db.RefreshTokens.Where(t => t.UsuarioId == usuarioId.Value).ExecuteDeleteAsync(ct);
        await _db.Usuarios.Where(u => u.Id == usuarioId.Value).ExecuteDeleteAsync(ct);

        await transacao.CommitAsync(ct);

        return Ok(new { mensagem = "Cliente e dados vinculados removidos." });
    }

    // ============================================================
    // AUXILIARES
    // ============================================================
    /// <summary>
    /// Resolve o cliente do usuário autenticado: primeiro pela claim do token,
    /// só cai no banco se o token não tiver a claim.
    /// </summary>
    private async Task<int?> ClienteDoTokenAsync(CancellationToken ct)
    {
        if (_usuarioAtual.ClienteId is int clienteId)
            return clienteId;

        var usuarioId = _usuarioAtual.Id;
        return usuarioId is null
            ? null
            : await _autenticacao.ObterClienteIdAsync(usuarioId.Value, ct);
    }

    private async Task<IActionResult> AtualizarAsync(int clienteId, ClienteUpdateDto dto, CancellationToken ct)
    {
        var cliente = await _db.Clientes
            .Include(c => c.Usuario)
            .Include(c => c.Endereco)
            .FirstOrDefaultAsync(c => c.Id == clienteId, ct);

        if (cliente is null)
            return NotFound(new { erro = "Cliente não encontrado." });

        cliente.Usuario.Nome = dto.Nome.Trim();
        cliente.Usuario.Telefone = NormalizarOpcional(dto.Telefone);
        cliente.Usuario.AtualizadoEm = DateTime.UtcNow;

        if (dto.Endereco is not null)
        {
            var endereco = cliente.Endereco;

            if (endereco is null)
            {
                endereco = new Endereco { ClienteId = cliente.Id };
                cliente.Endereco = endereco;
                _db.Enderecos.Add(endereco);
            }

            CopiarEndereco(endereco, dto.Endereco);
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new { mensagem = "Dados atualizados." });
    }

    /// <summary>Consulta enxuta: uma query para o cliente e outra para o endereço.</summary>
    private async Task<ClienteDetalheDto?> DetalheAsync(int clienteId, CancellationToken ct)
    {
        var dados = await _db.Clientes
            .AsNoTracking()
            .Where(c => c.Id == clienteId)
            .Select(c => new
            {
                c.Id,
                Nome = c.Usuario.Nome,
                Email = c.Usuario.Email,
                Telefone = c.Usuario.Telefone,
                c.CpfCnpj,
                Ativo = c.Usuario.Ativo,
                c.CriadoEm
            })
            .FirstOrDefaultAsync(ct);

        if (dados is null)
            return null;

        var endereco = await _db.Enderecos
            .AsNoTracking()
            .Where(e => e.ClienteId == clienteId)
            .Select(e => new EnderecoRespostaDto(
                e.Cep,
                e.Logradouro,
                e.Numero,
                e.Bairro,
                e.Cidade,
                e.Estado))
            .FirstOrDefaultAsync(ct);

        return new ClienteDetalheDto(
            dados.Id,
            dados.Nome,
            dados.Email,
            dados.Telefone,
            dados.CpfCnpj,
            dados.Ativo,
            dados.CriadoEm,
            endereco);
    }

    private string? IpDoCliente() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private static string SomenteDigitos(string texto) =>
        new(texto.Where(char.IsDigit).ToArray());

    private static string? NormalizarOpcional(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private static Endereco MontarEndereco(EnderecoDto dto) => new()
    {
        Cep = SomenteDigitos(dto.Cep),
        Logradouro = dto.Logradouro.Trim(),
        Numero = dto.Numero.Trim(),
        Bairro = dto.Bairro.Trim(),
        Cidade = dto.Cidade.Trim(),
        Estado = dto.Estado.Trim().ToUpperInvariant()
    };

    private static void CopiarEndereco(Endereco destino, EnderecoDto dto)
    {
        destino.Cep = SomenteDigitos(dto.Cep);
        destino.Logradouro = dto.Logradouro.Trim();
        destino.Numero = dto.Numero.Trim();
        destino.Bairro = dto.Bairro.Trim();
        destino.Cidade = dto.Cidade.Trim();
        destino.Estado = dto.Estado.Trim().ToUpperInvariant();
    }
}
