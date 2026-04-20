param(
    [Parameter(Mandatory)]
    [string]$ManifestPath,

    [Parameter(Mandatory)]
    [string]$OutputPath,

    [Parameter(Mandatory)]
    [string]$ScreenshotDir
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\common\helpers.ps1")
. (Join-Path $PSScriptRoot "..\common\assert-tools.ps1")
. (Join-Path $PSScriptRoot "..\common\paths.ps1")

Assert-WebVideoTools
Ensure-Directory -LiteralPath (Split-Path -Parent $OutputPath)
Ensure-Directory -LiteralPath $ScreenshotDir

Write-VideoSection "Gravando fluxo web com Playwright"
Write-VideoInfo "Manifesto de entrada: $ManifestPath"
Write-VideoInfo "Video bruto esperado em: $OutputPath"
Write-VideoInfo "Screenshots em: $ScreenshotDir"

$paths = Get-VideoPaths
$stdoutLog = Join-Path $paths.TempRoot "record-web.stdout.log"
$stderrLog = Join-Path $paths.TempRoot "record-web.stderr.log"

$process = Start-Process `
    -FilePath "node" `
    -ArgumentList @((Join-Path $PSScriptRoot "record-web.mjs"), "--manifest", $ManifestPath, "--screenshotDir", $ScreenshotDir, "--rawVideo", $OutputPath) `
    -RedirectStandardOutput $stdoutLog `
    -RedirectStandardError $stderrLog `
    -PassThru `
    -Wait

if ($process.ExitCode -ne 0) {
    Write-VideoWarn "A gravacao web falhou. Ultimas linhas do stdout:"
    if (Test-Path -LiteralPath $stdoutLog) {
        Get-Content -LiteralPath $stdoutLog -Tail 80 | Write-Host
    }
    Write-VideoWarn "Ultimas linhas do stderr:"
    if (Test-Path -LiteralPath $stderrLog) {
        Get-Content -LiteralPath $stderrLog -Tail 80 | Write-Host
    }
    throw "record-web.mjs retornou codigo $($process.ExitCode)."
}

Write-VideoInfo "Video bruto salvo em: $OutputPath"
