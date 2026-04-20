Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot "helpers.ps1")

function Resolve-AndroidSdkRoot {
    $candidates = @(
        $env:ANDROID_SDK_ROOT,
        $env:ANDROID_HOME,
        (Join-Path $env:LOCALAPPDATA "Android\Sdk")
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return [System.IO.Path]::GetFullPath($candidate)
        }
    }

    throw "Android SDK nao encontrado. Defina ANDROID_SDK_ROOT/ANDROID_HOME ou instale o Android SDK."
}

function Resolve-AdbPath {
    $sdkRoot = Resolve-AndroidSdkRoot
    $candidate = Join-Path $sdkRoot "platform-tools\adb.exe"
    if (Test-Path -LiteralPath $candidate) {
        return $candidate
    }

    $command = Get-Command "adb" -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    throw "adb.exe nao encontrado."
}

function Get-ConnectedEmulatorSerials {
    $adbPath = Resolve-AdbPath
    $lines = & $adbPath devices
    return @(
        $lines |
            Where-Object { $_ -match '^emulator-\d+\s+device$' } |
            ForEach-Object { ($_ -split '\s+')[0] }
    )
}

function Resolve-EmulatorSerial {
    $serials = @(Get-ConnectedEmulatorSerials)
    if ($serials.Count -eq 0) {
        throw "Nenhum emulador conectado via adb foi encontrado."
    }

    return $serials[0]
}

function Resolve-EmulatorPath {
    $sdkRoot = Resolve-AndroidSdkRoot
    $candidate = Join-Path $sdkRoot "emulator\emulator.exe"
    if (Test-Path -LiteralPath $candidate) {
        return $candidate
    }

    $command = Get-Command "emulator" -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    throw "emulator.exe nao encontrado."
}

function Get-AvailableAvdNames {
    $emulatorPath = Resolve-EmulatorPath
    $output = & $emulatorPath -list-avds
    return @($output | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Resolve-AvdName {
    param(
        [string]$PreferredName = ""
    )

    $available = @(Get-AvailableAvdNames)
    if ($available.Count -eq 0) {
        throw "Nenhum AVD encontrado no Android Emulator."
    }

    if (-not [string]::IsNullOrWhiteSpace($PreferredName) -and ($available -contains $PreferredName)) {
        return $PreferredName
    }

    return $available[0]
}
