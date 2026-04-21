param(
    [int]$Port = 4723,
    [int]$StartupTimeoutSeconds = 30
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\common\helpers.ps1")
. (Join-Path $PSScriptRoot "..\common\paths.ps1")
. (Join-Path $PSScriptRoot "..\common\assert-tools.ps1")
. (Join-Path $PSScriptRoot "..\common\android-tools.ps1")

Assert-CommandAvailable -CommandName "node" -InstallHint "Instale o Node.js e adicione ao PATH."

$paths = Get-VideoPaths
$appiumScriptPath = Join-Path $paths.ScriptsRoot "node_modules\appium\build\lib\main.js"
$appiumHome = Join-Path $paths.ScriptsRoot "node_modules\.cache\appium"
$androidSdkRoot = Resolve-AndroidSdkRoot
Assert-PathExists -LiteralPath $appiumScriptPath -Description "Appium local do pipeline"
Ensure-Directory -LiteralPath $appiumHome
$env:APPIUM_HOME = $appiumHome
$env:ANDROID_HOME = $androidSdkRoot
$env:ANDROID_SDK_ROOT = $androidSdkRoot

Write-VideoSection "Inicializando servidor Appium"

$existing = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue | Select-Object -First 1
if ($existing) {
    Write-VideoInfo "Porta $Port ja esta em uso. Vou assumir que o Appium ja esta ativo."
    return
}

$logPath = Join-Path $paths.TempRoot "appium-$Port.log"
$stdoutLogPath = Join-Path $paths.TempRoot "appium-$Port.stdout.log"
$stderrLogPath = Join-Path $paths.TempRoot "appium-$Port.stderr.log"
Ensure-Directory -LiteralPath (Split-Path -Parent $logPath)

Start-Process -FilePath "node" -ArgumentList @($appiumScriptPath, "--port", "$Port") -RedirectStandardOutput $stdoutLogPath -RedirectStandardError $stderrLogPath | Out-Null

$deadline = (Get-Date).AddSeconds($StartupTimeoutSeconds)
while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds 2
    $tcp = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($tcp) {
        Write-VideoInfo "Appium ativo na porta $Port."
        return
    }
}

throw "O servidor Appium nao iniciou na porta $Port em $StartupTimeoutSeconds segundos."
