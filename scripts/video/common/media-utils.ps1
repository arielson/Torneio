Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot "helpers.ps1")
. (Join-Path $PSScriptRoot "ffmpeg-tools.ps1")

function Get-MediaDurationSeconds {
    param(
        [Parameter(Mandatory)]
        [string]$LiteralPath
    )

    Assert-PathExists -LiteralPath $LiteralPath -Description "Arquivo de midia"

    $ffprobePath = Resolve-FfprobePath
    $durationText = & $ffprobePath -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 $LiteralPath
    if (-not $durationText) {
        throw "Nao foi possivel obter a duracao de: $LiteralPath"
    }

    return [double]::Parse($durationText.Trim(), [System.Globalization.CultureInfo]::InvariantCulture)
}

function Convert-ToFfmpegFilterPath {
    param(
        [Parameter(Mandatory)]
        [string]$LiteralPath
    )

    $fullPath = [System.IO.Path]::GetFullPath($LiteralPath)
    $normalized = $fullPath.Replace('\', '/')
    if ($normalized.Length -ge 2 -and $normalized[1] -eq ':') {
        $normalized = $normalized[0] + '\:' + $normalized.Substring(2)
    }

    return $normalized.Replace('[', '\[').Replace(']', '\]').Replace(',', '\,').Replace("'", "\'")
}
