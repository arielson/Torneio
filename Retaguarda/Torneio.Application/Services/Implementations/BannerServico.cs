using Torneio.Application.DTOs.Banner;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Application.Services.Implementations;

public class BannerServico : IBannerServico
{
    private readonly IBannerRepositorio _repositorio;
    private readonly ITorneioRepositorio _torneioRepositorio;

    public BannerServico(IBannerRepositorio repositorio, ITorneioRepositorio torneioRepositorio)
    {
        _repositorio = repositorio;
        _torneioRepositorio = torneioRepositorio;
    }

    public async Task<IEnumerable<BannerDto>> ListarTodos()
    {
        var lista = await _repositorio.ListarTodos();
        return lista.Select(ParaDto);
    }

    public async Task<IEnumerable<BannerDto>> ListarAtivos()
    {
        var lista = await _repositorio.ListarAtivos();
        return lista.Select(ParaDto);
    }

    public async Task<BannerDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterComTorneio(id);
        return entidade is null ? null : ParaDto(entidade);
    }

    public async Task<BannerDto> Criar(CriarBannerDto dto)
    {
        var torneio = await _torneioRepositorio.ObterPorId(dto.TorneioId)
            ?? throw new KeyNotFoundException($"Torneio '{dto.TorneioId}' não encontrado.");
        var entidade = Banner.Criar(dto.TorneioId, dto.ImagemUrl, dto.Ordem, dto.TipoDestino, dto.Destino);
        await _repositorio.Adicionar(entidade);
        entidade = (await _repositorio.ObterComTorneio(entidade.Id))!;
        return ParaDto(entidade);
    }

    public async Task Ativar(Guid id)
    {
        var e = await _repositorio.ObterPorId(id) ?? throw new KeyNotFoundException();
        e.Ativar();
        await _repositorio.Atualizar(e);
    }

    public async Task Desativar(Guid id)
    {
        var e = await _repositorio.ObterPorId(id) ?? throw new KeyNotFoundException();
        e.Desativar();
        await _repositorio.Atualizar(e);
    }

    public async Task Excluir(Guid id) => await _repositorio.Remover(id);

    private static BannerDto ParaDto(Banner e) => new()
    {
        Id = e.Id,
        ImagemUrl = e.ImagemUrl,
        TorneioId = e.TorneioId,
        TorneioSlug = e.Torneio?.Slug ?? "",
        TorneioNome = e.Torneio?.NomeTorneio ?? "",
        Ordem = e.Ordem,
        Ativo = e.Ativo,
        TipoDestino = e.TipoDestino.ToString(),
        Destino = e.Destino,
    };
}
