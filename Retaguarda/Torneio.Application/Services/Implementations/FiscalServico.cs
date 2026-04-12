using FluentValidation;
using Torneio.Application.Common;
using Torneio.Application.DTOs.Auth;
using Torneio.Application.DTOs.Fiscal;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Application.Services.Implementations;

public class FiscalServico : IFiscalServico
{
    private readonly IFiscalRepositorio _repositorio;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<CriarFiscalDto> _validador;

    public FiscalServico(
        IFiscalRepositorio repositorio,
        IPasswordHasher passwordHasher,
        IValidator<CriarFiscalDto> validador)
    {
        _repositorio = repositorio;
        _passwordHasher = passwordHasher;
        _validador = validador;
    }

    public async Task<FiscalDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id);
        return entidade is null ? null : ParaDto(entidade);
    }

    public async Task<IEnumerable<FiscalDto>> ListarPorAnoTorneio(Guid anoTorneioId)
    {
        var lista = await _repositorio.ListarPorAnoTorneio(anoTorneioId);
        return lista.Select(ParaDto);
    }

    public async Task<FiscalDto> Criar(CriarFiscalDto dto)
    {
        await _validador.ValidateAndThrowAsync(dto);

        var existente = await _repositorio.ObterPorUsuario(dto.Usuario, dto.TorneioId);
        if (existente is not null)
            throw new InvalidOperationException($"Usuário '{dto.Usuario}' já existe neste torneio.");

        var entidade = Fiscal.Criar(
            dto.TorneioId, dto.AnoTorneioId,
            dto.Nome, dto.Usuario, _passwordHasher.Hash(dto.Senha), dto.FotoUrl);

        await _repositorio.Adicionar(entidade);
        return ParaDto(entidade);
    }

    public async Task AtualizarSenha(Guid id, AtualizarSenhaDto dto)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Fiscal '{id}' não encontrado.");

        if (!_passwordHasher.Verificar(dto.SenhaAtual, entidade.SenhaHash))
            throw new InvalidOperationException("Senha atual incorreta.");

        entidade.AtualizarSenha(_passwordHasher.Hash(dto.NovaSenha));
        await _repositorio.Atualizar(entidade);
    }

    public async Task Remover(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Fiscal '{id}' não encontrado.");
        await _repositorio.Remover(entidade.Id);
    }

    private static FiscalDto ParaDto(Fiscal e) => new()
    {
        Id = e.Id,
        TorneioId = e.TorneioId,
        AnoTorneioId = e.AnoTorneioId,
        Nome = e.Nome,
        FotoUrl = e.FotoUrl,
        Usuario = e.Usuario
    };
}
