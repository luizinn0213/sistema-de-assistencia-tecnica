using Assistencia.Api.Data;
using Assistencia.Api.Dtos;
using Assistencia.Api.Enums;
using Assistencia.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Controllers;

[ApiController]
[Route("api/orcamentos")]
public class OrcamentosController : ControllerBase
{
    private readonly AppDbContext _context;

    private const decimal TaxaPlataformaPercentual = 10.0m;

    public OrcamentosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> Listar()
    {
        var orcamentos = await _context.Orcamentos
            .Include(o => o.Itens)
            .Include(o => o.Historico)
            .Include(o => o.Pagamento)
            .Include(o => o.Comissao)
            .AsNoTracking()
            .OrderByDescending(o => o.DataCriacao)
            .ToListAsync();

        return Ok(orcamentos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> BuscarPorId(int id)
    {
        var orcamento = await _context.Orcamentos
            .Include(o => o.Itens)
            .Include(o => o.Historico)
            .Include(o => o.Pagamento)
            .Include(o => o.Comissao)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orcamento is null)
            return NotFound("Orçamento não encontrado.");

        return Ok(orcamento);
    }

    [HttpPost]
    public async Task<ActionResult> Criar(
        CriarOrcamentoDto dados)
    {
        if (!dados.DiagnosticoConcluido)
        {
            return BadRequest(
                "Não é possível criar um orçamento sem diagnóstico concluído.");
        }

        if (dados.Itens is null || dados.Itens.Count == 0)
        {
            return BadRequest(
                "O orçamento precisa possuir pelo menos um item.");
        }

        var orcamento = new Orcamento
        {
            DiagnosticoId = dados.DiagnosticoId,
            DiagnosticoConcluido =
                dados.DiagnosticoConcluido,
            Versao = 1,
            Status = StatusOrcamento.Pendente,
            DataCriacao = DateTime.UtcNow
        };

        foreach (var dadosItem in dados.Itens)
        {
            var item = CriarItem(dadosItem);
            orcamento.Itens.Add(item);
        }

        orcamento.ValorTotal = orcamento.Itens
            .Sum(item => item.ValorTotal);

        orcamento.Historico.Add(
            new HistoricoOrcamento
            {
                Versao = orcamento.Versao,
                ValorTotal = orcamento.ValorTotal,
                Status = StatusOrcamento.Pendente,
                DataRegistro = DateTime.UtcNow,
                Obs = "Orçamento inicial criado."
            });

        _context.Orcamentos.Add(orcamento);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(BuscarPorId),
            new { id = orcamento.Id },
            orcamento);
    }

    [HttpPut("{id:int}/nova-versao")]
    public async Task<ActionResult> CriarNovaVersao(
        int id,
        List<CriarItemDto> novosItens)
    {
        if (novosItens is null || novosItens.Count == 0)
        {
            return BadRequest(
                "A nova versão precisa possuir pelo menos um item.");
        }

        var orcamento = await _context.Orcamentos
            .Include(o => o.Itens)
            .Include(o => o.Historico)
            .Include(o => o.Pagamento)
            .Include(o => o.Comissao)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orcamento is null)
            return NotFound("Orçamento não encontrado.");

        if (orcamento.Status == StatusOrcamento.Cancelado)
        {
            return BadRequest(
                "Não é possível alterar um orçamento cancelado.");
        }

        if (orcamento.Pagamento is not null)
        {
            return BadRequest(
                "Não é possível alterar um orçamento que já foi pago.");
        }

        _context.ItensOrcamento.RemoveRange(
            orcamento.Itens);

        orcamento.Itens.Clear();

        foreach (var dadosItem in novosItens)
        {
            var item = CriarItem(dadosItem);
            orcamento.Itens.Add(item);
        }

        if (orcamento.Comissao is not null)
        {
            _context.Comissoes.Remove(
                orcamento.Comissao);

            orcamento.Comissao = null;
        }

