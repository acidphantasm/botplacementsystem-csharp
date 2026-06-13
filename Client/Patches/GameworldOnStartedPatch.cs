using System.Reflection;
using BotPlacementSystemClient.Utils;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace BotPlacementSystemClient.Patches;

internal class GameworldOnStartedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
    }

    [PatchPostfix]
    private static void PatchPostfix(GameWorld __instance)
    {
        if (__instance == null)
        {
            return;
        }

        __instance.GetOrAddComponent<BotZoneVisualizer>();
        __instance.GetOrAddComponent<SpawnPointGetter>();
    }
}