$resourcesDir = $PSScriptRoot
$rcFile = Join-Path $resourcesDir "Resources.rc"
$resFile = Join-Path $resourcesDir "Resources.res"

function Invoke-Rc {
    $windowsKitsPath = "C:\Program Files (x86)\Windows Kits"

    if (-not (Test-Path $windowsKitsPath)) {
        return $false
    }

    # Find all rc.exe files in Windows SDK
    $rcExePaths = Get-ChildItem -Path $windowsKitsPath -Filter "rc.exe" -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match "\\bin\\.*\\(x64|x86)\\rc\.exe$" } |
        Sort-Object {
            # Extract version number from path for sorting
            if ($_.FullName -match "\\bin\\([\d\.]+)\\") {
                [version]$matches[1]
            }
        } -Descending

    # Get the latest rc.exe (prefer x64 over x86 for the same version)
    $latestRcExe = $rcExePaths |
        Where-Object { $_.FullName -match "\\x64\\rc\.exe$" } |
        Select-Object -First 1

    if (-not $latestRcExe) {
        $latestRcExe = $rcExePaths | Select-Object -First 1
    }

    if (-not $latestRcExe) {
        return $false
    }

    Write-Host "Using rc.exe: $($latestRcExe.FullName)"

    $includeArgs = @()

    # Extract SDK version from rc.exe path to build include paths
    if ($latestRcExe.FullName -match "\\bin\\([\d\.]+)\\") {
        $sdkVersion = $matches[1]
        # rc.exe is typically at: C:\...\Windows Kits\10\bin\10.0.26100.0\x64\rc.exe
        # We need to go up to C:\...\Windows Kits\10
        $x64Folder = Split-Path $latestRcExe.FullName -Parent
        $versionFolder = Split-Path $x64Folder -Parent
        $binFolder = Split-Path $versionFolder -Parent
        $sdkRoot = Split-Path $binFolder -Parent

        # Construct include paths for Windows SDK
        $includePaths = @(
            "$sdkRoot\Include\$sdkVersion\um",
            "$sdkRoot\Include\$sdkVersion\shared"
        )

        foreach ($path in $includePaths) {
            $includeArgs += "/I"
            $includeArgs += $path
        }

        Write-Host "SDK Version: $sdkVersion"
    }

    $allArgs = $includeArgs + @($rcFile)
    & $latestRcExe.FullName @allArgs

    return $LASTEXITCODE -eq 0
}

function Invoke-Windres {
    $windresCandidates = @("x86_64-w64-mingw32-windres", "i686-w64-mingw32-windres")

    foreach ($windres in $windresCandidates) {
        $windresCmd = Get-Command $windres -ErrorAction SilentlyContinue

        if ($windresCmd) {
            $windresPath = $windresCmd.Source
            Write-Host "Using windres: $windresPath"

            & $windresPath -i $rcFile -o $resFile -O res

            if ($LASTEXITCODE -eq 0) {
                return $true
            }

            Write-Warning "$windres failed with exit code: $LASTEXITCODE."
        }
    }

    return $false
}

if (Invoke-Rc) {
    Write-Host "Resource compilation completed successfully using rc.exe."
    exit 0
}

if (Invoke-Windres) {
    Write-Host "Resource compilation completed successfully using windres."
    exit 0
}

Write-Error "Could not compile resources: neither rc.exe nor windres was found or succeeded.`n$(
    if ($IsWindows) {
        "Install the Windows SDK (includes rc.exe):`n" +
        "  winget install Microsoft.WindowsSDK.10.0.26100"
    } elseif ($IsLinux) {
        "Install mingw-w64 (includes windres):`n" +
        "  sudo apt install mingw-w64"
    } elseif ($IsMacOS) {
        "Install mingw-w64 (includes windres):`n" +
        "  brew install mingw-w64"
    } else {
        "Install the Windows SDK (rc.exe) or mingw-w64 (windres)."
    }
)"
exit 1
