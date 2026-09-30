using Manutencao.Application.DTOs.Requests;
using Manutencao.Application.DTOs.Responses;
using Manutencao.Domain.Entities;
using Manutencao.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Manutencao.Application.Services;

public class SolicitacaoService : ISolicitacaoService
{
    private readonly DbContext _context;

    public SolicitacaoService(DbContext context) => _context = context;

    public async Task<HistoricoResponseDto> RegistrarHistoricoAsync(RegistrarHistoricoRequestDto dto)
    {
        var solicitacao = await _context.Set<Solicitacao>()
            .FirstOrDefaultAsync(s => s.Id == dto.SolicitacaoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada.");

        var tecnico = await _context.Set<Tecnico>()
            .FirstOrDefaultAsync(t => t.Id == dto.TecnicoId)
            ?? throw new KeyNotFoundException("Técnico não encontrado.");

        if (solicitacao.Status == StatusSolicitacao.Finalizada)
            throw new InvalidOperationException("Não é possível registrar histórico em solicitação finalizada.");

        var historico = new HistoricoSolicitacao
        {
            SolicitacaoId = dto.SolicitacaoId,
            TecnicoId = dto.TecnicoId,
            Tipo = dto.Tipo,
            Descricao = dto.Descricao,
            DataRegistro = DateTime.UtcNow
        };

        if (dto.Tipo == TipoHistorico.InicioAtendimento)
            solicitacao.Status = StatusSolicitacao.EmAtendimento;

        _context.Set<HistoricoSolicitacao>().Add(historico);
        await _context.SaveChangesAsync();

        return new HistoricoResponseDto
        {
            Id = historico.Id,
            SolicitacaoId = historico.SolicitacaoId,
            TecnicoId = historico.TecnicoId,
            TecnicoNome = tecnico.Nome,
            Tipo = historico.Tipo.ToString(),
            Descricao = historico.Descricao,
            DataRegistro = historico.DataRegistro
        };
    }

    public async Task<SolicitacaoResponseDto> FinalizarServicoAsync(FinalizarServicoRequestDto dto)
    {
        var solicitacao = await _context.Set<Solicitacao>()
            .Include(s => s.Tecnico)
            .FirstOrDefaultAsync(s => s.Id == dto.SolicitacaoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada.");

        if (solicitacao.TecnicoId != dto.TecnicoId)
            throw new UnauthorizedAccessException("Apenas o técnico responsável pode finalizar.");

        if (solicitacao.Status == StatusSolicitacao.Finalizada)
            throw new InvalidOperationException("Solicitação já está finalizada.");

        solicitacao.Status = StatusSolicitacao.Finalizada;
        solicitacao.DataFinalizacao = DateTime.UtcNow;
        solicitacao.SolucaoAplicada = dto.SolucaoAplicada;

        var historico = new HistoricoSolicitacao
        {
            SolicitacaoId = dto.SolicitacaoId,
            TecnicoId = dto.TecnicoId,
            Tipo = TipoHistorico.Finalizacao,
            Descricao = $"Serviço finalizado. Solução: {dto.SolucaoAplicada}" +
                        (string.IsNullOrWhiteSpace(dto.ObservacaoFinal) ? "" : $" | Obs: {dto.ObservacaoFinal}"),
            DataRegistro = DateTime.UtcNow
        };

        _context.Set<HistoricoSolicitacao>().Add(historico);
        await _context.SaveChangesAsync();

        return new SolicitacaoResponseDto
        {
            Id = solicitacao.Id,
            Titulo = solicitacao.Titulo,
            Descricao = solicitacao.Descricao,
            Status = solicitacao.Status.ToString(),
            TecnicoId = solicitacao.TecnicoId,
            TecnicoNome = solicitacao.Tecnico?.Nome ?? string.Empty,
            DataCriacao = solicitacao.DataCriacao,
            DataFinalizacao = solicitacao.DataFinalizacao,
            SolucaoAplicada = solicitacao.SolucaoAplicada
        };
    }

    public async Task<IEnumerable<HistoricoResponseDto>> ObterHistoricoAsync(int solicitacaoId)
    {
        return await _context.Set<HistoricoSolicitacao>()
            .Include(h => h.Tecnico)
            .Where(h => h.SolicitacaoId == solicitacaoId)
            .OrderByDescending(h => h.DataRegistro)
            .Select(h => new HistoricoResponseDto
            {
                Id = h.Id,
                SolicitacaoId = h.SolicitacaoId,
                TecnicoId = h.TecnicoId,
                TecnicoNome = h.Tecnico!.Nome,
                Tipo = h.Tipo.ToString(),
                Descricao = h.Descricao,
                DataRegistro = h.DataRegistro
            })
            .ToListAsync();
    }
}