using acidphantasm_botplacementsystem.Spawning;
using acidphantasm_botplacementsystem.Utils;
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace acidphantasm_botplacementsystem.Patches
{
    internal class PMCWaveCountPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BossSpawnScenario), nameof(BossSpawnScenario.method_0));
        }

        [PatchPostfix]
        private static void PatchPostfix(BossSpawnScenario __instance, BossLocationSpawn[] bossWaves)
        {
            if (__instance == null) return;

            Utility.currentMapZones = new List<BotZone>();
            Utility.mapName = string.Empty;
            Utility.allPMCs = new List<IPlayer>();
            Utility.allBots = new List<IPlayer>();
            Utility.allSpawnPoints = new List<ISpawnPoint>();
            Utility.playerSpawnPoints = new List<ISpawnPoint>();
        }
    }
    internal class PMCDistancePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BossSpawnerClass), nameof(BossSpawnerClass.method_2));
        }

        [PatchPrefix]
        private static bool PatchPrefix(BossSpawnerClass __instance, BossLocationSpawn wave, BotSpawnParams spawnParams, BotDifficulty difficulty, int followersCount, BotCreationDataClass creationData, ref bool __result)
        {
            if (wave.BossType != WildSpawnType.pmcBEAR && wave.BossType != WildSpawnType.pmcUSEC)
            {
                return true;
            }

            Logger.LogInfo($"Spawn Point Attempt: {creationData.Profiles[0].Nickname} | WildSpawnType: {wave.BossType} | Count: {1 + wave.EscortCount}");

            int soloPointCount = 1;
            int escortPointCount = 1 + wave.EscortCount;

            List<IPlayer> pmcList = Utility.GetAllPMCs();
            List<IPlayer> scavList = Utility.GetAllScavs();
            string location = Utility.CurrentLocation ?? "default";
            float distance = GetDistanceForMap(location);
            List<ISpawnPoint> validSpawnLocations = GetValidSpawnPoints(pmcList, scavList, distance, escortPointCount);

            if (validSpawnLocations.Count >= soloPointCount)
            {
                if (validSpawnLocations.Count < escortPointCount && validSpawnLocations.Count > 0)
                {
                    int neededSpawnPointCount = escortPointCount - validSpawnLocations.Count;
                    ISpawnPoint spawnPoint = validSpawnLocations[0];
                    for (int i = 0; i < neededSpawnPointCount; i++)
                    {
                        validSpawnLocations.Add(spawnPoint);
                    }
                }
                if (validSpawnLocations.Count >= escortPointCount)
                {
                    var botZone = __instance.BotSpawner_0.GetClosestZone(validSpawnLocations[0].Position, out float _);
                    __instance.Float_1 = Time.time;
                    __instance.WildSpawnType_0 = wave.BossType;
                    __instance.BotZone_1 = botZone;
                    if (creationData.SpawnStopped)
                    {
                        __result = false;
                        return false;
                    }
                    //__instance.method_3(creationData, wave, spawnParams, followersCount, botZone, validSpawnLocations);
                    PmcGroupSpawner.StartSpawnPMCGroup(creationData, wave, spawnParams, followersCount, botZone, validSpawnLocations, __instance, __instance.BotSpawner_0, __instance.IBotCreator);
                    __result = true;
                    return false;
                }
            }

            Logger.LogInfo($"No valid spawnpoints found - skipping spawn: {creationData.Profiles[0].Nickname} | WildSpawnType: {wave.BossType} | Count: {1 + wave.EscortCount}");
            __result = true;
            return false;
        }

        private static List<ISpawnPoint> GetValidSpawnPoints(IReadOnlyCollection<IPlayer> pmcPlayers, IReadOnlyCollection<IPlayer> scavPlayers, float distance, int neededPoints)
        {
            List<ISpawnPoint> validSpawnPoints = new List<ISpawnPoint>();
            List<ISpawnPoint> list = Utility.GetPlayerSpawnPoints();
            list = list.OrderBy(_ => Guid.NewGuid()).ToList();

            bool foundInitialPoint = false;

            for (int i = 0; i < list.Count; i++)
            {
                ISpawnPoint checkPoint = list[i];
                if (validSpawnPoints.Count == neededPoints)
                {
                    return validSpawnPoints;
                }
                if (foundInitialPoint && Vector3.Distance(checkPoint.Position, validSpawnPoints[0].Position) <= 10f)
                {
                    validSpawnPoints.Add(checkPoint);
                }
                if (!foundInitialPoint && IsValid(checkPoint, pmcPlayers, distance, true))
                {
                    if (IsValid(checkPoint, scavPlayers, 50f))
                    {
                        validSpawnPoints.Add(checkPoint);
                        foundInitialPoint = true;
                    }
                }
            }
            if (foundInitialPoint) return validSpawnPoints;
            
            // Get And Return Alternative Points if no Player points found
            // Uses lower distance checks for everything
            // Only returns a single point, it'll stack group members
            // Clear original list, it should already be clear but make sure
            validSpawnPoints.Clear();
            List<ISpawnPoint> alternativeList = Utility.GetBotNoBossNoSnipeSpawnPoints();
            alternativeList = alternativeList.OrderBy(_ => Guid.NewGuid()).ToList();
            
            for (int i = 0; i < alternativeList.Count; i++)
            {
                ISpawnPoint checkPoint = alternativeList[i];
                if (IsValid(checkPoint, pmcPlayers, distance / 2, true))
                {
                    if (IsValid(checkPoint, scavPlayers, 20f))
                    {
                        validSpawnPoints.Add(checkPoint);
                        return validSpawnPoints;
                    }
                }
            }
            
            // No spawn points found at all, return empty list
            return validSpawnPoints;
        }

        private static bool IsValid(ISpawnPoint spawnPoint, IReadOnlyCollection<IPlayer> players, float distance, bool checkAgainstMainPlayer = false)
        {
            if (spawnPoint == null) return false;
            if (spawnPoint.Collider == null) return false;
            if (Singleton<GameWorld>.Instance.MainPlayer != null)
            {
                var mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;
                if (checkAgainstMainPlayer && mainPlayer.Side == EPlayerSide.Savage)
                {
                    if (Vector3.Distance(spawnPoint.Position, mainPlayer.Position) < distance)
                    {
                        return false;
                    }
                }
            }
            if (players != null && players.Count != 0)
            {
                foreach (IPlayer player in players)
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
            }
            return true;
        }

        private static float GetDistanceForMap(string mapName)
        {
            mapName = mapName.ToLower();
            float distanceLimit = 50f;
            switch (mapName)
            {
                case "bigmap":
                    distanceLimit = Plugin.customs_PMCSpawnDistanceCheck;
                    return distanceLimit;
                case "factory4_day":
                case "factory4_night":
                    distanceLimit = Plugin.factory_PMCSpawnDistanceCheck;
                    return distanceLimit;
                case "interchange":
                    distanceLimit = Plugin.interchange_PMCSpawnDistanceCheck;
                    return distanceLimit;
                case "laboratory":
                    distanceLimit = Plugin.labs_PMCSpawnDistanceCheck;
                    return distanceLimit;
                case "lighthouse":
                    distanceLimit = Plugin.lighthouse_PMCSpawnDistanceCheck;
                    return distanceLimit;
                case "rezervbase":
                    distanceLimit = Plugin.reserve_PMCSpawnDistanceCheck;
                    return distanceLimit;
                case "sandbox":
                case "sandbox_high":
                    distanceLimit = Plugin.groundZero_PMCSpawnDistanceCheck;
                    return distanceLimit;
                case "shoreline":
                    distanceLimit = Plugin.shoreline_PMCSpawnDistanceCheck;
                    return distanceLimit;
                case "tarkovstreets":
                    distanceLimit = Plugin.streets_PMCSpawnDistanceCheck;
                    return distanceLimit;
                case "woods":
                    distanceLimit = Plugin.woods_PMCSpawnDistanceCheck;
                    return distanceLimit;
                case "labyrinth":
                    distanceLimit = Plugin.labyrinth_PMCSpawnDistanceCheck;
                    return distanceLimit;
                default:
                    return distanceLimit;
            }
        }
    }
    internal class BossAddProgressionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotSpawner), nameof(BotSpawner.method_10));
        }

        [PatchPostfix]
        private static void PatchPostfix(BotZone zone, BotCreationDataClass data, Action<BotOwner> callback, CancellationToken cancellationToken)
        {
            if (data.Profiles[0].Info.Settings.Role.IsBoss())
            {
                var bossName = data.Profiles[0].Info.Settings.Role;
                if (BossSpawnTracking.TrackedBosses.Contains(bossName) && Plugin.progressiveChances)
                {
                    Logger.LogInfo($"Saving boss as spawned: {bossName}");
                    BossSpawnTracking.UpdateBossSpawnChance(bossName);
                }
            }
        }
    }
}
