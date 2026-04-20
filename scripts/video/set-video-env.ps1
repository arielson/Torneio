Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-TorneioIdBySlug {
    param(
        [Parameter(Mandatory)]
        [string]$Slug
    )

    $appSettingsPath = Join-Path $PSScriptRoot "..\..\Retaguarda\Torneio.Web\appsettings.json"
    $npgsqlPath = Join-Path $PSScriptRoot "..\..\Retaguarda\Torneio.Web\bin\Debug\net10.0\Npgsql.dll"

    if (-not (Test-Path -LiteralPath $appSettingsPath) -or -not (Test-Path -LiteralPath $npgsqlPath)) {
        return $null
    }

    $config = Get-Content -LiteralPath $appSettingsPath -Raw | ConvertFrom-Json
    $connectionString = $config.ConnectionStrings.Default
    if ([string]::IsNullOrWhiteSpace($connectionString)) {
        return $null
    }

    try {
        Add-Type -Path $npgsqlPath -ErrorAction Stop
        $connection = [Npgsql.NpgsqlConnection]::new($connectionString)
        try {
            $connection.Open()
            $command = $connection.CreateCommand()
            $command.CommandText = "select id::text from torneio where slug = @slug limit 1"
            [void]$command.Parameters.AddWithValue("slug", $Slug)
            $result = $command.ExecuteScalar()
            if ($null -eq $result) {
                return $null
            }

            return [string]$result
        }
        finally {
            $connection.Dispose()
        }
    }
    catch {
        Write-Warning "Nao foi possivel resolver o ID do torneio '$Slug' automaticamente. O pipeline seguira sem esse valor."
        return $null
    }
}

Write-Host ""
Write-Host "== Configurando variaveis de ambiente do pipeline de video ==" -ForegroundColor Cyan

# Web: URL base da retaguarda
$env:VIDEO_WEB_BASE_URL = "http://localhost:5068"

# Slugs dos torneios
$env:VIDEO_SLUG_AMIGOS = "amigos-da-pesca-2026"
$env:VIDEO_SLUG_REI = "rei-dos-mares-2026"
$env:VIDEO_SLUG_BTS = "bts-sport-fishing-2026"

# IDs dos torneios
$env:VIDEO_TORNEIOID_AMIGOS = Get-TorneioIdBySlug -Slug $env:VIDEO_SLUG_AMIGOS
$env:VIDEO_TORNEIOID_REI = Get-TorneioIdBySlug -Slug $env:VIDEO_SLUG_REI
$env:VIDEO_TORNEIOID_BTS = Get-TorneioIdBySlug -Slug $env:VIDEO_SLUG_BTS

# Web: admins do torneio por slug
$env:VIDEO_WEB_AMIGOS_ADMIN_USERNAME = "ari"
$env:VIDEO_WEB_AMIGOS_ADMIN_PASSWORD = "httpr0x1"

$env:VIDEO_WEB_REI_ADMIN_USERNAME = "arielsonf"
$env:VIDEO_WEB_REI_ADMIN_PASSWORD = "httpr0x"

$env:VIDEO_WEB_BTS_ADMIN_USERNAME = "arielson"
$env:VIDEO_WEB_BTS_ADMIN_PASSWORD = "httpr0x"

# App: perfis separados por torneio
$env:VIDEO_APP_AMIGOS_FISCAL_USERNAME = "arielson"
$env:VIDEO_APP_AMIGOS_FISCAL_PASSWORD = "httpr0x"

$env:VIDEO_APP_AMIGOS_ADMIN_USERNAME = "ari"
$env:VIDEO_APP_AMIGOS_ADMIN_PASSWORD = "httpr0x1"

$env:VIDEO_APP_REI_ADMIN_USERNAME = "arielsonf"
$env:VIDEO_APP_REI_ADMIN_PASSWORD = "httpr0x"

$env:VIDEO_APP_BTS_ADMIN_USERNAME = "arielson"
$env:VIDEO_APP_BTS_ADMIN_PASSWORD = "httpr0x"

Write-Host "[info] Variaveis carregadas na sessao atual do PowerShell."
Write-Host "[info] Edite este arquivo para informar URL, slugs, usuarios e senhas reais."
Write-Host "[info] Depois execute um dos comandos abaixo:"
Write-Host "       .\scripts\video\run.ps1 -Target web -InstallDependencies -StartWeb"
Write-Host "       .\scripts\video\run.ps1 -Target app -InstallDependencies -StartEmulator -BuildAndInstallApp -StartAppium"
