param(
    [Parameter(Mandatory)]
    [string]$OutputPath,

    [int]$MaxSeconds = 180
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\common\helpers.ps1")
. (Join-Path $PSScriptRoot "..\common\assert-tools.ps1")

Assert-AppVideoTools
Ensure-Directory -LiteralPath (Split-Path -Parent $OutputPath)

$remotePath = "/sdcard/demo-recording.mp4"
Write-VideoSection "Captura base do emulador"
Write-VideoInfo "Comando previsto para gravacao bruta:"
Write-Host "adb shell screenrecord --time-limit $MaxSeconds $remotePath"
Write-VideoInfo "Depois disso, o arquivo deve ser copiado para: $OutputPath"
