param(
    [string]$BaseUrl = "http://localhost:5068",
    [int]$StartupTimeoutSeconds = 60
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\common\helpers.ps1")
. (Join-Path $PSScriptRoot "..\common\paths.ps1")

function Test-UrlOnline {
    param(
        [Parameter(Mandatory)]
        [string]$Url
    )

    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -Method Get -TimeoutSec 10
        return $response.StatusCode -ge 200 -and $response.StatusCode -lt 500
    }
    catch {
        return $false
    }
}

$paths = Get-VideoPaths
$webProjectPath = Join-Path $paths.RepoRoot "Retaguarda\Torneio.Web\Torneio.Web.csproj"
$stdoutLog = Join-Path $paths.TempRoot "torneio-web.stdout.log"
$stderrLog = Join-Path $paths.TempRoot "torneio-web.stderr.log"
Ensure-Directory -LiteralPath (Split-Path -Parent $stdoutLog)

Write-VideoSection "Inicializando retaguarda web"

if (Test-UrlOnline -Url $BaseUrl) {
    Write-VideoInfo "Retaguarda ja esta respondendo em $BaseUrl"
    return
}

Start-Process -FilePath "dotnet" -ArgumentList @("run", "--project", $webProjectPath, "--launch-profile", "http") -RedirectStandardOutput $stdoutLog -RedirectStandardError $stderrLog | Out-Null

$deadline = (Get-Date).AddSeconds($StartupTimeoutSeconds)
while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds 3
    if (Test-UrlOnline -Url $BaseUrl) {
        Write-VideoInfo "Retaguarda pronta em $BaseUrl"
        return
    }
}

throw "A retaguarda web nao ficou disponivel em $BaseUrl dentro do tempo esperado."
