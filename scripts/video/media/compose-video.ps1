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
    Set-Content -LiteralPath $concatPath -Value $concatEntries -Encoding Ascii

    $ffmpegPath = Resolve-FfmpegPath
    & $ffmpegPath -y -f concat -safe 0 -i $concatPath -vf "fps=30,format=yuv420p" -c:v libx264 -pix_fmt yuv420p $TempVideoPath | Out-Null
}

function Get-HybridSceneVideo {
    param(
        [Parameter(Mandatory)]
        [string]$RawVideoPath,

        [Parameter(Mandatory)]
        [string]$ShotsDir,

        [Parameter(Mandatory)]
        [string]$VoiceMetadataPath,

        [Parameter(Mandatory)]
        [string]$TimingMetadataPath,

        [string]$OverrideRawVideoPath = "",

        [string]$OverrideTimingMetadataPath = "",

        [Parameter(Mandatory)]
        [string]$TempVideoPath,

        [int[]]$RawSceneIndexes = @(10)
    )

    $voiceMetadata = Get-JsonFile -LiteralPath $VoiceMetadataPath
    $timingMetadata = Get-JsonFile -LiteralPath $TimingMetadataPath
    $sceneClipsRoot = Join-Path (Split-Path -Parent $TempVideoPath) "hybrid-clips"
    Ensure-Directory -LiteralPath $sceneClipsRoot
    $concatEntries = [System.Collections.Generic.List[string]]::new()
    $ffmpegPath = Resolve-FfmpegPath
    $culture = [System.Globalization.CultureInfo]::InvariantCulture

    foreach ($scene in $voiceMetadata.Scenes) {
        $sceneIndex = [int]$scene.Index
        $sceneClipPath = Join-Path $sceneClipsRoot ("scene-{0:00}.mp4" -f $sceneIndex)
        $duration = ([double]$scene.DurationSeconds).ToString($culture)

        if ($RawSceneIndexes -contains $sceneIndex) {
            $selectedRawVideoPath = $RawVideoPath
            $selectedTimingMetadata = $timingMetadata

            if (
                $sceneIndex -eq 10 -and
                -not [string]::IsNullOrWhiteSpace($OverrideRawVideoPath) -and
                -not [string]::IsNullOrWhiteSpace($OverrideTimingMetadataPath) -and
                (Test-Path -LiteralPath $OverrideRawVideoPath) -and
                (Test-Path -LiteralPath $OverrideTimingMetadataPath)
            ) {
                $selectedRawVideoPath = $OverrideRawVideoPath
                $selectedTimingMetadata = Get-JsonFile -LiteralPath $OverrideTimingMetadataPath
            }

            $timingScene = $selectedTimingMetadata.scenes | Where-Object { [int]$_.index -eq 1 -or [int]$_.index -eq $sceneIndex } | Select-Object -First 1
            if (-not $timingScene) {
                throw "Metadado de tempo nao encontrado para a cena $sceneIndex."
            }

            $start = ([double]$timingScene.startSeconds).ToString($culture)

            & $ffmpegPath -y `
                -ss $start `
                -i $selectedRawVideoPath `
                -t $duration `
                -an `
                -vf "fps=30,scale=trunc(iw/2)*2:trunc(ih/2)*2,format=yuv420p" `
                -c:v libx264 `
                -pix_fmt yuv420p `
                $sceneClipPath | Out-Null
        }
        else {
            $shotPath = Join-Path $ShotsDir ("scene-{0:00}.png" -f $sceneIndex)
            if (-not (Test-Path -LiteralPath $shotPath)) {
                throw "Screenshot nao encontrado para a cena $sceneIndex em: $shotPath"
            }

            & $ffmpegPath -y `
                -loop 1 `
                -i $shotPath `
                -t $duration `
                -an `
                -vf "fps=30,scale=trunc(iw/2)*2:trunc(ih/2)*2,format=yuv420p" `
                -c:v libx264 `
                -pix_fmt yuv420p `
                $sceneClipPath | Out-Null
        }

        $concatEntries.Add("file '$($sceneClipPath.Replace("'", "''"))'")
    }

    if ($concatEntries.Count -eq 0) {
        throw "Nenhum clip de cena foi gerado para a composicao hibrida."
    }

    $concatPath = Join-Path (Split-Path -Parent $TempVideoPath) "hybrid-concat.txt"
    Set-Content -LiteralPath $concatPath -Value $concatEntries -Encoding Ascii
    & $ffmpegPath -y -f concat -safe 0 -i $concatPath -an -c:v libx264 -pix_fmt yuv420p $TempVideoPath | Out-Null
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
    Set-Content -LiteralPath $concatPath -Value $concatEntries -Encoding Ascii
    & $ffmpegPath -y -f concat -safe 0 -i $concatPath -c copy $TempVideoPath | Out-Null
}

Ensure-Directory -LiteralPath (Split-Path -Parent $OutputPath)
$effectiveVideoPath = $RawVideoPath

if ($Target -eq "app") {
    Write-VideoInfo "Montando vídeo final do app com composicao hibrida, preservando a animacao real do sorteio."
    $effectiveVideoPath = Join-Path (Split-Path -Parent $OutputPath) "fallback-scenes.mp4"
    $overrideRawVideoPath = Join-Path (Split-Path -Parent $OutputPath) "raw\app-demo-sorteio-only.mp4"
    $overrideTimingMetadataPath = Join-Path (Split-Path -Parent $OutputPath) "app-sorteio-only-timings.json"
    Get-HybridSceneVideo -RawVideoPath $RawVideoPath -ShotsDir $ScreenshotDir -VoiceMetadataPath $VoiceMetadataPath -TimingMetadataPath $SceneTimingMetadataPath -TempVideoPath $effectiveVideoPath -OverrideRawVideoPath $overrideRawVideoPath -OverrideTimingMetadataPath $overrideTimingMetadataPath
}
elseif (-not (Test-Path -LiteralPath $effectiveVideoPath)) {
    Write-VideoWarn "Vídeo bruto não encontrado. Vou gerar um vídeo temporário a partir dos screenshots."
    $effectiveVideoPath = Join-Path (Split-Path -Parent $OutputPath) "fallback-scenes.mp4"
    Get-SceneVideoFromScreenshots -ShotsDir $ScreenshotDir -MetadataPath $VoiceMetadataPath -TempVideoPath $effectiveVideoPath
}

$subtitleFilterPath = Convert-ToFfmpegFilterPath -LiteralPath $SubtitlePath
$ffmpegPath = Resolve-FfmpegPath
$subtitleStyle = "FontName=Arial,FontSize=12,PrimaryColour=&H00FFFFFF,OutlineColour=&H00000000,BackColour=&H80000000,BorderStyle=3,Outline=1,Shadow=0,MarginV=8,Alignment=2"
$narrationDuration = (Get-MediaDurationSeconds -LiteralPath $NarrationPath).ToString([System.Globalization.CultureInfo]::InvariantCulture)

& $ffmpegPath -y `
    -i $effectiveVideoPath `
    -i $NarrationPath `
    -vf "subtitles='$subtitleFilterPath':force_style='$subtitleStyle'" `
    -map 0:v:0 `
    -map 1:a:0 `
    -c:v libx264 `
    -pix_fmt yuv420p `
    -c:a aac `
    -t $narrationDuration `
    -shortest `
    $OutputPath | Out-Null

Write-VideoInfo "Vídeo final gerado em: $OutputPath"
