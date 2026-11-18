using Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/equipamentos")]
[Produces("application/json")]
public class EquipamentosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IUsuarioAtual _usuarioAtual;
    private readonly IAutenticacaoService _autenticacao;

    public EquipamentosController(AppDbContext db, IUsuarioAtual usuarioAtual, IAutenticacaoService autenticacao)
    {
        _db = db;
        _usuarioAtual = usuarioAtual;
        _autenticacao = autenticacao;
    }

    // ============================================================
    // CLIENTE — vê e cadastra apenas os próprios equipamentos
    // ============================================================
    [HttpGet("meus")]
    [Authorize(Policy = Politicas.SomenteCliente)]
    [ProducesResponseType(typeof(List<EquipamentoRespostaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<EquipamentoRespostaDto>>> Meus(CancellationToken ct)
    {
        var clienteId = await ClienteDoTokenAsync(ct);
        if (clienteId is null)
            return NotFound(new { erro = "Nenhum cliente vinculado ao usuário autenticado." });

        return Ok(await ListarDoClienteAsync(clienteId.Value, ct));
    }

    /// <summary>O cliente cadastra o equipamento no próprio perfil (sem enviar clienteId).</summary>
    [HttpPost]
    [Authorize(Policy = Politicas.SomenteCliente)]
    [ProducesResponseType(typeof(EquipamentoRespostaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EquipamentoRespostaDto>> Criar(
        [FromBody] EquipamentoDto dto, CancellationToken ct)
    {
        var clienteId = await ClienteDoTokenAsync(ct);
        if (clienteId is null)
            return NotFound(new { erro = "Nenhum cliente vinculado ao usuário autenticado." });

        return await CriarParaClienteAsync(clienteId.Value, dto, ct);
    }

    // ============================================================
    // EQUIPE INTERNA
    // ============================================================
    [HttpGet]
    [Authorize(Policy = Politicas.EquipeTecnica)]
    [ProducesResponseType(typeof(PaginaDto<EquipamentoRespostaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaDto<EquipamentoRespostaDto>>> Listar(
        [FromQuery] int? clienteId,
        [FromQuery] string? busca,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken ct = default)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanho = Math.Clamp(tamanho, 1, 100);

        var consulta = _db.Equipamentos.AsNoTracking();

        if (clienteId is int filtroCliente)
            consulta = consulta.Where(e => e.ClienteId == filtroCliente);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            consulta = consulta.Where(e =>
                EF.Functions.Like(e.NumeroSerie, "%" + termo + "%") ||
                EF.Functions.Like(e.Marca, "%" + termo + "%") ||
                EF.Functions.Like(e.Modelo, "%" + termo + "%") ||
                EF.Functions.Like(e.Tipo, "%" + termo + "%"));
        }

        var total = await consulta.CountAsync(ct);

        var itens = await consulta
            .OrderBy(e => e.Id)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .Select(e => new EquipamentoRespostaDto(
                e.Id,
                e.ClienteId,
                e.Tipo,
                e.Marca,
                e.Modelo,
                e.NumeroSerie,
                e.CriadoEm))
            .ToListAsync(ct);

        var totalPaginas = (int)Math.Ceiling(total / (double)tamanho);

        return Ok(new PaginaDto<EquipamentoRespostaDto>(itens, pagina, tamanho, totalPaginas, total));
    }

    /// <summary>Cadastro feito pela equipe para um cliente específico.</summary>
    [HttpPost("cliente/{clienteId:int}")]
    [Authorize(Policy = Politicas.EquipeTecnica)]
    [ProducesResponseType(typeof(EquipamentoRespostaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipamentoRespostaDto>> CriarParaCliente(
        int clienteId, [FromBody] EquipamentoDto dto, CancellationToken ct) =>
        await CriarParaClienteAsync(clienteId, dto, ct);

    // ============================================================
    // DONO DO EQUIPAMENTO OU EQUIPE INTERNA
    // ============================================================
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EquipamentoRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipamentoRespostaDto>> Obter(int id, CancellationToken ct)
    {
        var equipamento = await _db.Equipamentos
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EquipamentoRespostaDto(
                e.Id,
                e.ClienteId,
                e.Tipo,
                e.Marca,
                e.Modelo,
                e.NumeroSerie,
                e.CriadoEm))
            .FirstOrDefaultAsync(ct);

        if (equipamento is null)
            return NotFound(new { erro = "Equipamento não encontrado." });

        if (!await PodeAcessarAsync(equipamento.ClienteId, ct))
            return Forbid();

        return Ok(equipamento);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] EquipamentoDto dto, CancellationToken ct)
    {
        var equipamento = await _db.Equipamentos.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (equipamento is null)
            return NotFound(new { erro = "Equipamento não encontrado." });

        if (!await PodeAcessarAsync(equipamento.ClienteId, ct))
            return Forbid();

        var numeroSerie = dto.NumeroSerie.Trim().ToUpperInvariant();

        if (await _db.Equipamentos.AnyAsync(e => e.Id != id && e.NumeroSerie == numeroSerie, ct))
            return BadRequest(new { erro = "Número de série já cadastrado em outro equipamento." });

        equipamento.Tipo = dto.Tipo.Trim();
        equipamento.Marca = dto.Marca.Trim();
        equipamento.Modelo = dto.Modelo.Trim();
        equipamento.NumeroSerie = numeroSerie;

        await _db.SaveChangesAsync(ct);
        return Ok(new { mensagem = "Equipamento atualizado." });
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        var equipamento = await _db.Equipamentos.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (equipamento is null)
            return NotFound(new { erro = "Equipamento não encontrado." });

        if (!await PodeAcessarAsync(equipamento.ClienteId, ct))
            return Forbid();

        _db.Equipamentos.Remove(equipamento);
        await _db.SaveChangesAsync(ct);

        return Ok(new { mensagem = "Equipamento removido." });
    }

    /// <summary>Histórico de atendimentos — será alimentado pelo módulo de solicitações.</summary>
    [HttpGet("{id:int}/historico")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Historico(int id, CancellationToken ct)
    {
        var clienteId = await _db.Equipamentos
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => (int?)e.ClienteId)
            .FirstOrDefaultAsync(ct);

        if (clienteId is null)
            return NotFound(new { erro = "Equipamento não encontrado." });

        if (!await PodeAcessarAsync(clienteId.Value, ct))
            return Forbid();

        return Ok(Array.Empty<object>());
    }

    // ============================================================
    // AUXILIARES
    // ============================================================
    private async Task<ActionResult<EquipamentoRespostaDto>> CriarParaClienteAsync(
        int clienteId, EquipamentoDto dto, CancellationToken ct)
    {
        if (!await _db.Clientes.AnyAsync(c => c.Id == clienteId, ct))
            return NotFound(new { erro = "Cliente não encontrado." });

        var numeroSerie = dto.NumeroSerie.Trim().ToUpperInvariant();

        if (await _db.Equipamentos.AnyAsync(e => e.NumeroSerie == numeroSerie, ct))
            return BadRequest(new { erro = "Número de série já cadastrado." });

        var equipamento = new Equipamento
        {
            ClienteId = clienteId,
            Tipo = dto.Tipo.Trim(),
            Marca = dto.Marca.Trim(),
            Modelo = dto.Modelo.Trim(),
            NumeroSerie = numeroSerie
        };

        _db.Equipamentos.Add(equipamento);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { erro = "Número de série já cadastrado." });
        }

        var resposta = new EquipamentoRespostaDto(
            equipamento.Id,
            equipamento.ClienteId,
            equipamento.Tipo,
            equipamento.Marca,
            equipamento.Modelo,
            equipamento.NumeroSerie,
            equipamento.CriadoEm);

        return CreatedAtAction(nameof(Obter), new { id = equipamento.Id }, resposta);
    }

    private async Task<List<EquipamentoRespostaDto>> ListarDoClienteAsync(int clienteId, CancellationToken ct) =>
        await _db.Equipamentos
            .AsNoTracking()
            .Where(e => e.ClienteId == clienteId)
            .OrderBy(e => e.Id)
            .Select(e => new EquipamentoRespostaDto(
                e.Id,
                e.ClienteId,
                e.Tipo,
                e.Marca,
                e.Modelo,
                e.NumeroSerie,
                e.CriadoEm))
            .ToListAsync(ct);

    /// <summary>Equipe interna acessa tudo; cliente acessa somente o que é seu.</summary>
    private async Task<bool> PodeAcessarAsync(int clienteDonoId, CancellationToken ct)
    {
        if (_usuarioAtual.EhEquipe)
            return true;

        var clienteId = await ClienteDoTokenAsync(ct);
        return clienteId == clienteDonoId;
    }

    private async Task<int?> ClienteDoTokenAsync(CancellationToken ct)
    {
        if (_usuarioAtual.ClienteId is int clienteId)
            return clienteId;

        var usuarioId = _usuarioAtual.Id;
        return usuarioId is null
            ? null
            : await _autenticacao.ObterClienteIdAsync(usuarioId.Value, ct);
    }
}
