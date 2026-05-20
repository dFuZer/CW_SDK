using HarmonyLib;

namespace ExampleMod.Patches;

/// <summary>
/// Example Harmony patch that tweaks human player movement at runtime.
/// Applied automatically when the game loads this assembly via <c>Harmony.PatchAll</c> —
/// no manual patch call from <see cref="ExampleMod"/> is required.
/// </summary>
[HarmonyPatch(typeof(PlayerController), "Start")]
internal static class PlayerControllerStartPatch
{
    /// <summary>
    /// Runs after <see cref="PlayerController.Start"/> completes.
    /// Raises jump impulse for human-controlled players only (skips AI bots and monsters).
    /// </summary>
    /// <param name="__instance">The patched PlayerController; Harmony injects this parameter name.</param>
    [HarmonyPostfix]
    private static void Postfix(PlayerController __instance)
    {
        var player = __instance.GetComponent<Player>();

        // Player.ai is true for NPCs/monsters using the player rig — we only want real humans.
        if (player != null && !player.ai)
        {
            // Default jumpImpulse is lower; 14f is a noticeable but still reasonable boost.
            __instance.jumpImpulse = 14f;
        }
    }
}
