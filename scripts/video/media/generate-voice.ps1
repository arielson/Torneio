param(
    [Parameter(Mandatory)]
    [string]$ManifestPath,

    [Parameter(Mandatory)]
    [string]$OutputPath,

    [Parameter(Mandatory)]
    [string]$MetadataPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\common\helpers.ps1")
. (Join-Path $PSScriptRoot "..\common\media-utils.ps1")
. (Join-Path $PSScriptRoot "..\common\ffmpeg-tools.ps1")

$manifest = Get-JsonFile -LiteralPath $ManifestPath
$outputDir = Split-Path -Parent $OutputPath
$outputStem = [System.IO.Path]::GetFileNameWithoutExtension($OutputPath)
$sceneAudioDir = Join-Path $outputDir "$outputStem-scenes"
$concatListPath = Join-Path $outputDir "$outputStem-concat.txt"

Ensure-Directory -LiteralPath $outputDir
Ensure-Directory -LiteralPath $sceneAudioDir

Add-Type -AssemblyName System.Speech
$culture = [System.Globalization.CultureInfo]::GetCultureInfo("pt-BR")
$synth = [System.Speech.Synthesis.SpeechSynthesizer]::new()
$synth.Rate = 0

$voiceName = [string]$manifest.voice.voiceName
$selectedVoice = $null
if (-not [string]::IsNullOrWhiteSpace($voiceName)) {
    $selectedVoice = $synth.GetInstalledVoices() |
        ForEach-Object { $_.VoiceInfo } |
        Where-Object { $_.Name -eq $voiceName } |
        Select-Object -First 1
}

if (-not $selectedVoice) {
    $selectedVoice = $synth.GetInstalledVoices() |
        ForEach-Object { $_.VoiceInfo } |
        Where-Object { $_.Culture.Name -eq $culture.Name } |
        Select-Object -First 1
}

if (-not $selectedVoice) {
    throw "Nenhuma voz instalada compatível com pt-BR foi encontrada no Windows."
}

$synth.SelectVoice($selectedVoice.Name)

$concatLines = [System.Collections.Generic.List[string]]::new()
$sceneMetadata = [System.Collections.Generic.List[object]]::new()
$index = 1

foreach ($scene in $manifest.scenes) {
    $scenePath = Join-Path $sceneAudioDir ("scene-{0:00}.wav" -f $index)
    $synth.SetOutputToWaveFile($scenePath)
    $synth.Speak([string]$scene.narration)
    $synth.SetOutputToNull()

    $durationSeconds = Get-MediaDurationSeconds -LiteralPath $scenePath
    $concatLines.Add("file '$($scenePath.Replace("'", "''"))'")
    $sceneMetadata.Add([pscustomobject]@{
            Index = $index
            Title = [string]$scene.title
            AudioPath = $scenePath
            DurationSeconds = [math]::Round($durationSeconds, 3)
        })
    $index++
}

Set-Content -LiteralPath $concatListPath -Value $concatLines -Encoding ASCII
$ffmpegPath = Resolve-FfmpegPath
& $ffmpegPath -y -f concat -safe 0 -i $concatListPath -c copy $OutputPath | Out-Null

$metadata = [pscustomobject]@{
    OutputPath = $OutputPath
    VoiceName = $selectedVoice.Name
    TotalDurationSeconds = [math]::Round((Get-MediaDurationSeconds -LiteralPath $OutputPath), 3)
    Scenes = $sceneMetadata
}

$metadataJson = $metadata | ConvertTo-Json -Depth 10
Set-TextFileUtf8 -LiteralPath $MetadataPath -Content ($metadataJson + [Environment]::NewLine)
Write-VideoInfo "Narração WAV gerada em: $OutputPath"
Write-VideoInfo "Metadados da narração gerados em: $MetadataPath"
