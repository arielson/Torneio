using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Torneio.Application;
using Torneio.Infrastructure;
using Torneio.Infrastructure.Seed;
using Torneio.Web.Middleware;

// Npgsql 8: aceita DateTime com Kind=Unspecified tratando como UTC (comportamento legado)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

builder.Services.AddAuthentication("TorneioCookie")
    .AddCookie("TorneioCookie", options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/acesso-negado";
        options.Cookie.Name = "TorneioAuth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminGeral", p =>
        p.RequireAuthenticatedUser().RequireClaim("perfil", "AdminGeral"));
    options.AddPolicy("AdminTorneio", p =>
        p.RequireAuthenticatedUser().RequireClaim("perfil", "AdminGeral", "AdminTorneio"));
    options.AddPolicy("MembroTorneio", p =>
        p.RequireAuthenticatedUser().RequireClaim("perfil", "Membro"));
});

builder.Services.AddControllersWithViews(options =>
{
    // Mensagens de model binding em português
    var m = options.ModelBindingMessageProvider;
    m.SetAttemptedValueIsInvalidAccessor((v, f) => $"O valor '{v}' é inválido para o campo '{f}'.");
    m.SetMissingBindRequiredValueAccessor(f => $"O campo '{f}' é obrigatório.");
    m.SetMissingKeyOrValueAccessor(() => "O campo é obrigatório.");
    m.SetNonPropertyAttemptedValueIsInvalidAccessor(v => $"O valor '{v}' é inválido.");
    m.SetNonPropertyUnknownValueIsInvalidAccessor(() => "O valor fornecido é inválido.");
    m.SetNonPropertyValueMustBeANumberAccessor(() => "O campo deve ser um número.");
    m.SetUnknownValueIsInvalidAccessor(f => $"O valor fornecido para '{f}' é inválido.");
    m.SetValueIsInvalidAccessor(v => $"O valor '{v}' é inválido.");
    m.SetValueMustBeANumberAccessor(f => $"O campo '{f}' deve ser um número.");
    m.SetValueMustNotBeNullAccessor(f => $"O campo '{f}' é obrigatório.");

    // Suprime [Required] implícito gerado por tipos não-nuláveis (mensagens seriam em inglês)
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<FormOptions>(o => o.ValueCountLimit = 1_048_576);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Serve arquivos do storage em /media (fotos de equipes, membros, etc.)
var storagePath = app.Configuration["Storage:BasePath"];
if (!string.IsNullOrEmpty(storagePath))
{
    storagePath = Path.IsPathRooted(storagePath)
        ? Path.GetFullPath(storagePath)
        : Path.GetFullPath(storagePath, app.Environment.ContentRootPath);

    Directory.CreateDirectory(storagePath);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(storagePath),
        RequestPath = "/media"
    });
}

app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<TrocarSenhaMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Torneio.Infrastructure.Data.TorneioDbContext>();
    await db.Database.MigrateAsync();
    var hasher = scope.ServiceProvider.GetRequiredService<Torneio.Application.Common.IPasswordHasher>();
    await DatabaseSeeder.SeedAsync(db, hasher);
}

app.Run();
