using FluentValidation;
using Torneio.Application.Common;
using Torneio.Application.DTOs.AdminTorneio;
using Torneio.Application.DTOs.Auth;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Application.Services.Implementations;

public class AdminTorneioServico : IAdminTorneioServico
{
    private readonly IAdminTorneioRepositorio _repositorio;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<CriarAdminTorneioDto> _validador;

    public AdminTorneioServico(
        IAdminTorneioRepositorio repositorio,
        IPasswordHasher passwordHasher,
        IValidator<CriarAdminTorneioDto> validador)
    {
        _repositorio = repositorio;
        _passwordHasher = passwordHasher;
        _validador = validador;
    }

    public async Task<AdminTorneioDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id);
        return entidade is null ? null : ParaDto(entidade);
    }

    public async Task<IEnumerable<AdminTorneioDto>> ListarPorTorneio(Guid torneioId)
    {
        var lista = await _repositorio.ListarPorTorneio(torneioId);
        return lista.Select(ParaDto);
    }

    public async Task<AdminTorneioDto> Criar(CriarAdminTorneioDto dto)
    {
        await _validador.ValidateAndThrowAsync(dto);

        var existente = await _repositorio.ObterPorUsuario(dto.Usuario);
        if (existente is not null && existente.TorneioId == dto.TorneioId)
            throw new InvalidOperationException($"Usuário '{dto.Usuario}' já é admin deste torneio.");

        // Reutiliza o UsuarioId se o usuário já existe em outro torneio
        var usuarioId = existente?.UsuarioId ?? Guid.NewGuid();

        var entidade = AdminTorneio.Criar(usuarioId, dto.TorneioId, dto.Nome, dto.Usuario,
            _passwordHasher.Hash(dto.Senha));
        await _repositorio.Adicionar(entidade);
        return ParaDto(entidade);
    }

    public async Task AtualizarSenha(Guid id, AtualizarSenhaDto dto)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"AdminTorneio '{id}' não encontrado.");

        if (!_passwordHasher.Verificar(dto.SenhaAtual, entidade.SenhaHash))
            throw new InvalidOperationException("Senha atual incorreta.");

        entidade.AtualizarSenha(_passwordHasher.Hash(dto.NovaSenha));
        await _repositorio.Atualizar(entidade);
    }

    public async Task Remover(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"AdminTorneio '{id}' não encontrado.");
        await _repositorio.Remover(entidade.Id);
    }

    private static AdminTorneioDto ParaDto(AdminTorneio e) => new()
    {
        Id = e.Id,
        UsuarioId = e.UsuarioId,
        TorneioId = e.TorneioId,
        Nome = e.Nome,
        Usuario = e.Usuario
    };
}
