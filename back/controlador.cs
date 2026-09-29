using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api;

// ============================================================
// AUTENTICAÇÃO
// ============================================================
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    public AuthController(AppDbContext db) => _db = db;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Senha, user.SenhaHash))
            return BadRequest(new { erro = "E-mail ou senha inválidos." });

        return Ok(new
        {
            usuarioId = user.Id,
            nome = user.Nome,
            email = user.Email,
            tipo = user.Tipo.ToString()
        });
    }
}

public record LoginDto(string Email, string Senha);

// ============================================================
// CLIENTES
// ============================================================
[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ClientesController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Cadastrar([FromBody] ClienteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@"))
            return BadRequest(new { erro = "E-mail inválido." });

        if (string.IsNullOrWhiteSpace(dto.Senha) || dto.Senha.Length < 6)
            return BadRequest(new { erro = "Senha deve ter ao menos 6 caracteres." });

        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email.ToLower()))
            return BadRequest(new { erro = "E-mail já cadastrado." });

        var doc = new string(dto.CpfCnpj.Where(char.IsDigit).ToArray());
        if (doc.Length < 11)
            return BadRequest(new { erro = "CPF/CNPJ inválido." });

        if (await _db.Clientes.AnyAsync(c => c.CpfCnpj == doc))
            return BadRequest(new { erro = "CPF/CNPJ já cadastrado." });

        var usuario = new Usuario
        {
            Nome = dto.Nome,
            Email = dto.Email.ToLower(),
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            Telefone = dto.Telefone,
            Tipo = TipoUsuario.Cliente
        };
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        var cliente = new Cliente { UsuarioId = usuario.Id, CpfCnpj = doc };
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();

        if (dto.Endereco != null)
        {
            _db.Enderecos.Add(new Endereco
            {
                ClienteId = cliente.Id,
                Cep = dto.Endereco.Cep,
                Logradouro = dto.Endereco.Logradouro,
                Numero = dto.Endereco.Numero,
                Bairro = dto.Endereco.Bairro,
                Cidade = dto.Endereco.Cidade,
                Estado = dto.Endereco.Estado
            });
            await _db.SaveChangesAsync();
        }

        return Ok(new { id = cliente.Id, mensagem = "Cliente cadastrado." });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(int id)
    {
        var c = await _db.Clientes
            .Include(x => x.Usuario)
            .Include(x => x.Endereco)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (c == null) return NotFound(new { erro = "Cliente não encontrado." });

        return Ok(new
        {
            id = c.Id,
            nome = c.Usuario.Nome,
            email = c.Usuario.Email,
            telefone = c.Usuario.Telefone,
            cpfCnpj = c.CpfCnpj,
            endereco = c.Endereco == null ? null : new
            {
                cep = c.Endereco.Cep,
                logradouro = c.Endereco.Logradouro,
                numero = c.Endereco.Numero,
                bairro = c.Endereco.Bairro,
                cidade = c.Endereco.Cidade,
                estado = c.Endereco.Estado
            }
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ClienteUpdateDto dto)
    {
        var c = await _db.Clientes
            .Include(x => x.Usuario)
            .Include(x => x.Endereco)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (c == null) return NotFound(new { erro = "Cliente não encontrado." });

        c.Usuario.Nome = dto.Nome;
        c.Usuario.Telefone = dto.Telefone;

        if (dto.Endereco != null)
        {
            if (c.Endereco == null)
            {
                c.Endereco = new Endereco { ClienteId = c.Id };
                _db.Enderecos.Add(c.Endereco);
            }
            c.Endereco.Cep = dto.Endereco.Cep;
            c.Endereco.Logradouro = dto.Endereco.Logradouro;
            c.Endereco.Numero = dto.Endereco.Numero;
            c.Endereco.Bairro = dto.Endereco.Bairro;
            c.Endereco.Cidade = dto.Endereco.Cidade;
            c.Endereco.Estado = dto.Endereco.Estado;
        }

        await _db.SaveChangesAsync();
        return Ok(new { mensagem = "Dados atualizados." });
    }
}

public record EnderecoDto(string Cep, string Logradouro, string Numero,
    string Bairro, string Cidade, string Estado);

public record ClienteDto(string Nome, string Email, string Senha,
    string? Telefone, string CpfCnpj, EnderecoDto? Endereco);

public record ClienteUpdateDto(string Nome, string? Telefone, EnderecoDto? Endereco);

// ============================================================
// EQUIPAMENTOS
// ============================================================
[ApiController]
[Route("api/equipamentos")]
public class EquipamentosController : ControllerBase
{
    private readonly AppDbContext _db;
    public EquipamentosController(AppDbContext db) => _db = db;

    [HttpGet("cliente/{clienteId}")]
    public async Task<IActionResult> Listar(int clienteId)
    {
        var lista = await _db.Equipamentos
            .Where(e => e.ClienteId == clienteId)
            .Select(e => new
            {
                id = e.Id,
                tipo = e.Tipo,
                marca = e.Marca,
                modelo = e.Modelo,
                numeroSerie = e.NumeroSerie
            })
            .ToListAsync();

        return Ok(lista);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(int id)
    {
        var e = await _db.Equipamentos.FindAsync(id);
        if (e == null) return NotFound(new { erro = "Equipamento não encontrado." });

        return Ok(new
        {
            id = e.Id,
            clienteId = e.ClienteId,
            tipo = e.Tipo,
            marca = e.Marca,
            modelo = e.Modelo,
            numeroSerie = e.NumeroSerie
        });
    }

    [HttpPost("cliente/{clienteId}")]
    public async Task<IActionResult> Criar(int clienteId, [FromBody] EquipamentoDto dto)
    {
        if (!await _db.Clientes.AnyAsync(c => c.Id == clienteId))
            return NotFound(new { erro = "Cliente não encontrado." });

        if (string.IsNullOrWhiteSpace(dto.NumeroSerie) || dto.NumeroSerie.Length < 4)
            return BadRequest(new { erro = "Número de série inválido." });

        if (await _db.Equipamentos.AnyAsync(e => e.NumeroSerie == dto.NumeroSerie))
            return BadRequest(new { erro = "Número de série já cadastrado." });

        var eq = new Equipamento
        {
            ClienteId = clienteId,
            Tipo = dto.Tipo,
            Marca = dto.Marca,
            Modelo = dto.Modelo,
            NumeroSerie = dto.NumeroSerie
        };
        _db.Equipamentos.Add(eq);
        await _db.SaveChangesAsync();

        return Ok(new { id = eq.Id, mensagem = "Equipamento cadastrado." });
    }

    [HttpGet("{id}/historico")]
    public IActionResult Historico(int id)
    {
        // Vazio — integração futura com módulo de solicitações (Pessoa 3)
        return Ok(new List<object>());
    }
}

public record EquipamentoDto(string Tipo, string Marca, string Modelo, string NumeroSerie);