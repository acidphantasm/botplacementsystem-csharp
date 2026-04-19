using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using acidphantasm_botplacementsystem.Utils;
using EFT;
using EFT.Game.Spawning;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace acidphantasm_botplacementsystem.Patches
{
    internal class TryToSpawnInZonePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotSpawner), nameof(BotSpawner.TryToSpawnInZoneAndDelay));
        }

        [PatchPrefix]
        private static void PatchPrefix(BotSpawner __instance, BotZone botZone, BotCreationDataClass data, bool withCheckMinMax, bool newWave, ref List<ISpawnPoint> pointsToSpawn, bool forcedSpawn = false)
        {
            if (!data.IsValidSpawnType(WildSpawnType.assault) || pointsToSpawn != null) return;

            var botType = data.Profiles[0].Info.Settings.Role;
            var mapName = Utility.CurrentLocation.ToLower();
            
            var pmcList = Utility.CachedPmcs;
            var pmcDistance = GetDistanceForMap(mapName);
            var scavList = Utility.CachedAssaultBots;

            var mapHasHotzone = DoesMapHaveHotzones(mapName);
            var hotZoneSelected = mapHasHotzone && IsZoneHotzone(mapName, botZone.NameZone);

            var isSmallMap = mapName.Contains("factory") || mapName.Contains("sandbox") || mapName.Contains("labyrinth") || mapName.Contains("laboratory");
            var scavDistance = hotZoneSelected ? 10f : isSmallMap ? 20f : 40f;

            pointsToSpawn = GetValidSpawnPoints(botZone, mapName, pmcList, pmcDistance, scavList, scavDistance, botType);

            if (!isSmallMap)
            {
                var scavsInZone = __instance.BotGame.BotsController.Bots.GetListByZone(botZone).Count(x => x.IsRole(WildSpawnType.assault));

                if (scavsInZone >= Plugin.zoneScavCap && (mapHasHotzone && !hotZoneSelected || !mapHasHotzone) || scavsInZone >= Plugin.hotzoneScavCap && mapHasHotzone && hotZoneSelected)
                {
                    var newBotZone = Utility.GetNewValidBotZone();
                    pointsToSpawn = GetNewSpawnPoints(mapName, botZone, newBotZone, mapHasHotzone, pmcList, pmcDistance, scavList, scavDistance, botType);
                    botZone = newBotZone;
                }

                if (pointsToSpawn.Count != 0)
                {
                    return;
                }

                var validZones = Utility.CachedNonSnipeZones;
                
                for (var i = 0; i < Math.Min(5, validZones.Count); i++)
                {
                    var newBotZone = validZones[i];
                    pointsToSpawn = GetNewSpawnPoints(mapName, botZone, newBotZone, mapHasHotzone, pmcList, pmcDistance, scavList, scavDistance, botType);
                    botZone = newBotZone;
                    if (pointsToSpawn.Count > 0) 
                        break;
                }

                if (pointsToSpawn.Count != 0)
                {
                    return;
                }
                    
                Plugin.LogSource.LogInfo($"{data.Id} - {botZone.NameZone} - Returning null points, no valid points in distance");
                pointsToSpawn = null;
            }
        }

        private static List<ISpawnPoint> GetValidSpawnPoints(BotZone botZone, string location, IReadOnlyCollection<IPlayer> pmcList, float pmcDistance, IReadOnlyCollection<IPlayer> scavList, float scavDistance, WildSpawnType botType)
        {
            var validSpawnPoints = new List<ISpawnPoint>();
            var allSpawnPoints = Utility.GetZoneSpawnPoints(botZone);
            if (allSpawnPoints.Count == 0)
            {
                // fallback to vanilla
                allSpawnPoints = botZone.SpawnPoints
                    .Where(x => x.Categories == ESpawnCategoryMask.All || x.Categories.ContainBotCategory())
                    .OrderBy(_ => GClass856.Random(0f, 1f))
                    .ToList();
            }

            foreach (var checkPoint in allSpawnPoints)
            {
                if (!IsValid(checkPoint, pmcList, pmcDistance) || !IsValid(checkPoint, scavList, scavDistance))
                {
                    continue;
                }
                validSpawnPoints.Add(checkPoint); 
                return validSpawnPoints;
            }
            return validSpawnPoints;
        }
        private static bool IsValid(ISpawnPoint spawnPoint, IReadOnlyCollection<IPlayer> players, float distance)
        {
            if (spawnPoint?.Collider == null)
            {
                return false;
            }

            if (players == null || players.Count == 0)
            {
                return true;
            }
            
            foreach (var player in players)
            {
                if (player == null || Utility.IsPlayerHeadless(player) || !player.HealthController.IsAlive)
                {
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
            return true;
        }
        private static float GetDistanceForMap(string mapName)
        {
            return mapName switch
            {
                "bigmap"                        => Plugin.customs_ScavSpawnDistanceCheck,
                "factory4_day" or "factory4_night" => Plugin.factory_ScavSpawnDistanceCheck,
                "interchange"                   => Plugin.interchange_ScavSpawnDistanceCheck,
                "laboratory"                    => Plugin.labs_ScavSpawnDistanceCheck,
                "lighthouse"                    => Plugin.lighthouse_ScavSpawnDistanceCheck,
                "rezervbase"                    => Plugin.reserve_ScavSpawnDistanceCheck,
                "sandbox" or "sandbox_high"     => Plugin.groundZero_ScavSpawnDistanceCheck,
                "shoreline"                     => Plugin.shoreline_ScavSpawnDistanceCheck,
                "tarkovstreets"                 => Plugin.streets_ScavSpawnDistanceCheck,
                "woods"                         => Plugin.woods_ScavSpawnDistanceCheck,
                "labyrinth"                     => Plugin.labyrinth_ScavSpawnDistanceCheck,
                _                               => 10f,
            };
        }
        
        private static bool DoesMapHaveHotzones(string mapName)
        {
            return Plugin.enableHotzones && Utility.MapHotSpots.ContainsKey(mapName);
        }
        private static bool IsZoneHotzone(string mapName, string botZone)
        {
            return Utility.MapHotSpots[mapName].Contains(botZone);
        }
        private static List<ISpawnPoint> GetNewSpawnPoints(string mapName, BotZone oldBotZone, BotZone newBotZone, bool mapHasHotzone, IReadOnlyCollection<IPlayer> pmcList, float pmcDistance, IReadOnlyCollection<IPlayer> scavList, float scavDistance, WildSpawnType botType)
        {
            var hotZoneSelected = mapHasHotzone && IsZoneHotzone(mapName, newBotZone.NameZone);
            if (mapHasHotzone && hotZoneSelected) scavDistance = 10f;

            var newPoints = GetValidSpawnPoints(newBotZone, mapName, pmcList, pmcDistance, scavList, scavDistance, botType);
            return newPoints;
        }
    }
}

