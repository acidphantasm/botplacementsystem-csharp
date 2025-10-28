using acidphantasm_botplacementsystem.Utils;
using EFT;
using EFT.Game.Spawning;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace acidphantasm_botplacementsystem.Patches
{
    internal class AssaultGroupPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProfileInfoSettingsClass), nameof(ProfileInfoSettingsClass.TryChangeRoleToAssaultGroup));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ProfileInfoSettingsClass __instance)
        {
            if(__instance.Role == WildSpawnType.assaultGroup)
            {
                __instance.Role = WildSpawnType.assault;
            }
            return false;
        }
    }
    internal class NonWavesSpawnScenarioUpdatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(NonWavesSpawnScenario), nameof(NonWavesSpawnScenario.Update));
        }

        [PatchPrefix]
        private static bool PatchPrefix(
            NonWavesSpawnScenario __instance,
            ref BotsController ___botsController_0,
            ref AbstractGame ___abstractGame_0,
            ref LocationSettingsClass.Location ___location_0,
            ref GClass1881<BotDifficulty> ___gclass1881_0,
            ref GClass1881<WildSpawnType> ___gclass1881_1,
            ref GClass1876 ___gclass1876_0,
            ref bool ___bool_0,
            ref bool ___bool_1,
            ref bool ___bool_2,
            ref float ___nullable_0,
            ref float ___float_2,
            ref float ___float_0)
        {
            if (__instance == null || !___bool_1) return true;

            if (___abstractGame_0.PastTime < (float)___location_0.BotStart || ___abstractGame_0.PastTime > (float)___location_0.BotStop)
            {
                return false;
            }
            if (___nullable_0.Equals(null) || ___nullable_0 <= ___abstractGame_0.PastTime)
            {
                ___bool_2 = !___bool_2;
                float newFloat = (___abstractGame_0.PastTime + (___bool_2 ? GClass856.Random((float)___location_0.BotSpawnTimeOnMin, (float)___location_0.BotSpawnTimeOnMax) : GClass856.Random((float)___location_0.BotSpawnTimeOffMin, (float)___location_0.BotSpawnTimeOffMax)));
                ___nullable_0 = newFloat;
            }
            if (!___bool_2)
            {
                return false;
            }
            if (___abstractGame_0.PastTime - ___float_0 < ___float_2)
            {
                return false;
            }
            ___float_0 = ___abstractGame_0.PastTime;
            int num = (___botsController_0.MaxCount - Plugin.softCap) - ___botsController_0.AliveLoadingDelayedBotsCount;
            if (___bool_0)
            {
                if (num < ___location_0.BotSpawnCountStep)
                {
                    return false;
                }
                ___float_2 = 10f;
                ___bool_0 = false;
            }
            else if (num <= 0)
            {
                if (!___bool_0)
                {
                    ___float_2 = (float)___location_0.BotSpawnPeriodCheck;
                    if (___float_2 < 10f)
                    {
                        ___float_2 = 10f;
                    }
                    ___bool_0 = ___botsController_0.MaxCount - ___botsController_0.AliveAndLoadingBotsCount <= 0;
                }
                return false;
            }

            num = ___gclass1876_0.TrySpawn(num, ___botsController_0, ___gclass1881_0);

            for (int i = 0; i < num; i++)
            {
                var wildSpawn = WildSpawnType.assault;
                string mapName = Utility.GetCurrentLocation().ToLower();
                if (Utility.currentMapZones.Count == 0)
                {
                    Utility.currentMapZones = ___botsController_0.BotSpawner.AllBotZones.ToList();
                }
                var botZone = GetValidBotZone(wildSpawn, 1, ___botsController_0.BotSpawner.AllBotZones, mapName, ___botsController_0);

                BotWaveDataClass botWaveDataClass = new BotWaveDataClass
                {
                    BotsCount = 1,
                    Time = Time.time,
                    Difficulty = ___gclass1881_0.Random(),
                    IsPlayers = GClass856.IsTrue100(Plugin.pScavChance),
                    Side = EPlayerSide.Savage,
                    WildSpawnType = wildSpawn,
                    SpawnAreaName = botZone,
                    WithCheckMinMax = false,
                    ChanceGroup = 0,
                };

                ___botsController_0.ActivateBotsByWave(botWaveDataClass);
            }
            return false;
        }

        private static string GetValidBotZone(WildSpawnType botType, int count, BotZone[] allZones, string location, BotsController _botsController)
        {
            List<BotZone> botZones = allZones.ToList().Where(x => !x.SnipeZone).ToList();
            botZones = botZones.OrderBy(_ => Guid.NewGuid()).ToList();

            if (Plugin.enableHotzones && GClass856.IsTrue100(Plugin.hotzoneScavChance) && Utility.mapHotSpots.ContainsKey(location))
            {
                var hotSpotZone = Utility.mapHotSpots[location].RandomElement();
                return hotSpotZone;
            }
            for (int i = 0; i < botZones.Count; i++)
            {
                BotZone currentZone = botZones[i];
                if (_botsController.Bots.GetListByZone(currentZone).Where(x => x.IsRole(WildSpawnType.assault)).ToList().Count < Plugin.zoneScavCap)
                {
                    return currentZone.NameZone;
                }
            }

            return "";
        }
    }
    internal class TryToSpawnInZonePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotSpawner), nameof(BotSpawner.TryToSpawnInZoneAndDelay));
        }

        [PatchPrefix]
        private static void PatchPrefix(BotSpawner __instance, BotZone botZone, BotCreationDataClass data, bool withCheckMinMax, bool newWave, ref List<ISpawnPoint> pointsToSpawn, bool forcedSpawn = false)
        {
            if (data.IsValidSpawnType(WildSpawnType.assault) && pointsToSpawn == null)
            {
                //Logger.LogInfo("TryToSpawnInZoneAndDelay Hit with empty spawn points and is a scav/marksman");

                WildSpawnType botType = data.Profiles[0].Info.Settings.Role;
                string mapName = Utility.GetCurrentLocation().ToLower();

                List<IPlayer> pmcList = Utility.GetAllPMCs();
                float pmcDistance = GetDistanceForMap(mapName);

                List<IPlayer> scavList = Utility.GetAllScavs();
                float scavDistance = mapName.Contains("factory") || mapName.Contains("sandbox") ? 20f : 40f;

                bool mapHasHotzone = DoesMapHaveHotzones(mapName);
                bool hotZoneSelected = mapHasHotzone ? IsZoneHotzone(mapName, botZone.NameZone) : false;
                List<BotZone> allMapZones = __instance.AllBotZones.ToList();

                if (mapHasHotzone && hotZoneSelected) scavDistance = 10f;
                pointsToSpawn = GetValidSpawnPoints(botZone, mapName, pmcList, pmcDistance, scavList, scavDistance, botType);

                if (!mapName.Contains("factory") && !mapName.Contains("sandbox") && !mapName.Contains("laboratory"))
                {
                    var scavsInZone = __instance.BotGame.BotsController.Bots.GetListByZone(botZone).Where(x => x.IsRole(WildSpawnType.assault)).ToList().Count;

                    if (scavsInZone >= Plugin.zoneScavCap && (mapHasHotzone && !hotZoneSelected || !mapHasHotzone))
                    {
                        var newBotZone = GetNewValidBotZone(allMapZones);
                        pointsToSpawn = GetNewSpawnPoints(mapName, botZone, newBotZone, mapHasHotzone, pmcList, pmcDistance, scavList, scavDistance, botType);
                        botZone = newBotZone;
                    }
                    else if (scavsInZone >= Plugin.hotzoneScavCap && mapHasHotzone && hotZoneSelected)
                    {
                        var newBotZone = GetNewValidBotZone(allMapZones);
                        pointsToSpawn = GetNewSpawnPoints(mapName, botZone, newBotZone, mapHasHotzone, pmcList, pmcDistance, scavList, scavDistance, botType);
                        botZone = newBotZone;
                    }

                    if (pointsToSpawn.Count == 0)
                    {
                        var maxTries = 5;
                        for (int i = 0; i < maxTries; i++)
                        {
                            var newBotZone = GetNewValidBotZone(allMapZones);
                            pointsToSpawn = GetNewSpawnPoints(mapName, botZone, newBotZone, mapHasHotzone, pmcList, pmcDistance, scavList, scavDistance, botType);
                            botZone = newBotZone;
                            if (pointsToSpawn.Count > 0) break;
                        }

                        if (pointsToSpawn.Count == 0)
                        {
                            Logger.LogInfo($"{data.Id} - {botZone.NameZone} - Returning null points, no valid points in distance");
                            pointsToSpawn = null;
                        }
                    }
                }
            }
        }

        private static List<ISpawnPoint> GetValidSpawnPoints(BotZone botZone, string location, IReadOnlyCollection<IPlayer> pmcList, float pmcDistance, IReadOnlyCollection<IPlayer> scavList, float scavDistance, WildSpawnType botType)
        {
            List<ISpawnPoint> validSpawnPoints = new List<ISpawnPoint>();
            List<ISpawnPoint> allSpawnPoints = botZone.SpawnPoints.ToList()
                .Where(x => x.Categories == ESpawnCategoryMask.All || x.Categories.ContainBotCategory())
                .ToList();

            allSpawnPoints = allSpawnPoints.OrderBy(_ => Guid.NewGuid()).ToList();

            int count = 0;
            for (int i = 0; i < allSpawnPoints.Count; i++)
            {
                ISpawnPoint checkPoint = allSpawnPoints[i];
                count++;
                //Logger.LogInfo($"Checking spawn point: {count}/{allSpawnPoints.Count}");
                if (IsValid(checkPoint, pmcList, pmcDistance) && IsValid(checkPoint, scavList, scavDistance))
                {
                    //Logger.LogInfo($"Adding initial point: {count}/{allSpawnPoints.Count}");
                    validSpawnPoints.Add(checkPoint); 
                    return validSpawnPoints;
                }
            }
            return validSpawnPoints;
        }
        private static bool IsValid(ISpawnPoint spawnPoint, IReadOnlyCollection<IPlayer> players, float distance)
        {
            if (spawnPoint == null) return false;
            if (spawnPoint.Collider == null) return false;
            if (players != null && players.Count != 0)
            {
                foreach (IPlayer player in players)
                {
                    if (player == null || player.Profile.GetCorrectedNickname().StartsWith("headless_"))
                    {
                        //Logger.LogInfo("Player is null or headless client, skip");
                        continue;
                    }
                    if (spawnPoint.Collider.Contains(player.Position))
                    {
                        return false;
                    }
                    if (Vector3.Distance(spawnPoint.Position, player.Position) < distance)
                    {
                        return false;
                    }
                }
                //Logger.LogInfo($"Point is valid after checking {players.Count}");
                return true;
            }
            return true;
        }
        private static float GetDistanceForMap(string mapName)
        {
            float distanceLimit = 10f;
            switch (mapName)
            {
                case "bigmap":
                    distanceLimit = Plugin.customs_ScavSpawnDistanceCheck;
                    return distanceLimit;
                case "factory4_day":
                case "factory4_night":
                    distanceLimit = Plugin.factory_ScavSpawnDistanceCheck;
                    return distanceLimit;
                case "interchange":
                    distanceLimit = Plugin.interchange_ScavSpawnDistanceCheck;
                    return distanceLimit;
                case "laboratory":
                    distanceLimit = Plugin.labs_ScavSpawnDistanceCheck;
                    return distanceLimit;
                case "lighthouse":
                    distanceLimit = Plugin.lighthouse_ScavSpawnDistanceCheck;
                    return distanceLimit;
                case "rezervbase":
                    distanceLimit = Plugin.reserve_ScavSpawnDistanceCheck;
                    return distanceLimit;
                case "sandbox":
                case "sandbox_high":
                    distanceLimit = Plugin.groundZero_ScavSpawnDistanceCheck;
                    return distanceLimit;
                case "shoreline":
                    distanceLimit = Plugin.shoreline_ScavSpawnDistanceCheck;
                    return distanceLimit;
                case "tarkovstreets":
                    distanceLimit = Plugin.streets_ScavSpawnDistanceCheck;
                    return distanceLimit;
                case "woods":
                    distanceLimit = Plugin.woods_ScavSpawnDistanceCheck;
                    return distanceLimit;
                case "labyrinth":
                    distanceLimit = Plugin.labyrinth_ScavSpawnDistanceCheck;
                    return distanceLimit;
                default:
                    return distanceLimit;
            }
        }
        private static Boolean DoesMapHaveHotzones(string mapName)
        {
            return Plugin.enableHotzones ? Utility.mapHotSpots.ContainsKey(mapName) : false;
        }
        private static Boolean IsZoneHotzone(string mapName, string botZone)
        {
            return Utility.mapHotSpots[mapName].Contains(botZone);
        }
        private static BotZone GetNewValidBotZone(List<BotZone> botZones)
        {
            return botZones.Where(x => !x.SnipeZone).ToList().OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
        }
        private static List<ISpawnPoint> GetNewSpawnPoints(string mapName, BotZone oldBotZone, BotZone newBotZone, bool mapHasHotzone, IReadOnlyCollection<IPlayer> pmcList, float pmcDistance, IReadOnlyCollection<IPlayer> scavList, float scavDistance, WildSpawnType botType)
        {
            Logger.LogInfo($"Old BotZone: {oldBotZone.NameZone} -> New BotZone: {newBotZone.NameZone}");
            bool hotZoneSelected = mapHasHotzone ? IsZoneHotzone(mapName, newBotZone.NameZone) : false;
            if (mapHasHotzone && hotZoneSelected) scavDistance = 10f;

            var newPoints = GetValidSpawnPoints(newBotZone, mapName, pmcList, pmcDistance, scavList, scavDistance, botType);
            return newPoints;
        }
    }
}
