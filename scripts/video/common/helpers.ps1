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
        throw "$Description não encontrado: $LiteralPath"
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
    $content = [System.IO.File]::ReadAllText($LiteralPath, [System.Text.Encoding]::UTF8)
    return $content | ConvertFrom-Json
}

function Get-Utf8NoBomEncoding {
    return [System.Text.UTF8Encoding]::new($false)
}

function Set-TextFileUtf8 {
    param(
        [Parameter(Mandatory)]
        [string]$LiteralPath,

        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string]$Content
    )

    [System.IO.File]::WriteAllText($LiteralPath, $Content, (Get-Utf8NoBomEncoding))
}
