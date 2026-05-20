# Check if config file exists
$configPath = Join-Path $PSScriptRoot "config.json"
if (!(Test-Path $configPath)) {
    Write-Error "Config file not found ! Please rename config.json.example to config.json and update the paths !"
    exit 1
}
$config = Get-Content $configPath | ConvertFrom-Json

# Get the paths from the config file
$workshopFolder = $config.WorkshopModFolder
$assetBundlePath1 = $config.AssetBundlePath1
$assetBundlePath2 = $config.AssetBundlePath2
$buildDllPath = $config.BuildDllPath

# Build the mod
dotnet build

# Check if the assetbundles, build dll and workshop folder exist
if (!(Test-Path $workshopFolder)) {
    Write-Error "Workshop mod folder not found: $workshopFolder"
    exit 1
}
if (!(Test-Path $assetBundlePath2)) {
    Write-Error "Asset bundle not found: $assetBundlePath2"
    exit 1
}
if (!(Test-Path $assetBundlePath1)) {
    Write-Error "Asset bundle not found: $assetBundlePath1"
    exit 1
}
if (!(Test-Path $buildDllPath)) {
    Write-Error "Build DLL not found: $buildDllPath"
    exit 1
}

# Copy the build dll and assetbundles to the workshop folder
Copy-Item $buildDllPath (Join-Path $workshopFolder (Split-Path $buildDllPath -Leaf)) -Force
Copy-Item $assetBundlePath1 (Join-Path $workshopFolder (Split-Path $assetBundlePath1 -Leaf)) -Force
Copy-Item $assetBundlePath2 (Join-Path $workshopFolder (Split-Path $assetBundlePath2 -Leaf)) -Force

# Clean the steam-build folder
$steamBuildFolder = Join-Path $PSScriptRoot "steam-build\build"
if (Test-Path $steamBuildFolder) {
    Remove-Item (Join-Path $steamBuildFolder "*") -Recurse -Force
}

# Copy the build dll and assetbundles to the steam-build folder to stage them for publishing
Copy-Item $buildDllPath (Join-Path $steamBuildFolder (Split-Path $buildDllPath -Leaf)) -Force
Copy-Item $assetBundlePath1 (Join-Path $steamBuildFolder (Split-Path $assetBundlePath1 -Leaf)) -Force
Copy-Item $assetBundlePath2 (Join-Path $steamBuildFolder (Split-Path $assetBundlePath2 -Leaf)) -Force
