using HarmonyLib;

namespace ExampleMod.Patches;

[HarmonyPatch(typeof(PlayerController), "Start")]
internal static class PlayerControllerStartPatch
{
    [HarmonyPostfix]
    private static void Postfix(PlayerController __instance)
    {
        var player = __instance.GetComponent<Player>();
        if (player != null && !player.ai)
        {
            __instance.jumpImpulse = 14f;
        }
    }
}
