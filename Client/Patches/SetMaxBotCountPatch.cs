using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Comfort.Common;

namespace acidphantasm_botplacementsystem.Patches
{
    internal class SetMaxBotCountPatch : ModulePatch
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
                "bigmap" => Plugin.CustomsMapLimit,
                "factory4_day" or "factory4_night" => Plugin.FactoryMapLimit,
                "interchange" => Plugin.InterchangeMapLimit,
                "laboratory" => Plugin.LabsMapLimit,
                "lighthouse" => Plugin.LighthouseMapLimit,
                "rezervbase" => Plugin.ReserveMapLimit,
                "sandbox" or "sandbox_high" => Plugin.GroundZeroMapLimit,
                "shoreline" => Plugin.ShorelineMapLimit,
                "tarkovstreets" => Plugin.StreetsMapLimit,
                "woods" => Plugin.WoodsMapLimit,
                "labyrinth" => Plugin.LabyrinthMapLimit,
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
