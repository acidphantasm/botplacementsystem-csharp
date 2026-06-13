using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BotPlacementSystemClient.Spawning;
using BotPlacementSystemClient.Utils;
using EFT;
using EFT.Game.Spawning;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace BotPlacementSystemClient.Patches;

internal class PmcSpawnHookPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BossSpawnerClass), nameof(BossSpawnerClass.method_2));
    }

    [PatchPrefix]
    private static bool PatchPrefix(BossSpawnerClass __instance, BossLocationSpawn wave, BotSpawnParams spawnParams, BotDifficulty difficulty, int followersCount, BotCreationDataClass creationData, ref bool __result)
    {
        try
        {
            if (wave.BossType != WildSpawnType.pmcBEAR && wave.BossType != WildSpawnType.pmcUSEC)
            {
                return true;
            }
                
            var location = Utility.CurrentLocation;
            var leaderName = creationData.Profiles[0].Nickname;
                
            if (Plugin.DebugLogging)
                Plugin.LogSource.LogInfo($"{leaderName} | WildSpawnType: {wave.BossType} | Count: {1 + wave.EscortCount} | {location}");

            var soloPointCount = 1;
            var escortPointCount = 1 + wave.EscortCount;
            var distance = GetDistanceForMap(location);
            var isSmallMap = location.Contains("factory4") || location.Contains("laboratory") || location.Contains("labyrinth");
            var scavDistance = isSmallMap ? 20f : 50f;

            List<ISpawnPoint> validSpawnLocations;
            lock (Utility.SpawnPointLock)
            {
                var pmcList = Utility.CachedPmcs.ToList();
                var scavList = Utility.CachedAssaultBots.Concat(Utility.CachedBosses).ToList();

                validSpawnLocations = GetValidSpawnPoints(pmcList, scavList, distance, scavDistance, escortPointCount, leaderName);

                if (validSpawnLocations.Count < escortPointCount && validSpawnLocations.Count > 0)
                {
                    var neededSpawnPointCount = escortPointCount - validSpawnLocations.Count;
                    var spawnPoint = validSpawnLocations[0];
                    for (var i = 0; i < neededSpawnPointCount; i++)
                    {
                        validSpawnLocations.Add(spawnPoint);
                    }
                }

                if (validSpawnLocations.Count >= escortPointCount)
                {
                    foreach (var point in validSpawnLocations)
                    {
                        Utility.ReservedSpawnPositions.Add(point.Position);
                    }
                }
            }

            if (validSpawnLocations.Count >= soloPointCount)
            {
                    
                if (Plugin.DebugLogging)
                    Plugin.LogSource.LogInfo($"{leaderName} | ValidLocations: {validSpawnLocations.Count} needed: {escortPointCount}");

                if (validSpawnLocations.Count >= escortPointCount)
                {
                    var botZone =
                        __instance.BotSpawner_0.GetClosestZone(validSpawnLocations[0].Position, out float _);
                    __instance.Float_1 = Time.time;
                    __instance.WildSpawnType_0 = wave.BossType;
                    __instance.BotZone_1 = botZone;

                    if (creationData.SpawnStopped)
                    {
                        if (Plugin.DebugLogging)
                            Plugin.LogSource.LogInfo($"{leaderName} | SpawnStopped before StartSpawnPMCGroup");
                        __result = false;
                        return false;
                    }

                    PmcGroupSpawner.StartSpawnPmcGroup(creationData, wave, spawnParams, followersCount, botZone,
                        validSpawnLocations).HandleExceptions();

                    __result = true;
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            if (Plugin.DebugLogging)
                Plugin.LogSource.LogError($"{creationData.Profiles[0].Nickname} | PatchPrefix EXCEPTION {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            __result = true;
            return false;
        }

        if (Plugin.DebugLogging)
            Plugin.LogSource.LogInfo($"{creationData.Profiles[0].Nickname} | No valid spawnpoints found - skipping spawn | WildSpawnType: {wave.BossType} | Count: {1 + wave.EscortCount}");
        __result = true;
        return false;
    }

    private static List<ISpawnPoint> GetValidSpawnPoints(IReadOnlyCollection<Player> pmcPlayers, IReadOnlyCollection<Player> scavPlayers, float distance, float scavDistance, int neededPoints, string leaderName)
    {
        // maybe need to check Utility.CurrentLocation == "tarkovstreets" on this
        if (!Plugin.PmcSpawnAnywhere)
        {
            var validPlayerSpawnPoints = GetPlayerSpawnPoints(pmcPlayers, scavPlayers, distance, scavDistance, neededPoints, leaderName);
            if (validPlayerSpawnPoints.Count >= neededPoints)
            {
                return validPlayerSpawnPoints;
            }

            if (Plugin.DebugLogging)
                Plugin.LogSource.LogInfo($"{leaderName} | Falling back to any spawn points, couldn't get enough points");
                
            var fallbackSpawnPoints = GetAnySpawnPoints(pmcPlayers, scavPlayers, distance * 0.75f, scavDistance * 0.75f, neededPoints, true, leaderName);
            return fallbackSpawnPoints;
        }

        var anywhereSpawnPoints = GetAnySpawnPoints(pmcPlayers, scavPlayers, distance, scavDistance, neededPoints, false, leaderName);
        return anywhereSpawnPoints;
    }

    private static List<ISpawnPoint> GetPlayerSpawnPoints(IReadOnlyCollection<Player> pmcPlayers, IReadOnlyCollection<Player> scavPlayers, float distance, float scavDistance, int neededPoints, string leaderName)
    {
        var validSpawnPoints = new List<ISpawnPoint>();

        var list = Utility.PlayerSpawnPoints;
        list = list.OrderBy(_ => GClass856.Random(0f, 1f)).ToList();

        var foundInitialPoint = false;

        foreach (var checkPoint in list)
        {
            if (validSpawnPoints.Count == neededPoints)
            {
                return validSpawnPoints;
            }

            switch (foundInitialPoint)
            {
                case true when Vector3.Distance(checkPoint.Position, validSpawnPoints[0].Position) <= 20f:
                    validSpawnPoints.Add(checkPoint);
                    break;
                case false when IsValid(checkPoint, pmcPlayers, distance):
                {
                    if (IsValid(checkPoint, scavPlayers, scavDistance))
                    {
                        validSpawnPoints.Add(checkPoint);
                        foundInitialPoint = true;
                            
                        if (Plugin.DebugLogging)
                        {
                            var closestPmcDistance = pmcPlayers
                                .Where(p => p != null && !Utility.IsPlayerHeadless(p))
                                .Select(p => Vector3.Distance(checkPoint.Position, p.Position))
                                .DefaultIfEmpty(float.MaxValue)
                                .Min();
                            Plugin.LogSource.LogInfo($"{leaderName} | PMC Initial SpawnPoint selected | Distance to nearest PMC: {closestPmcDistance:F1}m | Pmc Total: {pmcPlayers.Count}");
                        }
                    }

                    break;
                }
            }
        }

        return validSpawnPoints;
    }

    private static List<ISpawnPoint> GetAnySpawnPoints(IReadOnlyCollection<Player> pmcPlayers, IReadOnlyCollection<Player> scavPlayers, float distance, float scavDistance, int neededPoints, bool backupToPlayer, string leaderName)
    {
        var validSpawnPoints = new List<ISpawnPoint>();
        ISpawnPoint firstPoint = null;

        var alternativeList = backupToPlayer ? Utility.BackupPlayerSpawnPoints : Utility.CombinedSpawnPoints;
        alternativeList = alternativeList.OrderBy(_ => GClass856.Random(0f, 1f)).ToList();

        foreach (var checkPoint in alternativeList)
        {
            if (validSpawnPoints.Count == neededPoints)
                return validSpawnPoints;

            if (!IsValid(checkPoint, pmcPlayers, distance) || !IsValid(checkPoint, scavPlayers, scavDistance))
                continue;

            if (firstPoint == null)
            {
                firstPoint = checkPoint;
                validSpawnPoints.Add(checkPoint);
                    
                if (Plugin.DebugLogging)
                {
                    var closestPmcDistance = pmcPlayers
                        .Where(p => p != null && !Utility.IsPlayerHeadless(p))
                        .Select(p => Vector3.Distance(checkPoint.Position, p.Position))
                        .DefaultIfEmpty(float.MaxValue)
                        .Min();
                    Plugin.LogSource.LogInfo($"{leaderName} | PMC Initial SpawnPoint selected (AnySpawnPoints) | Fallback: {backupToPlayer} | Distance to nearest PMC: {closestPmcDistance:F1}m | Pmc Total: {pmcPlayers.Count}");
                }
                continue;
            }

            if (Vector3.Distance(checkPoint.Position, firstPoint.Position) <= 20f)
                validSpawnPoints.Add(checkPoint);
        }

        return validSpawnPoints;
    }

    private static bool IsValid(ISpawnPoint spawnPoint, IReadOnlyCollection<Player> players, float distance)
    {
        if (spawnPoint == null)
        {
            return false;
        }

        if (spawnPoint.Collider == null)
        {
            return false;
        }
            
        foreach (var reservedPos in Utility.ReservedSpawnPositions)
        {
            if (Vector3.Distance(spawnPoint.Position, reservedPos) < distance)
            {
                return false;
            }
        }
    
        foreach (var player in players)
        {
            if (player == null || Utility.IsPlayerHeadless(player))
            {
                continue;
            }
                
            Vector3 playerPosition;
            try
            {
                playerPosition = player.Position;
            }
            catch
            {
                Plugin.LogSource.LogInfo($"Player Position is Null when checking Pmc.IsValid()");
                continue;
            }
                
            if (spawnPoint.Collider.Contains(playerPosition))
            {
                return false;
            }
                
            if (Vector3.Distance(spawnPoint.Position, playerPosition) < distance)
            {
                return false;
            }
        }

        return true;
    }

    private static float GetDistanceForMap(string mapName)
    {
        var distance = mapName switch
        {
            "bigmap" => Plugin.CustomsPmcSpawnDistanceCheck,
            "factory4_day" or "factory4_night" => Plugin.FactoryPmcSpawnDistanceCheck,
            "interchange" => Plugin.InterchangePmcSpawnDistanceCheck,
            "laboratory" => Plugin.LabsPmcSpawnDistanceCheck,
            "lighthouse" => Plugin.LighthousePmcSpawnDistanceCheck,
            "rezervbase" => Plugin.ReservePmcSpawnDistanceCheck,
            "sandbox" or "sandbox_high" => Plugin.GroundZeroPmcSpawnDistanceCheck,
            "shoreline" => Plugin.ShorelinePmcSpawnDistanceCheck,
            "tarkovstreets" => Plugin.StreetsPmcSpawnDistanceCheck,
            "woods" => Plugin.WoodsPmcSpawnDistanceCheck,
            "labyrinth" => Plugin.LabyrinthPmcSpawnDistanceCheck,
            _ => -1f,
        };

        if (distance < 0f)
        {
            if (Plugin.DebugLogging)
                Plugin.LogSource.LogInfo($"PmcSpawnHookPatch | GetDistanceForMap | Unknown map '{mapName}' - falling back to 50f");
            return 50f;
        }

        return distance;
    }
}