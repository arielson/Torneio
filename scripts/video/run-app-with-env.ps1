Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. "$PSScriptRoot\set-video-env.ps1"
& "$PSScriptRoot\run.ps1" -Target app -InstallDependencies -StartEmulator -BuildAndInstallApp -StartAppium
