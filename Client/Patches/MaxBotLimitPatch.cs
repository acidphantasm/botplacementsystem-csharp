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

            var location = gameWorld.MainPlayer.Location;
            if (location == null) return;

            switch (location.ToLower())
            {
                case "bigmap":
                    maxCount = Plugin.customsMapLimit;
                    break;
                case "factory4_day":
                case "factory4_night":
                    maxCount = Plugin.factoryMapLimit;
                    break;
                case "interchange":
                    maxCount = Plugin.interchangeMapLimit;
                    break;
                case "laboratory":
                    maxCount = Plugin.labsMapLimit;
                    break;
                case "lighthouse":
                    maxCount = Plugin.lighthouseMapLimit;
                    break;
                case "rezervbase":
                    maxCount = Plugin.reserveMapLimit;
                    break;
                case "sandbox":
                case "sandbox_high":
                    maxCount = Plugin.groundZeroMapLimit;
                    break;
                case "shoreline":
                    maxCount = Plugin.shorelineMapLimit;
                    break;
                case "tarkovstreets":
                    maxCount = Plugin.streetsMapLimit;
                    break;
                case "woods":
                    maxCount = Plugin.woodsMapLimit;
                    break;
                case "labyrinth":
                    maxCount = Plugin.labyrinthMapLimit;
                    break;
                default:
                    maxCount = 0;
                    break;
            }

            Logger.LogInfo($"[ABPS] Setting max bots to {maxCount} on {location.ToLower()}");
            __instance.MaxCount = maxCount;

            if (__instance.BotSpawner != null)
            {
                __instance.BotSpawner.SetMaxBots(__instance.MaxCount);
                __instance.ZonesLeaveController.SetMaxBots(__instance.MaxCount);
            }
        }
    }
}
