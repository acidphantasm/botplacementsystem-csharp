using acidphantasm_botplacementsystem.Utils;
using EFT;
using EFT.Game.Spawning;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using Systems.Effects;
using UnityEngine;
using Object = System.Object;

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
        private static float _nextDespawnCheckTime = 0f;
        
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
            ref var isActive = ref ___bool_1;
            ref var isAtBotCap = ref ___bool_0;
            ref var isInSpawnWindow = ref ___bool_2;
            ref var nextWindowToggleTime = ref ___nullable_0;
            ref var spawnAttemptInterval = ref ___float_2;
            ref var lastSpawnAttemptTime = ref ___float_0;
            ref GClass1881<BotDifficulty> difficultyWeights = ref ___gclass1881_0;

            if (__instance == null || !isActive) return true;

            if (___abstractGame_0.PastTime < (float)___location_0.BotStart || ___abstractGame_0.PastTime > (float)___location_0.BotStop)
                return false;

            if (nextWindowToggleTime.Equals(null) || nextWindowToggleTime <= ___abstractGame_0.PastTime)
            {
                isInSpawnWindow = !isInSpawnWindow;
                nextWindowToggleTime = ___abstractGame_0.PastTime + (isInSpawnWindow
                    ? GClass856.Random((float)___location_0.BotSpawnTimeOnMin, (float)___location_0.BotSpawnTimeOnMax)
                    : GClass856.Random((float)___location_0.BotSpawnTimeOffMin, (float)___location_0.BotSpawnTimeOffMax));
            }

            if (!isInSpawnWindow)
            {
                return false;
            }

            if (___abstractGame_0.PastTime - lastSpawnAttemptTime < spawnAttemptInterval)
            {
                return false;
            }
            lastSpawnAttemptTime = ___abstractGame_0.PastTime;

            var freeSlots = (___botsController_0.MaxCount - Plugin.softCap) - ___botsController_0.AliveLoadingDelayedBotsCount;

            if (isAtBotCap)
            {
                if (freeSlots < ___location_0.BotSpawnCountStep)
                {
                    return false;
                }
                spawnAttemptInterval = 15f;
                isAtBotCap = false;
            }
            else if (freeSlots <= 0)
            {
                spawnAttemptInterval = Math.Max((float)___location_0.BotSpawnPeriodCheck, 15f);
                isAtBotCap = ___botsController_0.MaxCount - ___botsController_0.AliveAndLoadingBotsCount <= 0;
                return false;
            }
            
            if (Utility.BotsSpawnedPerPlayer > ___botsController_0.BotLocationModifier.NonWaveSpawnBotsLimitPerPlayerPvE)
            {
                return false;
            }
            
            var mapName = Utility.CurrentLocation.ToLower();
            if (Utility.CurrentMapZones.Count == 0)
            {
                Utility.CurrentMapZones = ___botsController_0.BotSpawner.AllBotZones.ToList();
            }

            var botZone = GetValidBotZone(WildSpawnType.assault, 1, ___botsController_0.BotSpawner.AllBotZones, mapName, ___botsController_0);
            ___botsController_0.ActivateBotsByWave(new BotWaveDataClass
            {
                BotsCount = 1,
                Time = Time.time,
                Difficulty = ___gclass1881_0.Random(),
                IsPlayers = GClass856.IsTrue100(Plugin.pScavChance),
                Side = EPlayerSide.Savage,
                WildSpawnType = WildSpawnType.assault,
                SpawnAreaName = botZone,
                WithCheckMinMax = false,
                ChanceGroup = 0,
            });
            Utility.BotsSpawnedPerPlayer += 1d / Math.Max(1, Utility.ConnectedPlayerCount);

            if (!(Time.time >= _nextDespawnCheckTime) || !Plugin.despawnFurthest)
            {
                return false;
            }
            
            _nextDespawnCheckTime = Time.time + Plugin.despawnTimer;
            var (count, center) = GetPlayerCountAndCenter();
            Utility.ConnectedPlayerCount = count;
            DespawnFurthestBots(___botsController_0, center);

            return false;
        }
        
        private static void DespawnFurthestBots(BotsController botsController, Vector3 centerOfPlayers)
        {
            var despawnDistance = Plugin.despawnDistance;

            if (Plugin.despawnPmcs)
            {
                foreach (var pmc in Utility.GetAllPMCs())
                {
                    if (pmc == null) continue;
                    if (Vector3.Distance(pmc.Position, centerOfPlayers) >= despawnDistance)
                        AttemptToDespawnBot(botsController, pmc.AIData.BotOwner);
                }
            }

            foreach (var scav in Utility.GetAllScavs())
            {
                if (scav == null) continue;
                if (Vector3.Distance(scav.Position, centerOfPlayers) >= despawnDistance)
                    AttemptToDespawnBot(botsController, scav.AIData.BotOwner);
            }
        }
        
        private static (int playerCount, Vector3 center) GetPlayerCountAndCenter()
        {
            var centerPoint = Vector3.zero;
            var count = 0;

            foreach (var player in Utility.GetAllPMCs())
            {
                if (player == null || player.IsAI)
                {
                    continue;
                }
                if (player.Profile.Info.MemberCategory == EMemberCategory.UnitTest)
                {
                    continue;
                }
                centerPoint += player.Position;
                count++;
            }
            foreach (var player in Utility.GetAllScavs())
            {
                if (player == null || player.IsAI)
                {
                    continue;
                }
                if (player.Profile.Info.MemberCategory == EMemberCategory.UnitTest)
                {
                    continue;
                }
                centerPoint += player.Position;
                count++;
            }

            return (count, count == 0 ? centerPoint : centerPoint / count);
        }

        private static void AttemptToDespawnBot(BotsController botsController, BotOwner botToDespawn)
        {
            var effectsCommutator = Singleton<Effects>.Instance.EffectsCommutator;
            var gameWorld = Singleton<GameWorld>.Instance;

            if (effectsCommutator is null || gameWorld is null) return;

            var botPlayer = botToDespawn.GetPlayer;
            
            effectsCommutator.StopBleedingForPlayer(botPlayer);
            gameWorld.UnregisterPlayer(botToDespawn);
            gameWorld.UnregisterPlayer(botPlayer);
            botToDespawn.Deactivate();
            botToDespawn.Dispose();
            botsController.BotDied(botToDespawn);
            botsController.DestroyInfo(botPlayer);
            UnityEngine.Object.DestroyImmediate(botToDespawn.gameObject);
            UnityEngine.Object.Destroy(botToDespawn);
        }
        
        private static string GetValidBotZone(WildSpawnType botType, int count, BotZone[] allZones, string location, BotsController _botsController)
        {
            if (Utility.CachedNonSnipeZones == null || Utility.CachedNonSnipeZones.Count == 0)
            {
                Utility.CachedNonSnipeZones = allZones.Where(x => !x.SnipeZone).ToList();
            }
            var botZones = Utility.CachedNonSnipeZones.OrderBy(_ => GClass856.Random(0, int.MaxValue)).ToList();

            if (Plugin.enableHotzones && GClass856.IsTrue100(Plugin.hotzoneScavChance) && Utility.MapHotSpots.ContainsKey(location))
            {
                var hotSpotZone = Utility.MapHotSpots[location].RandomElement();
                return hotSpotZone;
            }
            foreach (var currentZone in botZones)
            {
                if (_botsController.Bots.GetListByZone(currentZone).Count(x => x.IsRole(WildSpawnType.assault)) < Plugin.zoneScavCap)
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
            if (!data.IsValidSpawnType(WildSpawnType.assault) || pointsToSpawn != null) return;

            var botType = data.Profiles[0].Info.Settings.Role;
            var mapName = Utility.CurrentLocation.ToLower();
            
            var pmcList = Utility.GetAllPMCs();
            var pmcDistance = GetDistanceForMap(mapName);
            var scavList = Utility.GetAllScavs();

            var mapHasHotzone = DoesMapHaveHotzones(mapName);
            var hotZoneSelected = mapHasHotzone && IsZoneHotzone(mapName, botZone.NameZone);

            var isSmallMap = mapName.Contains("factory") || mapName.Contains("sandbox") || mapName.Contains("labyrinth") || mapName.Contains("laboratory");
            var scavDistance = hotZoneSelected ? 10f : isSmallMap ? 20f : 40f;

            pointsToSpawn = GetValidSpawnPoints(botZone, mapName, pmcList, pmcDistance, scavList, scavDistance, botType);

            if (!isSmallMap)
            {
                var allMapZones = __instance.AllBotZones.ToList();
                var scavsInZone = __instance.BotGame.BotsController.Bots.GetListByZone(botZone).Count(x => x.IsRole(WildSpawnType.assault));

                if (scavsInZone >= Plugin.zoneScavCap && (mapHasHotzone && !hotZoneSelected || !mapHasHotzone) || scavsInZone >= Plugin.hotzoneScavCap && mapHasHotzone && hotZoneSelected)
                {
                    var newBotZone = GetNewValidBotZone(allMapZones);
                    pointsToSpawn = GetNewSpawnPoints(mapName, botZone, newBotZone, mapHasHotzone, pmcList, pmcDistance, scavList, scavDistance, botType);
                    botZone = newBotZone;
                }

                if (pointsToSpawn.Count != 0)
                {
                    return;
                }
                
                var validZones = allMapZones.Where(x => !x.SnipeZone).ToList();
                for (var i = 0; i < 5; i++)
                {
                    var newBotZone = validZones.OrderBy(_ => GClass856.Random(0, int.MaxValue)).FirstOrDefault();
                    pointsToSpawn = GetNewSpawnPoints(mapName, botZone, newBotZone, mapHasHotzone, pmcList, pmcDistance, scavList, scavDistance, botType);
                    botZone = newBotZone;
                    if (pointsToSpawn.Count > 0) break;
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
            var allSpawnPoints = botZone.SpawnPoints
                .Where(x => x.Categories == ESpawnCategoryMask.All || x.Categories.ContainBotCategory())
                .OrderBy(_ => GClass856.Random(0, int.MaxValue))
                .ToList();

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
                if (player == null || player.Profile.GetCorrectedNickname().StartsWith("headless_"))
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
        private static BotZone GetNewValidBotZone(List<BotZone> botZones)
        {
            return botZones.Where(x => !x.SnipeZone).OrderBy(_ => GClass856.Random(0, int.MaxValue)).FirstOrDefault();
        }
        private static List<ISpawnPoint> GetNewSpawnPoints(string mapName, BotZone oldBotZone, BotZone newBotZone, bool mapHasHotzone, IReadOnlyCollection<IPlayer> pmcList, float pmcDistance, IReadOnlyCollection<IPlayer> scavList, float scavDistance, WildSpawnType botType)
        {
            Logger.LogInfo($"Old BotZone: {oldBotZone.NameZone} -> New BotZone: {newBotZone.NameZone}");
            var hotZoneSelected = mapHasHotzone && IsZoneHotzone(mapName, newBotZone.NameZone);
            if (mapHasHotzone && hotZoneSelected) scavDistance = 10f;

            var newPoints = GetValidSpawnPoints(newBotZone, mapName, pmcList, pmcDistance, scavList, scavDistance, botType);
            return newPoints;
        }
    }
}
