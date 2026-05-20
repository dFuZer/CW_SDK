using System;
using DbsContentApi;
using HarmonyLib;

namespace ExampleMod;

[ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
public class ExampleMod
{
    private bool _isPatched;
    public static bool DEBUG_MODE = true;

    static ExampleMod()
    {
        Instance = new ExampleMod();
    }

    /// <summary>
    ///     Constructor for the ExampleMod plugin.
    /// </summary>
    public ExampleMod()
    {

        DbsContentApiPlugin.SetModdedMobsOnly(true);
        DbsContentApiPlugin.SetAllItemsFree(true);
        DbsContentApiPlugin.SetModdedMapsOnly(true);

        CustomContent.Init();

        Logger.Log($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private Harmony? Harmony { get; set; }

    /// <summary>
    ///     Singleton instance of the ExampleMod plugin.
    /// </summary>
    public static ExampleMod Instance { get; }

    private void PatchAll()
    {
        if (_isPatched)
        {
            Logger.LogError("Already patched!");
            return;
        }

        Logger.Log("Patching...");
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

    /// <summary>
    ///     Unpatches all patches applied by the plugin.
    /// </summary>
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
