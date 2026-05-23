# Content Warning Mod SDK

## Start here — video tutorial

**Watch this first.** The tutorial walks through everything in this SDK and how to get started modding Content Warning end to end:

> **TODO:** Replace this placeholder with your tutorial link.  
> **[YouTube — Full Content Warning modding tutorial](https://youtu.be/Jd8gq_ZmvmA)**

It covers setup, Unity, asset bundles, building, local testing, and publishing to Steam Workshop. The rest of this README is a written reference you can come back to while you work.

---

**Platform:** **Windows only.** This SDK and tutorial do not support Linux or macOS.

**Game version:** latest Steam build only.

**Audience:** beginners building **Steam Workshop** mods with a C# DLL, Unity asset bundles, and **[DbsContentApi](https://steamcommunity.com/sharedfiles/filedetails/?id=3680666123)**. You do not need BepInEx to play or ship mods — BepInEx tooling here is only for **publicizing** game assemblies at compile time.

---

## License

You may use, modify, and distribute this template however you want. No warranty — use at your own risk.

---

## What you will ship

Every published mod is a **Workshop item** folder containing:

| File | Purpose |
|------|---------|
| `YourMod.dll` | C# plugin loaded by the game |
| `example_mod` (rename when you customize) | Asset bundle: item/mob prefabs, icons, etc. |
| `example_scene` (rename when you customize) | Asset bundle: your custom map scene |

Players subscribe on Workshop. **Everyone in a lobby who uses your mod (or any mod that registers maps/mobs/items) must also subscribe to [DbsContentApi](https://steamcommunity.com/sharedfiles/filedetails/?id=3680666123)** and load it **before** your mod in Content Warning's Workshop load order.

---

## Before you start

Install or prepare:

1. **Windows PC**
2. **Content Warning** (Steam, up to date)
3. **[.NET SDK](https://dotnet.microsoft.com/download)** — to run `dotnet build`
4. **PowerShell** — all helper scripts are PowerShell on Windows
5. **[SteamCMD](https://developer.valvesoftware.com/wiki/SteamCMD)** — to upload Workshop items
6. **[DbsContentApi](https://steamcommunity.com/sharedfiles/filedetails/?id=3680666123)** — subscribe in Steam Workshop
7. **Unity 6.x** — for building asset bundles (any Unity 6 version is fine)
8. **AssetRipper** — to create a Unity project from the game (covered in the video above)

---

## Get this template

You can **download as ZIP**, **clone**, or **fork** this repository. It is not a library you reference — it **is** your mod project.

### Rename your mod (recommended early)

Change the example names so your Workshop item and DLL are unique:

| Location | What to change |
|----------|----------------|
| `ExampleMod.csproj` | `<AssemblyName>`, `<Product>`, `<RootNamespace>` |
| `MyPluginInfo.cs` | `PLUGIN_GUID`, `PLUGIN_NAME`, `PLUGIN_VERSION` |
| `Logger.cs` | Log prefix string |
| Namespace / folder | `ExampleMod` → your mod name (optional but tidy) |
| `steam-build/mod.vdf` | `title`, `description`, paths |
| Asset bundle names | `example_mod` / `example_scene` in Unity **and** in `Modules/CustomContent.cs` if you rename them |

`PLUGIN_GUID` should be **globally unique** across the modding community (reverse-domain style, e.g. `com.yourname.coolmod`).

---

## Step 1 — Install DbsContentApi (modders and players)

1. Open [BD's content API on Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3680666123).
2. Click **Subscribe**.
3. Launch Content Warning and let Workshop content download.
4. In the game's **Workshop / mod load order**, ensure **DbsContentApi loads first**, then your mod.

**Modders:** you need DbsContentApi installed for **building** (DLL reference) and **running** tests.

**Players:** anyone joining a lobby with mods that use DbsContentApi must have it subscribed.

Locate the installed API DLL (path varies by machine), typically under:

`Steam/steamapps/workshop/content/2881650/<DbsContentApiWorkshopId>/`

Point `DbsContentApiDir` in `ExampleMod.csproj` at the folder containing `DbsContentApi.dll` (do not copy the DLL into this repo).

Also set `CWDir` in `ExampleMod.csproj` to your game install, e.g.:

`C:\Program Files (x86)\Steam\steamapps\common\Content Warning`

---

## Step 2 — Configure `config.json`

1. Copy `EXAMPLE.config.json` → `config.json` (same folder as the `.ps1` scripts).
2. Fill in every path for **your** machine.

`config.json` is **gitignored** on purpose — each developer keeps their own paths.

| Key | Meaning |
|-----|---------|
| `AssetBundlePath1` | Full path to the `example_scene` bundle file after Unity build |
| `AssetBundlePath2` | Full path to the `example_mod` bundle file after Unity build |
| `DebugDllPath` | `bin/Debug/netstandard2.1/YourMod.dll` after `dotnet build` |
| `ReleaseDllPath` | `bin/Release/netstandard2.1/YourMod.dll` after `dotnet build -c Release` |
| `WorkshopModFolder` | Your mod's Workshop install folder (see First publish below) |
| `SteamCmdPath` | Path to `steamcmd.exe` |
| `SteamUsername` | Steam **login name** (not display name) |

Bundle paths usually look like:

`.../Assets/AssetBundles/example_scene` and `.../Assets/AssetBundles/example_mod`

---

## Step 3 — Unity content (`UNITY_CWSDK`)

### Create the Unity project

1. Use **AssetRipper** on Content Warning to export a Unity project (see video above).
2. Open the project in **Unity 6**.
3. Copy the entire **`UNITY_CWSDK`** folder from this repo into your Unity project's **`Assets`** folder.
4. Copy **`UNITY_CWSDK/PUT_MY_CONTENT_IN_EDITOR_FOLDER/CreateAssetBundles.cs`** into `Assets/Editor/` (create the folder if needed). This adds the menu **Assets → Build AssetBundles**.

### Asset bundle layout (important)

| Bundle name | Contains |
|-------------|----------|
| **`example_mod`** | Item prefabs, mob prefabs, icons — anything that is not the map scene |
| **`example_scene`** | **One** custom map scene (e.g. `TestScene`) |

Assign assets to these bundle names in the Unity Inspector (bottom Asset Labels).

**`MAP_PREFABS`** (in `UNITY_CWSDK/MAP_PREFABS/`) are **mandatory** for custom maps. Include the required prefabs in your map scene / bundle setup as shown in the template.

Example content under `UNITY_CWSDK/EXAMPLES/` (map, mob, gun, item) is **optional** — use it as a starting point or ignore it and register your own prefabs later.

### Build bundles

In Unity: **Assets → Build AssetBundles**. Output goes to `Assets/AssetBundles/`. Update `config.json` with the full paths to `example_mod` and `example_scene`.

### Custom mobs (extra note)

If you author new monsters, you may need to edit `RigCreator.cs` in your Unity project — see `UNITY_CWSDK/note.txt` for the `[ContextMenu("Generate Rig")]` hint.

---

## Step 4 — C# mod project

### Entry point

`ExampleMod.cs` is discovered via `[ContentWarningPlugin]`. On load it can configure DbsContentApi flags and call `CustomContent.Init()`.

### `DEBUG_MODE`

`ExampleMod.DEBUG_MODE` is `true` in the template. Set it to **`false` for release builds**. You can branch your own code on this flag any way you like (extra logging, cheat shop prices, etc.).

### Harmony (optional example)

`Patches/HigherJumpingPatch.cs` only shows how to patch game code with Harmony — higher jump for human players. Use it as **inspiration**; you do not have to ship any patches. Patches with `[HarmonyPatch]` are applied automatically when the assembly loads.

### `VANILLA_COMPATIBLE`

In `MyPluginInfo.cs`, keep **`VANILLA_COMPATIBLE = false`** whenever you register a **map, mob, or item**. That content must exist on **all clients**, not host-only. Only set compatibility to `true` for mods that do not add synced custom entities (rare for this SDK).

### Registering content (surface level)

Registration lives in `Modules/CustomContent.cs`:

- **Map** — load `example_scene` bundle, find scene path, `Maps.RegisterMap(...)`.
- **Mob** — load prefab from `example_mod`, `Mobs.RegisterMonster(...)` with a `MobSetupConfig`.
- **Items** — load prefabs, add behaviours, `Items.RegisterItem(...)` with `ItemConfig`.

You can delete or comment out example registrations and only ship a map, or only items, etc. For deeper APIs, read **DbsContentApi** source or Workshop docs.

### Reusing vanilla game assets (advanced, optional)

The example gun clones the Dog's laser projectile from `Resources` instead of authoring a new projectile prefab. That works but is a bit **hacky** — fine for learning, not ideal for every mod. Curious modders can trace `CustomContent.cs` and the DbsContentApi repo for production patterns.

---

## Step 5 — Local testing (`update.ps1`)

Fast loop **without** uploading to Workshop for other players:

```powershell
.\update.ps1
```

This script:

1. Runs `dotnet build` (Debug)
2. Copies your **Debug DLL** + both asset bundles into `WorkshopModFolder` from `config.json`

Requirements before it works:

- `config.json` exists and paths are correct
- You already **published once** (even a stub) and **subscribed** to your mod so `WorkshopModFolder` exists
- Asset bundles were built in Unity
- `DbsContentApi` is installed and loads first

Launch the game, enable your mod, and test. Repeat: edit code → `update.ps1` → test in game.

---

## Step 6 — First Workshop publish

Simplest path for beginners:

1. Edit `steam-build/mod.vdf`:
   - Set `contentfolder` to your `steam-build\build` folder (absolute path)
   - Set `previewfile` to `steam-build\icon.png`
   - Set `title` / `description`
   - Leave `publishedfileid` as `"0"` for the first upload
   - Set `visibility` — `0` = public, `1` = friends only, `2` = **private** (good for first tests)

2. Put **one empty placeholder file** inside `steam-build\build\` (e.g. `placeholder.txt`). You are only creating the Workshop item ID, not shipping real content yet.

3. Run:

   ```powershell
   .\publish_or_update_the_mod.ps1
   ```

   SteamCMD logs in (Steam Guard may prompt), uploads the item, and **writes the new `publishedfileid` back into `mod.vdf` automatically**.

4. Subscribe to **your own mod** in Steam Workshop.

5. Find the install folder, e.g.:

   `Steam/steamapps/workshop/content/2881650/<YourPublishedFileId>/`

   Put that path in `config.json` → `WorkshopModFolder`.

6. Build real content: Unity bundles + `dotnet build`, then use `update.ps1` for local testing.

When ready for others to download your real build:

```powershell
.\prepare_final_build.ps1   # Release build → copies DLL + bundles to steam-build\build
.\publish_or_update_the_mod.ps1
```

`prepare_final_build.ps1` runs `dotnet build -c Release` and stages the Release DLL and both bundles into `steam-build\build` for upload.

Set `DEBUG_MODE = false` before a public release.

---

## Workshop visibility reference

| Value in `mod.vdf` | Meaning |
|--------------------|---------|
| `0` | Public |
| `1` | Friends only |
| `2` | Private |

You can keep the mod private while iterating; switch to public when ready.

---

## Project layout (quick reference)

```
CW_SDK/
├── ExampleMod.cs              # Plugin entry
├── MyPluginInfo.cs            # GUID, name, version, vanilla flag
├── Modules/CustomContent.cs   # Register maps / mobs / items
├── Modules/                   # Example item behaviours
├── Patches/                   # Example Harmony patch
├── ExampleMod.csproj          # Game + DbsContentApi references
├── EXAMPLE.config.json        # Copy → config.json
├── update.ps1                 # Local test: Debug build + copy to Workshop folder
├── prepare_final_build.ps1    # Stage Release build for upload
├── publish_or_update_the_mod.ps1
├── steam-build/
│   ├── mod.vdf
│   ├── icon.png
│   └── build/                 # Staged files for SteamCMD
└── UNITY_CWSDK/               # Copy into Unity Assets/
```

---

## Troubleshooting

**`Config file not found`**

Copy `EXAMPLE.config.json` to `config.json` and fill in paths.

**PowerShell script won't run**

Execution policy may block scripts. In an elevated or user PowerShell session:

```powershell
Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
```

**`dotnet` not found**

Install the [.NET SDK](https://dotnet.microsoft.com/download) and reopen the terminal.

**`steamcmd.exe not found`**

Install [SteamCMD](https://developer.valvesoftware.com/wiki/SteamCMD) and set `SteamCmdPath` in `config.json`.

**`Workshop mod folder not found`**

Publish once (even with a placeholder file), subscribe to your mod, then paste the real Workshop path into `config.json`.

**`Asset bundle not found`**

Build bundles in Unity (**Assets → Build AssetBundles**) and point `AssetBundlePath1` / `AssetBundlePath2` at the files on disk (no file extension).

**`DbsContentApi` / build errors**

Subscribe to the [Workshop item](https://steamcommunity.com/sharedfiles/filedetails/?id=3680666123), download content, and fix `DbsContentApiDir` in `ExampleMod.csproj`.

**Mod does nothing in game**

Confirm DbsContentApi is **enabled** and loads **before** your mod. Confirm you subscribed to your mod and ran `update.ps1` or published a build containing the DLL and both bundles.

**Friends can't join**

If you register maps, mobs, or items, all players need your mod **and** DbsContentApi.

---

## Links

- [Content Warning on Steam](https://store.steampowered.com/app/2881650/Content_Warning/)
- [BD's content API (DbsContentApi)](https://steamcommunity.com/sharedfiles/filedetails/?id=3680666123)
- [SteamCMD documentation](https://developer.valvesoftware.com/wiki/SteamCMD)

Good luck — watch the [video tutorial](https://youtu.be/Jd8gq_ZmvmA) first, then use this README as a checklist while you work.
