param(
    [switch]$InstallMissing,
    [switch]$InstallNodeDependencies,
    [switch]$InstallPlaywrightBrowsers,
    [switch]$InstallAppiumDriver
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "helpers.ps1")
. (Join-Path $PSScriptRoot "assert-tools.ps1")
. (Join-Path $PSScriptRoot "install-node-deps.ps1")
. (Join-Path $PSScriptRoot "android-tools.ps1")
. (Join-Path $PSScriptRoot "ffmpeg-tools.ps1")

Write-VideoSection "Validando ferramentas do pipeline"

$checks = @(
    @{ Name = "ffmpeg"; Hint = "winget install Gyan.FFmpeg" },
    @{ Name = "ffprobe"; Hint = "Instalado junto com FFmpeg" },
    @{ Name = "node"; Hint = "winget install OpenJS.NodeJS.LTS" },
    @{ Name = "npm"; Hint = "Instalado junto com Node.js" },
    @{ Name = "adb"; Hint = "Android SDK Platform Tools" },
    @{ Name = "emulator"; Hint = "Android Emulator via Android Studio" }
)

foreach ($check in $checks) {
    if ($check.Name -eq "ffmpeg") {
        try {
            $resolved = Resolve-FfmpegPath
            Write-VideoInfo "ffmpeg encontrado em $resolved"
            continue
        }
        catch {
        }
    }

    if ($check.Name -eq "ffprobe") {
        try {
            $resolved = Resolve-FfprobePath
            Write-VideoInfo "ffprobe encontrado em $resolved"
            continue
        }
        catch {
        }
    }

    if ($check.Name -eq "adb") {
        try {
            $resolved = Resolve-AdbPath
            Write-VideoInfo "adb encontrado em $resolved"
            continue
        }
        catch {
        }
    }

    if ($check.Name -eq "emulator") {
        try {
            $resolved = Resolve-EmulatorPath
            Write-VideoInfo "emulator encontrado em $resolved"
            continue
        }
        catch {
        }
    }

    if (Test-CommandAvailable -CommandName $check.Name) {
        Write-VideoInfo "$($check.Name) encontrado."
        continue
    }

    if (-not $InstallMissing) {
        Write-VideoWarn "$($check.Name) ausente. Sugestao: $($check.Hint)"
        continue
    }

    Write-VideoWarn "$($check.Name) ausente. Sugestao: $($check.Hint)"
}

Write-VideoInfo "Validacao concluida."

if ($InstallMissing) {
    $ffmpegMissing = $false
    try {
        $null = Resolve-FfmpegPath
    }
    catch {
        $ffmpegMissing = $true
    }
}

if ($InstallMissing -and $ffmpegMissing -and (Get-Command "winget" -ErrorAction SilentlyContinue)) {
    Write-VideoSection "Instalando FFmpeg via winget"
    & winget install --id Gyan.FFmpeg -e --accept-package-agreements --accept-source-agreements
}

if ($InstallNodeDependencies) {
    Install-VideoNodeDependencies -InstallBrowsers:$InstallPlaywrightBrowsers -InstallAppiumDriver:$InstallAppiumDriver
}
