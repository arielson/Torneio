param(
    [Parameter(Mandatory)]
    [string]$ManifestPath,

    [Parameter(Mandatory)]
    [string]$OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\common\helpers.ps1")

$manifest = Get-JsonFile -LiteralPath $ManifestPath
$lines = [System.Collections.Generic.List[string]]::new()

$lines.Add("# $($manifest.title)")
$lines.Add("")
$lines.Add("## Resumo")
$lines.Add("")
$lines.Add("- target: $($manifest.target)")
$lines.Add("- profile: $($manifest.profile)")
$lines.Add("- format: $($manifest.video.width)x$($manifest.video.height)")
$lines.Add("- voice provider: $($manifest.voice.provider)")
$lines.Add("")
$lines.Add("## Cenas")
$lines.Add("")

$sceneIndex = 1
foreach ($scene in $manifest.scenes) {
    $lines.Add("### Cena $sceneIndex - $($scene.title)")
    $lines.Add("")
    $lines.Add("- torneio: $($scene.tournament)")
    $lines.Add("- tela: $($scene.screen)")
    $lines.Add("- duracao estimada: $($scene.durationSeconds)s")
    $lines.Add("- acao: $($scene.action)")
    $lines.Add("- resultado esperado: $($scene.expected)")
    $lines.Add("- narracao: $($scene.narration)")
    $lines.Add("")
    $sceneIndex++
}

Ensure-Directory -LiteralPath (Split-Path -Parent $OutputPath)
Set-Content -LiteralPath $OutputPath -Value $lines -Encoding UTF8
Write-VideoInfo "Roteiro Markdown gerado em: $OutputPath"
