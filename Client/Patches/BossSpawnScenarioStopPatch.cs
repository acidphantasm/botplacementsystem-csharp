using System.Reflection;
using BotPlacementSystemClient.Spawning;
using BotPlacementSystemClient.Utils;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace BotPlacementSystemClient.Patches;

internal class BossSpawnScenarioStopPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BossSpawnScenario), nameof(BossSpawnScenario.Stop));
    }

    [PatchPostfix]
    private static void PatchPostfix()
    {
        PmcGroupSpawner.Initialized = false;
        Utility.Initialized = false;
        BossSpawnTracking.EndRaidMergeData();
    }
}