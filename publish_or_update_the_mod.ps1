# Publish or update your mod on Steam Workshop.
# 1. Run .\prepare_final_build.ps1 to stage the Release build.
# 2. Update steam-build\mod.vdf (contentfolder, previewfile, publishedfileid).
# 3. Rename this file to publish_or_update_the_mod.ps1 and update config.json.
# 4. Run: .\publish_or_update_the_mod.ps1

$configPath = Join-Path $PSScriptRoot "config.json"
if (!(Test-Path $configPath)) {
    Write-Error "Config file not found ! Please rename EXAMPLE.config.json to config.json and update the paths !"
    exit 1
}
$config = Get-Content $configPath | ConvertFrom-Json
$steamCmd = $config.SteamCmdPath
$steamUsername = $config.SteamUsername

$modVdf = Join-Path $PSScriptRoot "steam-build\mod.vdf"

if (!(Test-Path $steamCmd)) {
    Write-Error "steamcmd.exe not found: $steamCmd"
    exit 1
}
if (!(Test-Path $modVdf)) {
    Write-Error "mod.vdf not found: $modVdf"
    exit 1
}

& $steamCmd +login $steamUsername +workshop_build_item $modVdf +quit
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
