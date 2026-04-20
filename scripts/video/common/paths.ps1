Set-StrictMode -Version Latest

function Get-VideoRepoRoot {
    return [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\..\.."))
}

function Get-VideoPaths {
    $repoRoot = Get-VideoRepoRoot
    $scriptsRoot = Join-Path $repoRoot "scripts\video"
    $artifactsRoot = Join-Path $repoRoot "artifacts\video"

    return @{
        RepoRoot = $repoRoot
        ScriptsRoot = $scriptsRoot
        CommonRoot = Join-Path $scriptsRoot "common"
        AppRoot = Join-Path $scriptsRoot "app"
        WebRoot = Join-Path $scriptsRoot "web"
        MediaRoot = Join-Path $scriptsRoot "media"
        ManifestRoot = Join-Path $scriptsRoot "manifests"
        ArtifactsRoot = $artifactsRoot
        AppArtifactsRoot = Join-Path $artifactsRoot "app"
        WebArtifactsRoot = Join-Path $artifactsRoot "web"
        AudioRoot = Join-Path $artifactsRoot "audio"
        SubtitleRoot = Join-Path $artifactsRoot "subtitles"
        TempRoot = Join-Path $artifactsRoot "temp"
    }
}
