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
$deviceAbi = (& $adbPath -s $emulatorSerial shell getprop ro.product.cpu.abi).Trim()

function Resolve-FlutterTargetPlatform {
    param(
        [Parameter(Mandatory)]
        [string]$Abi
    )

    switch -Regex ($Abi) {
        '^x86_64$' { return 'android-x64' }
        '^arm64-v8a$' { return 'android-arm64' }
        '^armeabi-v7a$' { return 'android-arm' }
        default { return 'android-arm64' }
    }
}

$targetPlatform = Resolve-FlutterTargetPlatform -Abi $deviceAbi
$packageName = "com.example.torneio_app"

Write-VideoSection "Preparando app Android para automacao"
Write-VideoInfo "ABI detectada no emulador: $deviceAbi"
Write-VideoInfo "Target platform do Flutter: $targetPlatform"

if ($BuildApk -or -not (Test-Path -LiteralPath $apkPath)) {
    Write-VideoInfo "Gerando APK debug via Flutter."
    Push-Location $appRoot
    try {
        & $flutterPath build apk --debug --target-platform $targetPlatform --dart-define=VIDEO_DEMO_SORTEIO=true
    }
    finally {
        Pop-Location
    }
}

Assert-PathExists -LiteralPath $apkPath -Description "APK debug do app"
Write-VideoInfo "Instalando APK no emulador $emulatorSerial."
& $adbPath -s $emulatorSerial uninstall $packageName | Out-Null
& $adbPath -s $emulatorSerial install $apkPath | Out-Null
Write-VideoInfo "APK instalado com sucesso."
