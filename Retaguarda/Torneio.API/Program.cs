using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Torneio.Application;
using Torneio.API.Middleware;
using Torneio.API.Services;
using Torneio.Infrastructure;
using Torneio.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

// ── Camadas de aplicação e infraestrutura ──────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

// ── JWT ────────────────────────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection(JwtOptions.Section);
builder.Services.Configure<JwtOptions>(jwtSection);
builder.Services.AddScoped<TokenServico>();

var jwtKey = jwtSection["SecretKey"]!;
var jwtIssuer = jwtSection["Issuer"]!;
var jwtAudience = jwtSection["Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// ── Políticas de autorização ───────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminGeral", policy =>
        policy.RequireClaim("perfil", "AdminGeral"));

    options.AddPolicy("AdminTorneio", policy =>
        policy.RequireClaim("perfil", "AdminGeral", "AdminTorneio"));

    options.AddPolicy("MembroTorneio", policy =>
        policy.RequireClaim("perfil", "Membro"));
});

// ── Controllers ────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Seed ───────────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Torneio.Infrastructure.Data.TorneioDbContext>();
    await db.Database.MigrateAsync();
    var hasher = scope.ServiceProvider.GetRequiredService<Torneio.Application.Common.IPasswordHasher>();
    await DatabaseSeeder.SeedAsync(db, hasher);
}

// ── Pipeline ───────────────────────────────────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();

// Serve arquivos do storage em /media — mesmo diretório físico que o Torneio.Web
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

app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();   // após auth → lê claims; após routing → lê slug da rota
app.UseAuthorization();
app.MapControllers();

app.Run();
