param(
    [string]$ManifestPath = "",
    [switch]$GenerateOnly,
    [switch]$StartWeb,
    [switch]$InstallDependencies
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\common\paths.ps1")
. (Join-Path $PSScriptRoot "..\common\helpers.ps1")
. (Join-Path $PSScriptRoot "..\common\assert-tools.ps1")
. (Join-Path $PSScriptRoot "..\common\prepare-output.ps1")
. (Join-Path $PSScriptRoot "..\common\install-node-deps.ps1")

$paths = Get-VideoPaths
if ([string]::IsNullOrWhiteSpace($ManifestPath)) {
    $ManifestPath = Join-Path $paths.ManifestRoot "web-demo.json"
}

$output = Initialize-VideoOutput -Target "web"
$manifest = Get-JsonFile -LiteralPath $ManifestPath

Write-VideoSection "Pipeline de video - Web"
Assert-WebVideoTools

if ($InstallDependencies) {
    Install-VideoNodeDependencies -InstallBrowsers
}

$scriptOutputPath = Join-Path $output.TargetRoot "roteiro-web.md"
$subtitleOutputPath = Join-Path $output.SubtitleRoot "web-demo.srt"
$voiceOutputPath = Join-Path $output.AudioRoot "web-demo-narration.wav"
$voiceMetadataPath = Join-Path $output.AudioRoot "web-demo-narration.json"
$rawVideoOutputPath = Join-Path $output.TargetRoot "raw\web-demo.mp4"
$sceneOutputRoot = Join-Path $output.TargetRoot "screens"
$finalVideoOutputPath = Join-Path $output.TargetRoot "web-demo-final.mp4"

& (Join-Path $paths.MediaRoot "generate-script.ps1") -ManifestPath $ManifestPath -OutputPath $scriptOutputPath
& (Join-Path $paths.MediaRoot "generate-voice.ps1") -ManifestPath $ManifestPath -OutputPath $voiceOutputPath -MetadataPath $voiceMetadataPath
& (Join-Path $paths.MediaRoot "generate-subtitles.ps1") -ManifestPath $ManifestPath -OutputPath $subtitleOutputPath -VoiceMetadataPath $voiceMetadataPath

if ($GenerateOnly) {
    Write-VideoInfo "Pipeline encerrado em modo GenerateOnly."
    return
}

if ($StartWeb) {
    & (Join-Path $PSScriptRoot "start-web.ps1") -BaseUrl $manifest.baseUrl
}

& (Join-Path $PSScriptRoot "record-web.ps1") -ManifestPath $ManifestPath -OutputPath $rawVideoOutputPath -ScreenshotDir $sceneOutputRoot
& (Join-Path $paths.MediaRoot "compose-video.ps1") `
    -Target "web" `
    -RawVideoPath $rawVideoOutputPath `
    -SubtitlePath $subtitleOutputPath `
    -NarrationPath $voiceOutputPath `
    -ScreenshotDir $sceneOutputRoot `
    -VoiceMetadataPath $voiceMetadataPath `
    -OutputPath $finalVideoOutputPath
Write-VideoInfo "Manifesto carregado: $($manifest.title)"
