param(
    [switch]$BuildApk
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\common\helpers.ps1")
. (Join-Path $PSScriptRoot "..\common\paths.ps1")
. (Join-Path $PSScriptRoot "..\common\android-tools.ps1")
. (Join-Path $PSScriptRoot "..\common\flutter-tools.ps1")

$paths = Get-VideoPaths
$appRoot = Join-Path $paths.RepoRoot "App\torneio_app"
$flutterPath = Resolve-FlutterPath
$adbPath = Resolve-AdbPath
$emulatorSerial = Resolve-EmulatorSerial
$apkPath = Join-Path $appRoot "build\app\outputs\flutter-apk\app-debug.apk"

Write-VideoSection "Preparando app Android para automacao"

if ($BuildApk -or -not (Test-Path -LiteralPath $apkPath)) {
    Write-VideoInfo "Gerando APK debug via Flutter."
    Push-Location $appRoot
    try {
        & $flutterPath build apk --debug
    }
    finally {
        Pop-Location
    }
}

Assert-PathExists -LiteralPath $apkPath -Description "APK debug do app"
Write-VideoInfo "Instalando APK no emulador $emulatorSerial."
& $adbPath -s $emulatorSerial install -r $apkPath | Out-Null
Write-VideoInfo "APK instalado com sucesso."
