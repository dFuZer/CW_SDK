# THIS SCRIPT COMPILES THE MOD AND UPDATES THE ASSET BUNDLES INSIDE THE WORKSHOP FOLDER FOR LOCAL TESTING.

# Before you can run this script, you need to build the mod in Debug + Release mode, and update the config.json file with the correct paths.
# You also need to publish your mod on the workshop, subscribe to it, and update the config.json file with the correct workshop folder path.
# You also need to build the asset bundles and update the config.json file with the correct asset bundle paths.
# Then, rename EXAMPLE.config.json to config.json

# Check if config file exists
$configPath = Join-Path $PSScriptRoot "config.json"
if (!(Test-Path $configPath)) {
    Write-Error "Config file not found ! Please rename EXAMPLE.config.json to config.json and update the paths !"
    exit 1
}
$config = Get-Content $configPath | ConvertFrom-Json

# Get the paths from the config file
$workshopFolder = $config.WorkshopModFolder
$assetBundlePath1 = $config.AssetBundlePath1
$assetBundlePath2 = $config.AssetBundlePath2
$debugDllPath = $config.DebugDllPath

# Build the mod (Debug)
dotnet build

# Check if the assetbundles, debug dll and workshop folder exist
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
if (!(Test-Path $debugDllPath)) {
    Write-Error "Debug DLL not found: $debugDllPath"
    exit 1
}

# Copy the debug dll and assetbundles to the workshop folder for local testing
Copy-Item $debugDllPath (Join-Path $workshopFolder (Split-Path $debugDllPath -Leaf)) -Force
Copy-Item $assetBundlePath1 (Join-Path $workshopFolder (Split-Path $assetBundlePath1 -Leaf)) -Force
Copy-Item $assetBundlePath2 (Join-Path $workshopFolder (Split-Path $assetBundlePath2 -Leaf)) -Force
