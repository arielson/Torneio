param(
    [Parameter(Mandatory)]
    [string]$Target,

    [Parameter(Mandatory)]
    [string]$RawVideoPath,

    [Parameter(Mandatory)]
    [string]$SubtitlePath,

    [Parameter(Mandatory)]
    [string]$NarrationPath,

    [Parameter(Mandatory)]
    [string]$ScreenshotDir,

    [Parameter(Mandatory)]
    [string]$VoiceMetadataPath,

    [Parameter(Mandatory)]
    [string]$OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\common\helpers.ps1")
. (Join-Path $PSScriptRoot "..\common\media-utils.ps1")
. (Join-Path $PSScriptRoot "..\common\ffmpeg-tools.ps1")

function Get-SceneVideoFromScreenshots {
    param(
        [Parameter(Mandatory)]
        [string]$ShotsDir,

        [Parameter(Mandatory)]
        [string]$MetadataPath,

        [Parameter(Mandatory)]
        [string]$TempVideoPath
    )

    $metadata = Get-JsonFile -LiteralPath $MetadataPath
    $concatEntries = [System.Collections.Generic.List[string]]::new()

    foreach ($scene in $metadata.Scenes) {
        $shotPath = Join-Path $ShotsDir ("scene-{0:00}.png" -f [int]$scene.Index)
        if (-not (Test-Path -LiteralPath $shotPath)) {
            continue
        }

        $concatEntries.Add("file '$($shotPath.Replace("'", "''"))'")
        $concatEntries.Add(("duration {0}" -f ([double]$scene.DurationSeconds).ToString([System.Globalization.CultureInfo]::InvariantCulture)))
    }

    if ($concatEntries.Count -eq 0) {
        throw "Nenhum screenshot encontrado para gerar video alternativo em: $ShotsDir"
    }

    $lastScene = $metadata.Scenes | Select-Object -Last 1
    $lastShotPath = Join-Path $ShotsDir ("scene-{0:00}.png" -f [int]$lastScene.Index)
    if (Test-Path -LiteralPath $lastShotPath) {
        $concatEntries.Add("file '$($lastShotPath.Replace("'", "''"))'")
    }

    $concatPath = Join-Path (Split-Path -Parent $TempVideoPath) "images-concat.txt"
    Set-Content -LiteralPath $concatPath -Value $concatEntries -Encoding UTF8

    $ffmpegPath = Resolve-FfmpegPath
    & $ffmpegPath -y -f concat -safe 0 -i $concatPath -vsync vfr -pix_fmt yuv420p $TempVideoPath | Out-Null
}

Ensure-Directory -LiteralPath (Split-Path -Parent $OutputPath)
$effectiveVideoPath = $RawVideoPath

if (-not (Test-Path -LiteralPath $effectiveVideoPath)) {
    Write-VideoWarn "Video bruto nao encontrado. Vou gerar um video temporario a partir dos screenshots."
    $effectiveVideoPath = Join-Path (Split-Path -Parent $OutputPath) "fallback-scenes.mp4"
    Get-SceneVideoFromScreenshots -ShotsDir $ScreenshotDir -MetadataPath $VoiceMetadataPath -TempVideoPath $effectiveVideoPath
}

$subtitleFilterPath = Convert-ToFfmpegFilterPath -LiteralPath $SubtitlePath
$ffmpegPath = Resolve-FfmpegPath
$subtitleStyle = "FontName=Arial,FontSize=12,PrimaryColour=&H00FFFFFF,OutlineColour=&H00000000,BackColour=&H80000000,BorderStyle=3,Outline=1,Shadow=0,MarginV=8,Alignment=2"

& $ffmpegPath -y `
    -stream_loop -1 `
    -i $effectiveVideoPath `
    -i $NarrationPath `
    -vf "subtitles='$subtitleFilterPath':force_style='$subtitleStyle'" `
    -map 0:v:0 `
    -map 1:a:0 `
    -c:v libx264 `
    -pix_fmt yuv420p `
    -c:a aac `
    -shortest `
    $OutputPath | Out-Null

Write-VideoInfo "Video final gerado em: $OutputPath"
