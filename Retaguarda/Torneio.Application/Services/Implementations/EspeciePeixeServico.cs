using Torneio.Application.DTOs.EspeciePeixe;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Application.Services.Implementations;

public class EspeciePeixeServico : IEspeciePeixeServico
{
    private readonly IEspeciePeixeRepositorio _repositorio;

    public EspeciePeixeServico(IEspeciePeixeRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IEnumerable<EspeciePeixeDto>> ListarTodas()
    {
        var lista = await _repositorio.ListarTodas();
        return lista.Select(ParaDto);
    }

    public async Task<EspeciePeixeDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id);
        return entidade is null ? null : ParaDto(entidade);
    }

    public async Task<EspeciePeixeDto> Criar(CriarEspeciePeixeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
            throw new ArgumentException("O nome é obrigatório.");

        var entidade = EspeciePeixe.Criar(dto.Nome.Trim(), dto.NomeCientifico?.Trim(), dto.FotoUrl);
        await _repositorio.Adicionar(entidade);
        return ParaDto(entidade);
    }

    public async Task Atualizar(Guid id, AtualizarEspeciePeixeDto dto)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Espécie '{id}' não encontrada.");

        entidade.Atualizar(dto.Nome.Trim(), dto.NomeCientifico?.Trim(), dto.FotoUrl);
        await _repositorio.Atualizar(entidade);
    }

    public async Task Remover(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Espécie '{id}' não encontrada.");

        await _repositorio.Remover(entidade.Id);
    }

    private static EspeciePeixeDto ParaDto(EspeciePeixe e) => new()
    {
        Id = e.Id,
        Nome = e.Nome,
        NomeCientifico = e.NomeCientifico,
        FotoUrl = e.FotoUrl
    };
}
