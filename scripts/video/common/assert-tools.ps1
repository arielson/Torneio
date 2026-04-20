Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot "helpers.ps1")
. (Join-Path $PSScriptRoot "android-tools.ps1")
. (Join-Path $PSScriptRoot "ffmpeg-tools.ps1")

function Test-CommandAvailable {
    param(
        [Parameter(Mandatory)]
        [string]$CommandName
    )

    return $null -ne (Get-Command $CommandName -ErrorAction SilentlyContinue)
}

function Assert-CommandAvailable {
    param(
        [Parameter(Mandatory)]
        [string]$CommandName,

        [Parameter(Mandatory)]
        [string]$InstallHint
    )

    if (-not (Test-CommandAvailable -CommandName $CommandName)) {
        throw "Comando obrigatorio ausente: $CommandName. $InstallHint"
    }
}

function Assert-BaseVideoTools {
    $null = Resolve-FfmpegPath
    $null = Resolve-FfprobePath
    Assert-CommandAvailable -CommandName "node" -InstallHint "Instale o Node.js e adicione ao PATH."
    Assert-CommandAvailable -CommandName "npm" -InstallHint "Instale o npm junto com o Node.js."
}

function Assert-WebVideoTools {
    Assert-BaseVideoTools
}

function Assert-AppVideoTools {
    Assert-BaseVideoTools
    $null = Resolve-AdbPath
    $null = Resolve-EmulatorPath
}
