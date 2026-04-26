using Torneio.Application.Common;
using Torneio.Application.DTOs.Auth;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Application.Services.Implementations;

public class AutenticacaoServico : IAutenticacaoServico
{
    private readonly IAdminGeralRepositorio _adminGeralRepositorio;
    private readonly IAdminTorneioRepositorio _adminTorneioRepositorio;
    private readonly IFiscalRepositorio _fiscalRepositorio;
    private readonly IMembroRepositorio _membroRepositorio;
    private readonly ITorneioRepositorio _torneioRepositorio;
    private readonly IPasswordHasher _passwordHasher;

    public AutenticacaoServico(
        IAdminGeralRepositorio adminGeralRepositorio,
        IAdminTorneioRepositorio adminTorneioRepositorio,
        IFiscalRepositorio fiscalRepositorio,
        IMembroRepositorio membroRepositorio,
        ITorneioRepositorio torneioRepositorio,
        IPasswordHasher passwordHasher)
    {
        _adminGeralRepositorio = adminGeralRepositorio;
        _adminTorneioRepositorio = adminTorneioRepositorio;
        _fiscalRepositorio = fiscalRepositorio;
        _membroRepositorio = membroRepositorio;
        _torneioRepositorio = torneioRepositorio;
        _passwordHasher = passwordHasher;
    }

    public async Task<UsuarioAutenticadoDto?> AutenticarAdminGeral(string usuario, string senha)
    {
        var entidade = await _adminGeralRepositorio.ObterPorUsuario(usuario);
        if (entidade is null || !_passwordHasher.Verificar(senha, entidade.SenhaHash))
            return null;

        return new UsuarioAutenticadoDto
        {
            Id = entidade.Id,
            Nome = entidade.Nome,
            Usuario = entidade.Usuario,
            Perfil = PerfilUsuario.AdminGeral
        };
    }

    public async Task<UsuarioAutenticadoDto?> AutenticarAdminTorneio(string usuario, string senha, Guid torneioId)
    {
        var entidade = await _adminTorneioRepositorio.ObterPorUsuario(usuario, torneioId);
        if (entidade is null || !_passwordHasher.Verificar(senha, entidade.SenhaHash))
            return null;

        var torneio = await _torneioRepositorio.ObterPorId(torneioId);

        return new UsuarioAutenticadoDto
        {
            Id = entidade.Id,
            Nome = entidade.Nome,
            Usuario = entidade.Usuario,
            Perfil = PerfilUsuario.AdminTorneio,
            TorneioId = torneioId,
            Slug = torneio?.Slug,
            DeveAlterarSenha = entidade.DeveAlterarSenha
        };
    }

    public async Task<UsuarioAutenticadoDto?> AutenticarFiscal(string usuario, string senha, Guid torneioId)
    {
        var entidade = await _fiscalRepositorio.ObterPorUsuario(usuario, torneioId);
        if (entidade is null || !_passwordHasher.Verificar(senha, entidade.SenhaHash))
            return null;

        var torneio = await _torneioRepositorio.ObterPorId(torneioId);

        return new UsuarioAutenticadoDto
        {
            Id = entidade.Id,
            Nome = entidade.Nome,
            Usuario = entidade.Usuario,
            Perfil = PerfilUsuario.Fiscal,
            TorneioId = torneioId,
            Slug = torneio?.Slug,
            DeveAlterarSenha = entidade.DeveAlterarSenha
        };
    }

    public async Task TrocarSenha(Guid usuarioId, string perfil, string senhaAtual, string novaSenha, Guid? torneioId)
    {
        switch (perfil.ToLowerInvariant())
        {
            case "admintorneio":
            {
                var entidade = await _adminTorneioRepositorio.ObterPorId(usuarioId)
                    ?? throw new InvalidOperationException("Usuário não encontrado.");
                if (!_passwordHasher.Verificar(senhaAtual, entidade.SenhaHash))
                    throw new InvalidOperationException("Senha atual incorreta.");
                entidade.AtualizarSenha(_passwordHasher.Hash(novaSenha));
                await _adminTorneioRepositorio.Atualizar(entidade);
                break;
            }
            case "fiscal":
            {
                var entidade = await _fiscalRepositorio.ObterPorId(usuarioId)
                    ?? throw new InvalidOperationException("Usuário não encontrado.");
                if (!_passwordHasher.Verificar(senhaAtual, entidade.SenhaHash))
                    throw new InvalidOperationException("Senha atual incorreta.");
                entidade.AtualizarSenha(_passwordHasher.Hash(novaSenha));
                await _fiscalRepositorio.Atualizar(entidade);
                break;
            }
            case "membro":
            {
                var entidade = await _membroRepositorio.ObterPorId(usuarioId)
                    ?? throw new InvalidOperationException("Usuário não encontrado.");
                if (string.IsNullOrWhiteSpace(entidade.SenhaHash) || !_passwordHasher.Verificar(senhaAtual, entidade.SenhaHash))
                    throw new InvalidOperationException("Senha atual incorreta.");
                entidade.AtualizarCredenciais(null, _passwordHasher.Hash(novaSenha));
                await _membroRepositorio.Atualizar(entidade);
                break;
            }
            default:
                throw new InvalidOperationException($"Perfil '{perfil}' não suporta troca de senha obrigatória.");
        }
    }

    public async Task<UsuarioAutenticadoDto?> AutenticarMembro(string usuario, string senha, Guid torneioId)
    {
        var entidade = await _membroRepositorio.ObterPorUsuario(torneioId, usuario);
        if (entidade is null || string.IsNullOrWhiteSpace(entidade.SenhaHash) || !_passwordHasher.Verificar(senha, entidade.SenhaHash))
            return null;

        var torneio = await _torneioRepositorio.ObterPorId(torneioId);

        return new UsuarioAutenticadoDto
        {
            Id = entidade.Id,
            Nome = entidade.Nome,
            Usuario = entidade.Usuario ?? usuario,
            Perfil = PerfilUsuario.Membro,
            TorneioId = torneioId,
            Slug = torneio?.Slug,
            DeveAlterarSenha = entidade.DeveAlterarSenha
        };
    }
}
