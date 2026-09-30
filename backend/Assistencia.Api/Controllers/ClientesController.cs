using Assistencia.Api.Data;
using Assistencia.Api.Dtos;
using Assistencia.Api.Enums;
using Assistencia.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClientesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> Obter(int id)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Usuario)
            .Include(c => c.Endereco)
            .Include(c => c.Equipamentos)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente is null)
        {
            return NotFound(
                new { erro = "Cliente nÃ£o encontrado." });
        }

        return Ok(new
        {
            cliente.Id,
            cliente.UsuarioId,
            Nome = cliente.Usuario!.Nome,
            Email = cliente.Usuario.Email,
            cliente.Usuario.Telefone,
            cliente.CpfCnpj,
            cliente.Endereco,
            cliente.Equipamentos
        });
    }

    [HttpPost]
    public async Task<ActionResult> Cadastrar(
        CriarClienteDto dados)
    {
        var email = dados.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) ||
            !email.Contains('@'))
        {
            return BadRequest(
                new { erro = "E-mail invÃ¡lido." });
        }

        if (string.IsNullOrWhiteSpace(dados.Senha) ||
            dados.Senha.Length < 6)
        {
            return BadRequest(new
            {
                erro =
                    "A senha deve possuir pelo menos 6 caracteres."
            });
        }

        var emailCadastrado = await _context.Usuarios
            .AnyAsync(u => u.Email == email);

        if (emailCadastrado)
        {
            return Conflict(
                new { erro = "E-mail jÃ¡ cadastrado." });
        }

        var documento = new string(
            dados.CpfCnpj
                .Where(char.IsDigit)
                .ToArray());

        if (documento.Length != 11 &&
            documento.Length != 14)
        {
            return BadRequest(
                new { erro = "CPF/CNPJ invÃ¡lido." });
        }

        var documentoCadastrado = await _context.Clientes
            .AnyAsync(c => c.CpfCnpj == documento);

        if (documentoCadastrado)
        {
            return Conflict(
                new { erro = "CPF/CNPJ jÃ¡ cadastrado." });
        }

        var usuario = new Usuario
        {
            Nome = dados.Nome.Trim(),
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(
                dados.Senha),
            Telefone = dados.Telefone?.Trim(),
            Tipo = TipoUsuario.Cliente
        };

        var cliente = new Cliente
        {
            Usuario = usuario,
            CpfCnpj = documento
        };

        if (dados.Endereco is not null)
        {
            cliente.Endereco = CriarEndereco(
                dados.Endereco);
        }

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(Obter),
            new { id = cliente.Id },
            new
            {
                cliente.Id,
                cliente.UsuarioId,
                usuario.Nome,
                usuario.Email,
                Mensagem = "Cliente cadastrado."
            });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Atualizar(
        int id,
        AtualizarClienteDto dados)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Usuario)
            .Include(c => c.Endereco)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente is null)
        {
            return NotFound(
                new { erro = "Cliente nÃ£o encontrado." });
        }

        if (string.IsNullOrWhiteSpace(dados.Nome))
        {
            return BadRequest(
                new { erro = "O nome Ã© obrigatÃ³rio." });
        }

        cliente.Usuario!.Nome = dados.Nome.Trim();
        cliente.Usuario.Telefone =
            dados.Telefone?.Trim();

        if (dados.Endereco is not null)
        {
            if (cliente.Endereco is null)
            {
                cliente.Endereco = CriarEndereco(
                    dados.Endereco);
            }
            else
            {
                AtualizarEndereco(
                    cliente.Endereco,
                    dados.Endereco);
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Mensagem = "Dados do cliente atualizados."
        });
    }

    private static Endereco CriarEndereco(
        EnderecoDto dados)
    {
        return new Endereco
        {
            Cep = dados.Cep.Trim(),
            Logradouro = dados.Logradouro.Trim(),
            Numero = dados.Numero.Trim(),
            Bairro = dados.Bairro.Trim(),
            Cidade = dados.Cidade.Trim(),
            Estado = dados.Estado.Trim().ToUpperInvariant()
        };
    }

    private static void AtualizarEndereco(
        Endereco endereco,
        EnderecoDto dados)
    {
        endereco.Cep = dados.Cep.Trim();
        endereco.Logradouro = dados.Logradouro.Trim();
        endereco.Numero = dados.Numero.Trim();
        endereco.Bairro = dados.Bairro.Trim();
        endereco.Cidade = dados.Cidade.Trim();
        endereco.Estado =
            dados.Estado.Trim().ToUpperInvariant();
    }
}
