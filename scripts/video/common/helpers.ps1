Set-StrictMode -Version Latest

function Write-VideoSection {
    param(
        [Parameter(Mandatory)]
        [string]$Message
    )

    Write-Host ""
    Write-Host "== $Message ==" -ForegroundColor Cyan
}

function Write-VideoInfo {
    param(
        [Parameter(Mandatory)]
        [string]$Message
    )

    Write-Host "[info] $Message"
}

function Write-VideoWarn {
    param(
        [Parameter(Mandatory)]
        [string]$Message
    )

    Write-Host "[warn] $Message" -ForegroundColor Yellow
}

function Assert-PathExists {
    param(
        [Parameter(Mandatory)]
        [string]$LiteralPath,

        [Parameter(Mandatory)]
        [string]$Description
    )

    if (-not (Test-Path -LiteralPath $LiteralPath)) {
        throw "$Description nao encontrado: $LiteralPath"
    }
}

function Ensure-Directory {
    param(
        [Parameter(Mandatory)]
        [string]$LiteralPath
    )

    if (-not (Test-Path -LiteralPath $LiteralPath)) {
        New-Item -ItemType Directory -Path $LiteralPath | Out-Null
    }
}

function Get-JsonFile {
    param(
        [Parameter(Mandatory)]
        [string]$LiteralPath
    )

    Assert-PathExists -LiteralPath $LiteralPath -Description "Arquivo JSON"
    return Get-Content -LiteralPath $LiteralPath -Raw | ConvertFrom-Json
}
