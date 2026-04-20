param(
    [Parameter(Mandatory)]
    [ValidateSet("app", "web")]
    [string]$Target,

    [string]$ManifestPath = "",
    [string]$AvdName = "",
    [switch]$StartEmulator,
    [switch]$BuildAndInstallApp,
    [switch]$StartWeb,
    [switch]$StartAppium,
    [int]$AppiumPort = 4723,
    [switch]$GenerateOnly,
    [switch]$InstallDependencies
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

switch ($Target) {
    "app" {
        & (Join-Path $scriptRoot "app\run.ps1") -ManifestPath $ManifestPath -AvdName $AvdName -StartEmulator:$StartEmulator -BuildAndInstallApp:$BuildAndInstallApp -StartAppium:$StartAppium -AppiumPort $AppiumPort -GenerateOnly:$GenerateOnly -InstallDependencies:$InstallDependencies
        break
    }
    "web" {
        & (Join-Path $scriptRoot "web\run.ps1") -ManifestPath $ManifestPath -GenerateOnly:$GenerateOnly -StartWeb:$StartWeb -InstallDependencies:$InstallDependencies
        break
    }
}
