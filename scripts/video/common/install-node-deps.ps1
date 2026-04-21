Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "paths.ps1")
. (Join-Path $PSScriptRoot "helpers.ps1")
. (Join-Path $PSScriptRoot "assert-tools.ps1")

function Install-VideoNodeDependencies {
    param(
        [switch]$InstallBrowsers,
        [switch]$InstallAppiumDriver
    )

    Assert-CommandAvailable -CommandName "node" -InstallHint "Instale o Node.js e adicione ao PATH."
    Assert-CommandAvailable -CommandName "npm" -InstallHint "Instale o npm junto com o Node.js."

    $paths = Get-VideoPaths
    $packageJsonPath = Join-Path $paths.ScriptsRoot "package.json"
    $appiumHome = Join-Path $paths.ScriptsRoot "node_modules\.cache\appium"
    Assert-PathExists -LiteralPath $packageJsonPath -Description "package.json do pipeline de video"
    Ensure-Directory -LiteralPath $appiumHome
    $env:APPIUM_HOME = $appiumHome

    Write-VideoSection "Instalando dependencias Node do pipeline"
    & npm install --prefix $paths.ScriptsRoot

    if ($InstallBrowsers) {
        Write-VideoSection "Instalando browsers do Playwright"
        & npm run install:browsers --prefix $paths.ScriptsRoot
    }

    if ($InstallAppiumDriver) {
        Write-VideoSection "Instalando driver UiAutomator2 do Appium"
        & npm run install:appium-driver --prefix $paths.ScriptsRoot
    }
}
