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
    private readonly ITorneioRepositorio _torneioRepositorio;
    private readonly IPasswordHasher _passwordHasher;

    public AutenticacaoServico(
        IAdminGeralRepositorio adminGeralRepositorio,
        IAdminTorneioRepositorio adminTorneioRepositorio,
        IFiscalRepositorio fiscalRepositorio,
        ITorneioRepositorio torneioRepositorio,
        IPasswordHasher passwordHasher)
    {
        _adminGeralRepositorio = adminGeralRepositorio;
        _adminTorneioRepositorio = adminTorneioRepositorio;
        _fiscalRepositorio = fiscalRepositorio;
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
        var admins = await _adminTorneioRepositorio.ListarPorTorneio(torneioId);
        var entidade = admins.FirstOrDefault(a => a.Usuario == usuario);
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
            Slug = torneio?.Slug
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
            Slug = torneio?.Slug
        };
    }
}
