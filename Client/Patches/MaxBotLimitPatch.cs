using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Comfort.Common;

namespace acidphantasm_botplacementsystem.Patches
{
    internal class MaxBotLimitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsController), nameof(BotsController.SetSettings));
        }

        [PatchPostfix]
        private static void PatchPostfix(BotsController __instance, int maxCount)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null) return;

            var location = gameWorld.LocationId;
            if (string.IsNullOrEmpty(location)) return;

            maxCount = location.ToLower() switch
            {
                "bigmap" => Plugin.customsMapLimit,
                "factory4_day" or "factory4_night" => Plugin.factoryMapLimit,
                "interchange" => Plugin.interchangeMapLimit,
                "laboratory" => Plugin.labsMapLimit,
                "lighthouse" => Plugin.lighthouseMapLimit,
                "rezervbase" => Plugin.reserveMapLimit,
                "sandbox" or "sandbox_high" => Plugin.groundZeroMapLimit,
                "shoreline" => Plugin.shorelineMapLimit,
                "tarkovstreets" => Plugin.streetsMapLimit,
                "woods" => Plugin.woodsMapLimit,
                "labyrinth" => Plugin.labyrinthMapLimit,
                _ => 0
            };

            Plugin.LogSource.LogInfo($"[ABPS] Setting max bots to {maxCount} on {location.ToLower()}");
            __instance.MaxCount = maxCount;

            if (__instance.BotSpawner == null)
            {
                return;
            }
            
            __instance.BotSpawner.SetMaxBots(__instance.MaxCount);
            __instance.ZonesLeaveController.SetMaxBots(__instance.MaxCount);
        }
    }
}
