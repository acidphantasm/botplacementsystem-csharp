using System.Reflection;
using acidphantasm_botplacementsystem.Spawning;
using acidphantasm_botplacementsystem.Utils;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace acidphantasm_botplacementsystem.Patches
{
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
}