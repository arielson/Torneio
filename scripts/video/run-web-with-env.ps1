Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath "$PSScriptRoot\..\..\artifacts\video\temp")) {
    New-Item -ItemType Directory -Path "$PSScriptRoot\..\..\artifacts\video\temp" | Out-Null
}

$transcriptStamp = Get-Date -Format "yyyyMMdd-HHmmss"
$transcriptPath = "$PSScriptRoot\..\..\artifacts\video\temp\run-web-with-env-$transcriptStamp.log"
Start-Transcript -Path $transcriptPath -Force | Out-Null

. "$PSScriptRoot\set-video-env.ps1"
try {
    & "$PSScriptRoot\run.ps1" -Target web -InstallDependencies -StartWeb
}
finally {
    Stop-Transcript | Out-Null
}
