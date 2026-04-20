param(
    [string]$ManifestPath = "",
    [string]$AvdName = "",
    [switch]$StartEmulator,
    [switch]$BuildAndInstallApp,
    [switch]$StartAppium,
    [int]$AppiumPort = 4723,
    [switch]$GenerateOnly,
    [switch]$InstallDependencies
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\common\paths.ps1")
. (Join-Path $PSScriptRoot "..\common\helpers.ps1")
. (Join-Path $PSScriptRoot "..\common\assert-tools.ps1")
. (Join-Path $PSScriptRoot "..\common\prepare-output.ps1")
. (Join-Path $PSScriptRoot "..\common\install-node-deps.ps1")
. (Join-Path $PSScriptRoot "..\common\android-tools.ps1")

$paths = Get-VideoPaths
if ([string]::IsNullOrWhiteSpace($ManifestPath)) {
    $ManifestPath = Join-Path $paths.ManifestRoot "app-demo.json"
}

$output = Initialize-VideoOutput -Target "app"
$manifest = Get-JsonFile -LiteralPath $ManifestPath

Write-VideoSection "Pipeline de video - App Android"
Assert-AppVideoTools

if ($InstallDependencies) {
    Install-VideoNodeDependencies -InstallAppiumDriver
}

$scriptOutputPath = Join-Path $output.TargetRoot "roteiro-app.md"
$subtitleOutputPath = Join-Path $output.SubtitleRoot "app-demo.srt"
$voiceOutputPath = Join-Path $output.AudioRoot "app-demo-narration.wav"
$voiceMetadataPath = Join-Path $output.AudioRoot "app-demo-narration.json"
$rawVideoOutputPath = Join-Path $output.TargetRoot "raw\app-demo.mp4"
$sceneOutputRoot = Join-Path $output.TargetRoot "screens"
$finalVideoOutputPath = Join-Path $output.TargetRoot "app-demo-final.mp4"

& (Join-Path $paths.MediaRoot "generate-script.ps1") -ManifestPath $ManifestPath -OutputPath $scriptOutputPath
& (Join-Path $paths.MediaRoot "generate-voice.ps1") -ManifestPath $ManifestPath -OutputPath $voiceOutputPath -MetadataPath $voiceMetadataPath
& (Join-Path $paths.MediaRoot "generate-subtitles.ps1") -ManifestPath $ManifestPath -OutputPath $subtitleOutputPath -VoiceMetadataPath $voiceMetadataPath

if ($GenerateOnly) {
    Write-VideoInfo "Pipeline encerrado em modo GenerateOnly."
    return
}

if ($StartEmulator) {
    & (Join-Path $PSScriptRoot "start-emulator.ps1") -AvdName $AvdName
}

if ($BuildAndInstallApp) {
    & (Join-Path $PSScriptRoot "build-install-app.ps1")
}

if ($StartAppium) {
    & (Join-Path $PSScriptRoot "start-appium.ps1") -Port $AppiumPort
}

$env:VIDEO_ADB_PATH = Resolve-AdbPath
$env:VIDEO_ADB_SERIAL = Resolve-EmulatorSerial
Write-VideoSection "Gravando fluxo Android com Appium"
& node (Join-Path $PSScriptRoot "record-app.mjs") --manifest $ManifestPath --outputDir $sceneOutputRoot --rawVideo $rawVideoOutputPath --port $AppiumPort
& (Join-Path $paths.MediaRoot "compose-video.ps1") `
    -Target "app" `
    -RawVideoPath $rawVideoOutputPath `
    -SubtitlePath $subtitleOutputPath `
    -NarrationPath $voiceOutputPath `
    -ScreenshotDir $sceneOutputRoot `
    -VoiceMetadataPath $voiceMetadataPath `
    -OutputPath $finalVideoOutputPath
Write-VideoInfo "Manifesto carregado: $($manifest.title)"
