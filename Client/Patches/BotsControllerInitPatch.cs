using System.Reflection;
using acidphantasm_botplacementsystem.Utils;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace acidphantasm_botplacementsystem.Patches
{
    internal class BotsControllerInitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsController), nameof(BotsController.Init));
        }

        [PatchPostfix]
        private static void PatchPostfix(BotsController __instance)
        {
            if (__instance == null)
                return;

            if (!Utility.Initialized)
            {
                Plugin.LogSource.LogInfo($"InitializeSpawnPoints from BotsController.Init {__instance.BotSpawner.AllBotZones.Length}");
                Utility.InitializeSpawnPoints(__instance.BotSpawner.AllBotZones);
            }
        }
    }
}