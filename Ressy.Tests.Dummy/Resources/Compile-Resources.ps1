# Locate the latest rc.exe from Windows SDK
$windowsKitsPath = "C:\Program Files (x86)\Windows Kits"

if (-not (Test-Path $windowsKitsPath)) {
    Write-Error "Windows SDK not found at: $windowsKitsPath."
    exit 1
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

if ($rcExePaths.Count -eq 0) {
    Write-Error "rc.exe not found in Windows SDK."
    exit 1
}

# Get the latest rc.exe (prefer x64 over x86 for the same version)
$latestRcExe = $rcExePaths |
    Where-Object { $_.FullName -match "\\x64\\rc\.exe$" } |
    Select-Object -First 1

if (-not $latestRcExe) {
    $latestRcExe = $rcExePaths | Select-Object -First 1
}

Write-Host "Using rc.exe: $($latestRcExe.FullName)"

# Extract SDK version from rc.exe path
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

    # Build /I arguments for each include path
    $includeArgs = @()
    foreach ($path in $includePaths) {
        $includeArgs += "/I"
        $includeArgs += $path
    }

    Write-Host "SDK Version: $sdkVersion"
    Write-Host "Include paths:"
    $includePaths | ForEach-Object { Write-Host "  $_" }
}

# Run rc.exe on Resources.rc with include paths
$allArgs = $includeArgs + @((Join-Path $PSScriptRoot "Resources.rc"))
& $latestRcExe.FullName @allArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "rc.exe failed with exit code: $LASTEXITCODE."
    exit $LASTEXITCODE
}

Write-Host "Resource compilation completed successfully."
