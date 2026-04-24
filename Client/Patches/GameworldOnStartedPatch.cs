using acidphantasm_botplacementsystem.Utils;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace acidphantasm_botplacementsystem.Patches
{
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
}
