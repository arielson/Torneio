using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using Torneio.Application.Common;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;
using Torneio.Infrastructure.Data;
using Torneio.Infrastructure.Repositories;
using Torneio.Infrastructure.Services;
using Torneio.Infrastructure.Services.Options;

namespace Torneio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath)
    {
        // QuestPDF — licença Community (gratuita)
        QuestPDF.Settings.License = LicenseType.Community;

        // EF Core + Npgsql
        services.AddDbContext<TorneioDbContext>((sp, options) =>
        {
            options
                .UseNpgsql(configuration.GetConnectionString("Default"))
                .UseSnakeCaseNamingConvention();
        });

        // TenantContext — scoped para que cada request tenha seu próprio contexto
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

        // Serviços de aplicação implementados na Infrastructure
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITenantResolver, TenantResolver>();
        services.AddScoped<IFileStorage, FileStorage>();
        services.AddScoped<ISorteioServico, SorteioServico>();
        services.AddScoped<ISorteioGrupoServico, SorteioGrupoServico>();
        services.AddScoped<IRelatorioServico, RelatorioServico>();
        services.AddScoped<ISmsVerificacaoServico, TwilioSmsVerificacaoServico>();

        // Repositórios
        services.AddScoped<ITorneioRepositorio, TorneioRepositorio>();
        services.AddScoped<IAdminGeralRepositorio, AdminGeralRepositorio>();
        services.AddScoped<IAdminTorneioRepositorio, AdminTorneioRepositorio>();
        services.AddScoped<IFiscalRepositorio, FiscalRepositorio>();
        services.AddScoped<IEquipeRepositorio, EquipeRepositorio>();
        services.AddScoped<IMembroRepositorio, MembroRepositorio>();
        services.AddScoped<IRegistroPublicoMembroRepositorio, RegistroPublicoMembroRepositorio>();
        services.AddScoped<IItemRepositorio, ItemRepositorio>();
        services.AddScoped<IPatrocinadorRepositorio, PatrocinadorRepositorio>();
        services.AddScoped<IParcelaTorneioRepositorio, ParcelaTorneioRepositorio>();
        services.AddScoped<IProdutoExtraTorneioRepositorio, ProdutoExtraTorneioRepositorio>();
        services.AddScoped<IProdutoExtraMembroRepositorio, ProdutoExtraMembroRepositorio>();
        services.AddScoped<IDoacaoPatrocinadorRepositorio, DoacaoPatrocinadorRepositorio>();
        services.AddScoped<ICustoTorneioRepositorio, CustoTorneioRepositorio>();
        services.AddScoped<IChecklistTorneioItemRepositorio, ChecklistTorneioItemRepositorio>();
        services.AddScoped<ICapturaRepositorio, CapturaRepositorio>();
        services.AddScoped<ISorteioEquipeRepositorio, SorteioEquipeRepositorio>();
        services.AddScoped<IGrupoRepositorio, GrupoRepositorio>();
        services.AddScoped<ISorteioGrupoRepositorio, SorteioGrupoRepositorio>();
        services.AddScoped<IPremioRepositorio, PremioRepositorio>();
        services.AddScoped<IBannerRepositorio, BannerRepositorio>();
        services.AddScoped<ILogAuditoriaRepositorio, LogAuditoriaRepositorio>();

        // Opções de armazenamento de arquivos
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.Section));
        services.PostConfigure<StorageOptions>(options =>
        {
            if (string.IsNullOrWhiteSpace(options.BasePath))
            {
                return;
            }

            options.BasePath = Path.IsPathRooted(options.BasePath)
                ? Path.GetFullPath(options.BasePath)
                : Path.GetFullPath(options.BasePath, contentRootPath);
        });
        services.Configure<TwilioOptions>(configuration.GetSection(TwilioOptions.Section));

        return services;
    }
}
