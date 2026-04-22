using FluentValidation;
using Torneio.Application.Common;
using Torneio.Application.DTOs.AdminGeral;
using Torneio.Application.DTOs.Auth;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Application.Services.Implementations;

public class AdminGeralServico : IAdminGeralServico
{
    private readonly IAdminGeralRepositorio _repositorio;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<CriarAdminGeralDto> _validador;

    public AdminGeralServico(
        IAdminGeralRepositorio repositorio,
        IPasswordHasher passwordHasher,
        IValidator<CriarAdminGeralDto> validador)
    {
        _repositorio = repositorio;
        _passwordHasher = passwordHasher;
        _validador = validador;
    }

    public async Task<AdminGeralDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id);
        return entidade is null ? null : ParaDto(entidade);
    }

    public async Task<IEnumerable<AdminGeralDto>> ListarTodos()
    {
        var lista = await _repositorio.ListarTodos();
        return lista.Select(ParaDto);
    }

    public async Task<AdminGeralDto> Criar(CriarAdminGeralDto dto)
    {
        await _validador.ValidateAndThrowAsync(dto);

        var existente = await _repositorio.ObterPorUsuario(dto.Usuario);
        if (existente is not null)
            throw new InvalidOperationException($"Usuário '{dto.Usuario}' já está em uso.");

        var entidade = AdminGeral.Criar(dto.Nome, dto.Usuario, _passwordHasher.Hash(dto.Senha));
        await _repositorio.Adicionar(entidade);
        return ParaDto(entidade);
    }

    public async Task AtualizarSenha(Guid id, AtualizarSenhaDto dto)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"AdminGeral '{id}' não encontrado.");

        if (!_passwordHasher.Verificar(dto.SenhaAtual, entidade.SenhaHash))
            throw new InvalidOperationException("Senha atual incorreta.");

        entidade.AtualizarSenha(_passwordHasher.Hash(dto.NovaSenha));
        await _repositorio.Atualizar(entidade);
    }

    public async Task Remover(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"AdminGeral '{id}' não encontrado.");
        var admins = (await _repositorio.ListarTodos()).ToList();
        if (admins.Count <= 1)
            throw new InvalidOperationException("Deve existir ao menos um Admin Geral ativo no sistema.");

        if (string.Equals(entidade.Usuario, "admin", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("O primeiro Admin Geral do sistema nao pode ser excluido.");

        await _repositorio.Remover(entidade.Id);
    }

    private static AdminGeralDto ParaDto(AdminGeral e) => new()
    {
        Id = e.Id,
        Nome = e.Nome,
        Usuario = e.Usuario
    };
}
