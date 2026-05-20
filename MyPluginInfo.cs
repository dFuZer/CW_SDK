namespace ExampleMod;

/// <summary>
/// Compile-time metadata consumed by the mod loader and Steam Workshop packaging.
/// Update these values before publishing; the GUID must remain stable across updates
/// so existing subscribers receive patches instead of a duplicate mod entry.
/// </summary>
public class MyPluginInfo
{
    /// <summary>
    /// Unique identifier for this mod. Used by the Content Warning plugin loader and Harmony ID.
    /// Must be globally unique across all mods (reverse-domain style is recommended).
    /// </summary>
    public const string PLUGIN_GUID = "example.mod.guid";

    /// <summary>Human-readable name shown in logs and workshop tooling.</summary>
    public const string PLUGIN_NAME = "EXAMPLE MOD";

    /// <summary>Semantic version string reported to the plugin loader.</summary>
    public const string PLUGIN_VERSION = "0.0.0";

    /// <summary>
    /// When true, clients without this mod can still join the lobby.
    /// Set to true only for host-only or purely client-side cosmetic changes.
    /// This example mod ships custom maps/mobs/items, so it requires all players to have it.
    /// </summary>
    public const bool VANILLA_COMPATIBLE = false;
}
