using Torneio.Application.DTOs.Notificacao;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Application.Services.Implementations;

public class MensagemTorneioServico : IMensagemTorneioServico
{
    private readonly IMensagemTorneioRepositorio _repositorio;
    private readonly ISeguidorTorneioRepositorio _seguidorRepositorio;
    private readonly INotificacaoServico _notificacao;
    private readonly ITenantContext _tenantContext;

    public MensagemTorneioServico(
        IMensagemTorneioRepositorio repositorio,
        ISeguidorTorneioRepositorio seguidorRepositorio,
        INotificacaoServico notificacao,
        ITenantContext tenantContext)
    {
        _repositorio = repositorio;
        _seguidorRepositorio = seguidorRepositorio;
        _notificacao = notificacao;
        _tenantContext = tenantContext;
    }

    public async Task<MensagemTorneioDto> EnviarAsync(string titulo, string corpo, string criadoPor)
    {
        var mensagem = MensagemTorneio.Criar(_tenantContext.TorneioId, titulo, corpo, criadoPor);
        await _repositorio.Adicionar(mensagem);

        var tokens = (await _seguidorRepositorio.ListarTokens(_tenantContext.TorneioId)).ToList();
        if (tokens.Count > 0)
            await _notificacao.EnviarParaTokensAsync(tokens, titulo, corpo);

        return ParaDto(mensagem);
    }

    public async Task<IEnumerable<MensagemTorneioDto>> ListarAsync()
    {
        var lista = await _repositorio.ListarPorTorneio(_tenantContext.TorneioId);
        return lista.OrderByDescending(m => m.CriadoEm).Select(ParaDto);
    }

    public async Task RemoverAsync(Guid id)
    {
        var mensagem = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Mensagem '{id}' não encontrada.");
        if (mensagem.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Mensagem '{id}' não encontrada.");

        await _repositorio.Remover(id);
    }

    private static MensagemTorneioDto ParaDto(MensagemTorneio m) => new()
    {
        Id = m.Id,
        TorneioId = m.TorneioId,
        Titulo = m.Titulo,
        Corpo = m.Corpo,
        CriadoPor = m.CriadoPor,
        CriadoEm = m.CriadoEm
    };
}