        orcamento.Versao++;
        orcamento.Status = StatusOrcamento.Pendente;
        orcamento.DataResposta = null;

        orcamento.ValorTotal = orcamento.Itens
            .Sum(item => item.ValorTotal);

        orcamento.Historico.Add(
            new HistoricoOrcamento
            {
                Versao = orcamento.Versao,
                ValorTotal = orcamento.ValorTotal,
                Status = StatusOrcamento.Pendente,
                DataRegistro = DateTime.UtcNow,
                Obs = $"Versão {orcamento.Versao} criada."
            });

        await _context.SaveChangesAsync();

        return Ok(orcamento);
    }

    [HttpPost("{id:int}/resposta")]
    public async Task<ActionResult> ResponderCliente(
        int id,
        RespostaClienteDto dados)
    {
        var orcamento = await _context.Orcamentos
            .Include(o => o.Historico)
            .Include(o => o.Comissao)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orcamento is null)
            return NotFound("Orçamento não encontrado.");

        if (orcamento.Status != StatusOrcamento.Pendente)
        {
            return BadRequest(
                "Somente um orçamento pendente pode receber uma resposta.");
        }

        orcamento.DataResposta = DateTime.UtcNow;

        orcamento.Status = dados.Aprovado
            ? StatusOrcamento.Aprovado
            : StatusOrcamento.Rejeitado;

        orcamento.Historico.Add(
            new HistoricoOrcamento
            {
                Versao = orcamento.Versao,
                ValorTotal = orcamento.ValorTotal,
                Status = orcamento.Status,
                DataRegistro = DateTime.UtcNow,
                Obs = dados.Aprovado
                    ? "Orçamento aprovado pelo cliente."
                    : $"Orçamento rejeitado: {dados.Obs}"
            });

        if (dados.Aprovado)
        {
            var valorComissao =
                orcamento.ValorTotal *
                TaxaPlataformaPercentual / 100;

            orcamento.Comissao = new Comissao
            {
                Porcentagem =
                    TaxaPlataformaPercentual,
                ValorComissao = valorComissao,
                DataCalculo = DateTime.UtcNow
            };
        }

        await _context.SaveChangesAsync();

        return Ok(orcamento);
    }

    [HttpPost("{id:int}/pagamento-simulado")]
    public async Task<ActionResult> SimularPagamento(
        int id,
        SimularPagamentoDto dados)
    {
        var orcamento = await _context.Orcamentos
            .Include(o => o.Pagamento)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orcamento is null)
            return NotFound("Orçamento não encontrado.");

        if (orcamento.Status != StatusOrcamento.Aprovado)
        {
            return BadRequest(
                "Somente um orçamento aprovado pode ser pago.");
        }

        if (orcamento.Pagamento is not null)
        {
            return Conflict(
                "Esse orçamento já possui um pagamento.");
        }

        var formasPermitidas = new[]
        {
            "Pix",
            "Cartao",
            "Boleto"
        };

        var formaPagamento = formasPermitidas
            .FirstOrDefault(forma =>
                string.Equals(
                    forma,
                    dados.FormaPagamento.Trim(),
                    StringComparison.OrdinalIgnoreCase));

        if (formaPagamento is null)
        {
            return BadRequest(
                "A forma de pagamento deve ser Pix, Cartao ou Boleto.");
        }

        var pagamento = new Pagamento
        {
            Valor = orcamento.ValorTotal,
            FormaPagamento = formaPagamento,
            Status = StatusPagamento.Aprovado,
            DataPagamento = DateTime.UtcNow
        };

        orcamento.Pagamento = pagamento;

        await _context.SaveChangesAsync();

        return Ok(pagamento);
    }

    private static ItemOrcamento CriarItem(
        CriarItemDto dados)
    {
        return new ItemOrcamento
        {
            Descricao = dados.Descricao.Trim(),
            Tipo = dados.Tipo,
            Quantidade = dados.Quantidade,
            ValorUnitario = dados.ValorUnitario
        };
    }
}