using System;
using DbsContentApi;
using HarmonyLib;

namespace ExampleMod;

/// <summary>
/// Entry point for this Content Warning mod.
/// The game discovers this type via <see cref="ContentWarningPluginAttribute"/>, loads the assembly,
/// runs <c>Harmony.PatchAll(asm)</c> on every class marked with Harmony attributes, then executes
/// static constructors — which is how <see cref="Instance"/> and custom content get initialized.
/// </summary>
[ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
public class ExampleMod
{
    private bool _isPatched;

    /// <summary>
    /// Toggle this while developing. You can set it to false on your release build.
    /// That way, you can use that boolean value to modify the behaviour of your mod during development.
    /// </summary>
    public static bool DEBUG_MODE = true;

    /// <summary>
    /// Runs before the instance constructor. The game triggers this when loading plugin types,
    /// so we create the singleton here to guarantee a single initialization path.
    /// </summary>
    static ExampleMod()
    {
        Instance = new ExampleMod();
    }

    /// <summary>
    /// Configures DbsContentApi flags and registers maps, mobs, and shop items.
    /// Keep this lightweight — heavy work belongs in deferred registrations inside
    /// <see cref="CustomContent"/>.
    /// </summary>
    public ExampleMod()
    {
        // Restrict spawning to monsters registered through DbsContentApi rather than vanilla roster.
        DbsContentApiPlugin.SetModdedMobsOnly(true);

        // Shop UI will set a price 0 for all items (useful when testing custom content).
        DbsContentApiPlugin.SetAllItemsFree(true);

        // Only maps registered via DbsContentApi are selectable (vanilla maps are disabled).
        DbsContentApiPlugin.SetModdedMapsOnly(true);

        CustomContent.Init();

        Logger.Log($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
    private Harmony? Harmony { get; set; }
    public static ExampleMod Instance { get; }

    private void PatchAll()
    {
        if (_isPatched)
        {
            Logger.LogError("Already patched!");
            return;
        }

        Logger.Log("Patching...");

        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        try
        {
            Harmony.PatchAll();
            _isPatched = true;
            Logger.Log("Patched!");
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to patch: {e}");
        }
    }

    public void UnpatchAll()
    {
        if (!_isPatched)
        {
            Logger.LogError("Already unpatched!");
            return;
        }

        Logger.Log("Unpatching...");

        try
        {
            Harmony?.UnpatchSelf();
            _isPatched = false;
            Logger.Log("Unpatched!");
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to unpatch: {e}");
        }
    }
}
