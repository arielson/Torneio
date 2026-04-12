using FluentValidation;
using Torneio.Application.Common;
using Torneio.Application.DTOs.Torneio;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Application.Services.Implementations;

public class TorneioServico : ITorneioServico
{
    private readonly ITorneioRepositorio _repositorio;
    private readonly IValidator<CriarTorneioDto> _validadorCriar;
    private readonly IValidator<AtualizarTorneioDto> _validadorAtualizar;

    public TorneioServico(
        ITorneioRepositorio repositorio,
        IValidator<CriarTorneioDto> validadorCriar,
        IValidator<AtualizarTorneioDto> validadorAtualizar)
    {
        _repositorio = repositorio;
        _validadorCriar = validadorCriar;
        _validadorAtualizar = validadorAtualizar;
    }

    public async Task<TorneioDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id);
        return entidade is null ? null : ParaDto(entidade);
    }

    public async Task<TorneioDto?> ObterPorSlug(string slug)
    {
        var entidade = await _repositorio.ObterPorSlug(slug);
        return entidade is null ? null : ParaDto(entidade);
    }

    public async Task<IEnumerable<TorneioDto>> ListarTodos()
    {
        var lista = await _repositorio.ListarTodos();
        return lista.Select(ParaDto);
    }

    public async Task<IEnumerable<TorneioDto>> ListarAtivos()
    {
        var lista = await _repositorio.ListarAtivos();
        return lista.Select(ParaDto);
    }

    public async Task<TorneioDto> Criar(CriarTorneioDto dto)
    {
        await _validadorCriar.ValidateAndThrowAsync(dto);

        var existente = await _repositorio.ObterPorSlug(dto.Slug);
        if (existente is not null)
            throw new InvalidOperationException($"Já existe um torneio com o slug '{dto.Slug}'.");

        var preset = TorneioPresets.Get(dto.TipoTorneio);

        var entidade = TorneioEntity.Criar(
            dto.Slug, dto.NomeTorneio,
            preset.LabelEquipe, preset.LabelMembro, preset.LabelSupervisor,
            preset.LabelItem, preset.LabelCaptura, preset.MedidaCaptura,
            dto.ModoSorteio, dto.TipoTorneio,
            dto.UsarFatorMultiplicador, dto.PermitirCapturaOffline, dto.LogoUrl);

        await _repositorio.Adicionar(entidade);
        return ParaDto(entidade);
    }

    public async Task Atualizar(Guid id, AtualizarTorneioDto dto)
    {
        await _validadorAtualizar.ValidateAndThrowAsync(dto);

        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Torneio '{id}' não encontrado.");

        var preset = TorneioPresets.Get(entidade.TipoTorneio);

        entidade.AtualizarConfiguracoes(
            dto.NomeTorneio,
            preset.LabelEquipe, preset.LabelMembro, preset.LabelSupervisor,
            preset.LabelItem, preset.LabelCaptura, preset.MedidaCaptura,
            dto.ModoSorteio, dto.UsarFatorMultiplicador,
            dto.PermitirCapturaOffline, dto.LogoUrl);

        await _repositorio.Atualizar(entidade);
    }

    public async Task Ativar(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Torneio '{id}' não encontrado.");
        entidade.Ativar();
        await _repositorio.Atualizar(entidade);
    }

    public async Task Desativar(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Torneio '{id}' não encontrado.");
        entidade.Desativar();
        await _repositorio.Atualizar(entidade);
    }

    private static TorneioDto ParaDto(TorneioEntity e) => new()
    {
        Id = e.Id,
        Slug = e.Slug,
        NomeTorneio = e.NomeTorneio,
        LogoUrl = e.LogoUrl,
        Ativo = e.Ativo,
        LabelEquipe = e.LabelEquipe,
        LabelMembro = e.LabelMembro,
        LabelSupervisor = e.LabelSupervisor,
        LabelItem = e.LabelItem,
        LabelCaptura = e.LabelCaptura,
        UsarFatorMultiplicador = e.UsarFatorMultiplicador,
        MedidaCaptura = e.MedidaCaptura,
        PermitirCapturaOffline = e.PermitirCapturaOffline,
        ModoSorteio = e.ModoSorteio.ToString(),
        TipoTorneio = e.TipoTorneio,
    };
}
