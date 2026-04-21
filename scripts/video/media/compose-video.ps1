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

    [string]$SceneTimingMetadataPath = "",

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

function Get-SceneVideoFromRawVideo {
    param(
        [Parameter(Mandatory)]
        [string]$RawVideoPath,

        [Parameter(Mandatory)]
        [string]$TimingMetadataPath,

        [Parameter(Mandatory)]
        [string]$TempVideoPath
    )

    $timingMetadata = Get-JsonFile -LiteralPath $TimingMetadataPath
    $segmentsRoot = Join-Path (Split-Path -Parent $TempVideoPath) "segments"
    Ensure-Directory -LiteralPath $segmentsRoot
    $concatEntries = [System.Collections.Generic.List[string]]::new()
    $ffmpegPath = Resolve-FfmpegPath

    foreach ($scene in $timingMetadata.scenes) {
        $segmentPath = Join-Path $segmentsRoot ("scene-{0:00}.mp4" -f [int]$scene.index)
        $start = ([double]$scene.startSeconds).ToString([System.Globalization.CultureInfo]::InvariantCulture)
        $duration = ([double]($scene.endSeconds - $scene.startSeconds)).ToString([System.Globalization.CultureInfo]::InvariantCulture)

        & $ffmpegPath -y -ss $start -i $RawVideoPath -t $duration -an -c:v libx264 -pix_fmt yuv420p $segmentPath | Out-Null
        $concatEntries.Add("file '$($segmentPath.Replace("'", "''"))'")
    }

    if ($concatEntries.Count -eq 0) {
        throw "Nenhum segmento encontrado para gerar video a partir do bruto."
    }

    $concatPath = Join-Path (Split-Path -Parent $TempVideoPath) "segments-concat.txt"
    Set-Content -LiteralPath $concatPath -Value $concatEntries -Encoding UTF8
    & $ffmpegPath -y -f concat -safe 0 -i $concatPath -c copy $TempVideoPath | Out-Null
}

Ensure-Directory -LiteralPath (Split-Path -Parent $OutputPath)
$effectiveVideoPath = $RawVideoPath

if ($Target -eq "app" -and -not [string]::IsNullOrWhiteSpace($SceneTimingMetadataPath) -and (Test-Path -LiteralPath $SceneTimingMetadataPath) -and (Test-Path -LiteralPath $effectiveVideoPath)) {
    Write-VideoInfo "Montando video final do app a partir dos segmentos gravados por cena."
    $effectiveVideoPath = Join-Path (Split-Path -Parent $OutputPath) "scene-segments.mp4"
    Get-SceneVideoFromRawVideo -RawVideoPath $RawVideoPath -TimingMetadataPath $SceneTimingMetadataPath -TempVideoPath $effectiveVideoPath
}
elseif (-not (Test-Path -LiteralPath $effectiveVideoPath)) {
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
