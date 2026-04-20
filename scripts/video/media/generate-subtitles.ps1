param(
    [Parameter(Mandatory)]
    [string]$ManifestPath,

    [Parameter(Mandatory)]
    [string]$OutputPath,

    [string]$VoiceMetadataPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\common\helpers.ps1")

function Format-SrtTime {
    param(
        [Parameter(Mandatory)]
        [double]$TotalSeconds
    )

    $timeSpan = [System.TimeSpan]::FromSeconds($TotalSeconds)
    $hours = [int][math]::Floor($timeSpan.TotalHours)
    $minutes = $timeSpan.Minutes
    $seconds = $timeSpan.Seconds
    $milliseconds = $timeSpan.Milliseconds
    $hourText = $hours.ToString("00", [System.Globalization.CultureInfo]::InvariantCulture)
    $minuteText = $minutes.ToString("00", [System.Globalization.CultureInfo]::InvariantCulture)
    $secondText = $seconds.ToString("00", [System.Globalization.CultureInfo]::InvariantCulture)
    $millisecondText = $milliseconds.ToString("000", [System.Globalization.CultureInfo]::InvariantCulture)
    return "$hourText`:$minuteText`:$secondText,$millisecondText"
}

$manifest = Get-JsonFile -LiteralPath $ManifestPath
$voiceMetadata = $null
if (-not [string]::IsNullOrWhiteSpace($VoiceMetadataPath) -and (Test-Path -LiteralPath $VoiceMetadataPath)) {
    $voiceMetadata = Get-JsonFile -LiteralPath $VoiceMetadataPath
}

$lines = [System.Collections.Generic.List[string]]::new()
$currentStart = 0.0
$index = 1

foreach ($scene in $manifest.scenes) {
    $sceneDuration = if ($voiceMetadata -and $voiceMetadata.Scenes.Count -ge $index) {
        [double]$voiceMetadata.Scenes[$index - 1].DurationSeconds
    }
    else {
        [double]$scene.durationSeconds
    }

    $start = Format-SrtTime -TotalSeconds $currentStart
    $currentStart += $sceneDuration
    $end = Format-SrtTime -TotalSeconds $currentStart

    $lines.Add([string]$index)
    $lines.Add("$start --> $end")
    $lines.Add([string]$scene.narration)
    $lines.Add("")
    $index++
}

Ensure-Directory -LiteralPath (Split-Path -Parent $OutputPath)
Set-Content -LiteralPath $OutputPath -Value $lines -Encoding UTF8
Write-VideoInfo "Legenda SRT gerada em: $OutputPath"
