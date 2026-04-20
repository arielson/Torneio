Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot "paths.ps1")
. (Join-Path $PSScriptRoot "helpers.ps1")

function Initialize-VideoOutput {
    param(
        [Parameter(Mandatory)]
        [ValidateSet("app", "web")]
        [string]$Target
    )

    $paths = Get-VideoPaths
    $targetRoot = if ($Target -eq "app") { $paths.AppArtifactsRoot } else { $paths.WebArtifactsRoot }

    Ensure-Directory -LiteralPath $paths.ArtifactsRoot
    Ensure-Directory -LiteralPath $targetRoot
    Ensure-Directory -LiteralPath $paths.AudioRoot
    Ensure-Directory -LiteralPath $paths.SubtitleRoot
    Ensure-Directory -LiteralPath $paths.TempRoot

    return @{
        TargetRoot = $targetRoot
        AudioRoot = $paths.AudioRoot
        SubtitleRoot = $paths.SubtitleRoot
        TempRoot = $paths.TempRoot
    }
}
