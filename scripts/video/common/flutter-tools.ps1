Set-StrictMode -Version Latest

function Resolve-FlutterPath {
    $candidates = @(
        (Get-Command "flutter" -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source -ErrorAction SilentlyContinue),
        "C:\Users\ariel\develop\flutter\bin\flutter.bat",
        (Join-Path $env:USERPROFILE "flutter\bin\flutter.bat"),
        (Join-Path $env:LOCALAPPDATA "Flutter\bin\flutter.bat"),
        (Join-Path $env:USERPROFILE "fvm\default\bin\flutter.bat")
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return [System.IO.Path]::GetFullPath($candidate)
        }
    }

    throw "Flutter nao encontrado."
}
