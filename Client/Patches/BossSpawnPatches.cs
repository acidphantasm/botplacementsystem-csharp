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

            Utility.currentMapZones.Clear();
            Utility.mapName = string.Empty;
            Utility.allPMCs.Clear();
            Utility.allBots.Clear();
            Utility.allSpawnPoints.Clear();
            Utility.playerSpawnPoints.Clear();
            Utility.backupPlayerSpawnPoints.Clear();
            Utility.combinedSpawnPoints.Clear();
            Utility.botsSpawnedPerPlayer = 0.0;
            Utility.connectedPlayerCount = 0;
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
            if (Plugin.botSpawnerInstance == null)
            {
                Plugin.botSpawnerInstance = __instance.BotSpawner_0;
            }
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
            location = location.ToLower();
            
            float distance = GetDistanceForMap(location);
            var scavDistance =
                location.Contains("factory4") || location.Contains("laboratory") || location.Contains("labyrinth")
                    ? 20f
                    : 50f;
            List<ISpawnPoint> validSpawnLocations = GetValidSpawnPoints(pmcList, scavList, distance, scavDistance, escortPointCount);

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

        private static List<ISpawnPoint> GetValidSpawnPoints(IReadOnlyCollection<IPlayer> pmcPlayers, IReadOnlyCollection<IPlayer> scavPlayers, float distance, float scavDistance, int neededPoints)
        {
            if (!Plugin.pmcSpawnAnywhere)
            {
                var validPlayerSpawnPoints = GetPlayerSpawnPoints(pmcPlayers, scavPlayers, distance, scavDistance, neededPoints);
                if (validPlayerSpawnPoints.Count > 0) return validPlayerSpawnPoints;
                
                // Use fallback anywhere, except cut the distances down to try to get valid. If this fails it'll return an empty list, which stops the spawn
                var fallbackSpawnPoints = GetAnySpawnPoints(pmcPlayers, scavPlayers, distance * 0.75f, scavDistance * 0.75f, true);
                return fallbackSpawnPoints;
            }
            
            var anywhereSpawnPoints = GetAnySpawnPoints(pmcPlayers, scavPlayers, distance, scavDistance);
            return anywhereSpawnPoints;
        }

        private static List<ISpawnPoint> GetPlayerSpawnPoints(IReadOnlyCollection<IPlayer> pmcPlayers, IReadOnlyCollection<IPlayer> scavPlayers, float distance, float scavDistance, int neededPoints)
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
                    if (IsValid(checkPoint, scavPlayers, scavDistance))
                    {
                        validSpawnPoints.Add(checkPoint);
                        foundInitialPoint = true;
                    }
                }
            }

            return validSpawnPoints;;
        }

        private static List<ISpawnPoint> GetAnySpawnPoints(IReadOnlyCollection<IPlayer> pmcPlayers, IReadOnlyCollection<IPlayer> scavPlayers, float distance, float scavDistance, bool backupToPlayer = false)
        {
            List<ISpawnPoint> validSpawnPoints = new List<ISpawnPoint>();
            
            List<ISpawnPoint> alternativeList = backupToPlayer ? Utility.GetBotNoBossNoSnipeSpawnPoints() : Utility.GetCombinedPlayerAndBotSpawnPoints();
            alternativeList = alternativeList.OrderBy(_ => Guid.NewGuid()).ToList();
            
            for (int i = 0; i < alternativeList.Count; i++)
            {
                ISpawnPoint checkPoint = alternativeList[i];
                if (IsValid(checkPoint, pmcPlayers, distance, true))
                {
                    if (IsValid(checkPoint, scavPlayers, scavDistance))
                    {
                        validSpawnPoints.Add(checkPoint);
                        return validSpawnPoints;
                    }
                }
            }
            
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
                if (BossSpawnTracking.TrackedBosses.Contains(bossName) && (Plugin.progressiveChances || Plugin.regressiveChances))
                {
                    Logger.LogInfo($"Saving boss as spawned: {bossName}");
                    BossSpawnTracking.UpdateBossSpawnChance(bossName);
                }
            }
        }
    }
}
