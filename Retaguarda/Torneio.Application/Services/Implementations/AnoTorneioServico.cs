using FluentValidation;
using Torneio.Application.DTOs.AnoTorneio;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Application.Services.Implementations;

public class AnoTorneioServico : IAnoTorneioServico
{
    private readonly IAnoTorneioRepositorio _repositorio;
    private readonly IFiscalRepositorio _fiscalRepositorio;
    private readonly IEquipeRepositorio _equipeRepositorio;
    private readonly IMembroRepositorio _membroRepositorio;
    private readonly IValidator<CriarAnoTorneioDto> _validador;

    public AnoTorneioServico(
        IAnoTorneioRepositorio repositorio,
        IFiscalRepositorio fiscalRepositorio,
        IEquipeRepositorio equipeRepositorio,
        IMembroRepositorio membroRepositorio,
        IValidator<CriarAnoTorneioDto> validador)
    {
        _repositorio = repositorio;
        _fiscalRepositorio = fiscalRepositorio;
        _equipeRepositorio = equipeRepositorio;
        _membroRepositorio = membroRepositorio;
        _validador = validador;
    }

    public async Task<AnoTorneioDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id);
        return entidade is null ? null : ParaDto(entidade);
    }

    public async Task<IEnumerable<AnoTorneioDto>> ListarPorTorneio(Guid torneioId)
    {
        var lista = await _repositorio.ListarPorTorneio(torneioId);
        return lista.Select(ParaDto);
    }

    public async Task<AnoTorneioDto> Criar(CriarAnoTorneioDto dto)
    {
        await _validador.ValidateAndThrowAsync(dto);

        var existente = await _repositorio.ObterPorAno(dto.TorneioId, dto.Ano);
        if (existente is not null)
            throw new InvalidOperationException($"Já existe um ano {dto.Ano} para este torneio.");

        var entidade = AnoTorneio.Criar(dto.TorneioId, dto.Ano);
        await _repositorio.Adicionar(entidade);
        return ParaDto(entidade);
    }

    public async Task Liberar(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Ano do torneio '{id}' não encontrado.");
        entidade.Liberar();
        await _repositorio.Atualizar(entidade);
    }

    public async Task Finalizar(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Ano do torneio '{id}' não encontrado.");
        entidade.Finalizar();
        await _repositorio.Atualizar(entidade);
    }

    public async Task Reabrir(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Ano do torneio '{id}' não encontrado.");
        entidade.Reabrir();
        await _repositorio.Atualizar(entidade);
    }

    public async Task<AnoTorneioDto> ReplicarAno(Guid anoTorneioOrigemId, int novoAno)
    {
        var origem = await _repositorio.ObterPorId(anoTorneioOrigemId)
            ?? throw new KeyNotFoundException($"Ano origem '{anoTorneioOrigemId}' não encontrado.");

        var existente = await _repositorio.ObterPorAno(origem.TorneioId, novoAno);
        if (existente is not null)
            throw new InvalidOperationException($"Já existe o ano {novoAno} para este torneio.");

        var novoAnoEntidade = AnoTorneio.Criar(origem.TorneioId, novoAno);
        await _repositorio.Adicionar(novoAnoEntidade);

        // Replica Fiscais
        var fiscais = await _fiscalRepositorio.ListarPorAnoTorneio(anoTorneioOrigemId);
        var mapaFiscais = new Dictionary<Guid, Guid>();
        foreach (var fiscal in fiscais)
        {
            var novoFiscal = Fiscal.Criar(
                fiscal.TorneioId, novoAnoEntidade.Id,
                fiscal.Nome, fiscal.Usuario, fiscal.SenhaHash, fiscal.FotoUrl);
            await _fiscalRepositorio.Adicionar(novoFiscal);
            mapaFiscais[fiscal.Id] = novoFiscal.Id;
        }

        // Replica Equipes e Membros
        var equipes = await _equipeRepositorio.ListarPorAnoTorneio(anoTorneioOrigemId);
        foreach (var equipe in equipes)
        {
            var novoFiscalId = mapaFiscais.TryGetValue(equipe.FiscalId, out var fid) ? fid : equipe.FiscalId;
            var novaEquipe = Equipe.Criar(
                equipe.TorneioId, novoAnoEntidade.Id,
                equipe.Nome, equipe.Capitao, novoFiscalId, equipe.QtdVagas,
                equipe.FotoUrl, equipe.FotoCapitaoUrl);
            await _equipeRepositorio.Adicionar(novaEquipe);
        }

        var membros = await _membroRepositorio.ListarPorAnoTorneio(anoTorneioOrigemId);
        foreach (var membro in membros)
        {
            var novoMembro = Membro.Criar(
                membro.TorneioId, novoAnoEntidade.Id,
                membro.Nome, membro.FotoUrl);
            await _membroRepositorio.Adicionar(novoMembro);
        }

        return ParaDto(novoAnoEntidade);
    }

    private static AnoTorneioDto ParaDto(AnoTorneio e) => new()
    {
        Id = e.Id,
        TorneioId = e.TorneioId,
        Ano = e.Ano,
        Status = e.Status.ToString()
    };
}
