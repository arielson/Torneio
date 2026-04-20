param(
    [string]$AvdName = "",

    [int]$BootTimeoutSeconds = 180
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\common\helpers.ps1")
. (Join-Path $PSScriptRoot "..\common\assert-tools.ps1")
. (Join-Path $PSScriptRoot "..\common\android-tools.ps1")

Assert-AppVideoTools

Write-VideoSection "Inicializando Android Emulator"

$adbPath = Resolve-AdbPath
$emulatorPath = Resolve-EmulatorPath
$resolvedAvdName = Resolve-AvdName -PreferredName $AvdName
$existingDevice = @(Get-ConnectedEmulatorSerials)
if ($existingDevice) {
    Write-VideoInfo "Emulador ja detectado via adb: $($existingDevice[0])"
    return
}

Start-Process -FilePath $emulatorPath -ArgumentList @("-avd", $resolvedAvdName) | Out-Null
Write-VideoInfo "Aguardando boot do emulador: $resolvedAvdName"

$deadline = (Get-Date).AddSeconds($BootTimeoutSeconds)
$emulatorSerial = $null
while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds 5
    try {
        if (-not $emulatorSerial) {
            $serials = @(Get-ConnectedEmulatorSerials)
            if ($serials.Count -gt 0) {
                $emulatorSerial = $serials[0]
            }
        }

        if (-not $emulatorSerial) {
            continue
        }

        $bootState = (& $adbPath -s $emulatorSerial shell getprop sys.boot_completed 2>$null).Trim()
        if ($bootState -eq "1") {
            Write-VideoInfo "Emulador iniciado com sucesso: $emulatorSerial"
            return
        }
    }
    catch {
        Start-Sleep -Seconds 2
    }
}

throw "O emulador nao concluiu o boot em $BootTimeoutSeconds segundos."
