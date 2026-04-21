using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Torneio.Application.Services.Implementations;
using Torneio.Application.Services.Interfaces;

namespace Torneio.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Validators
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Services
        services.AddScoped<ITorneioServico, TorneioServico>();
        services.AddScoped<IAdminGeralServico, AdminGeralServico>();
        services.AddScoped<IAdminTorneioServico, AdminTorneioServico>();
        services.AddScoped<IFiscalServico, FiscalServico>();
        services.AddScoped<IEquipeServico, EquipeServico>();
        services.AddScoped<IMembroServico, MembroServico>();
        services.AddScoped<IItemServico, ItemServico>();
        services.AddScoped<IPatrocinadorServico, PatrocinadorServico>();
        services.AddScoped<ICapturaServico, CapturaServico>();
        services.AddScoped<ISorteioAppServico, SorteioAppServico>();
        services.AddScoped<IGrupoAppServico, GrupoAppServico>();
        services.AddScoped<ISorteioGrupoAppServico, SorteioGrupoAppServico>();
        services.AddScoped<IPremioServico, PremioServico>();
        services.AddScoped<IAutenticacaoServico, AutenticacaoServico>();
        services.AddScoped<IBannerServico, BannerServico>();
        services.AddScoped<ILogAuditoriaServico, LogAuditoriaServico>();

        return services;
    }
}
