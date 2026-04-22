using acidphantasm_botplacementsystem.Spawning;
using acidphantasm_botplacementsystem.Utils;
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace acidphantasm_botplacementsystem.Patches
{
    internal class PmcSpawnHookPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BossSpawnerClass), nameof(BossSpawnerClass.method_2));
        }

        [PatchPrefix]
        private static bool PatchPrefix(BossSpawnerClass __instance, BossLocationSpawn wave, BotSpawnParams spawnParams,
            BotDifficulty difficulty, int followersCount, BotCreationDataClass creationData, ref bool __result)
        {
            Plugin.BotSpawnerInstance ??= __instance.BotSpawner_0;
            
            if (!PmcGroupSpawner.Initialized)
            {
                Plugin.LogSource.LogInfo($"Resetting PmcGroupSpawner");
                PmcGroupSpawner.InitializePmcSpawner(__instance, __instance.BotSpawner_0, __instance.IBotCreator);
            }
            
            if (wave.BossType != WildSpawnType.pmcBEAR && wave.BossType != WildSpawnType.pmcUSEC)
            {
                return true;
            }

            Logger.LogInfo($"Spawn Point Attempt: {creationData.Profiles[0].Nickname} | WildSpawnType: {wave.BossType} | Count: {1 + wave.EscortCount}");

            var soloPointCount = 1;
            var escortPointCount = 1 + wave.EscortCount;

            var pmcList = Utility.CachedPmcs;
            var scavList = Utility.CachedAssaultBots.Concat(Utility.CachedBosses).ToList();
            var location = Utility.CurrentLocation ?? "default";
            location = location.ToLower();

            var distance = GetDistanceForMap(location);
            var isSmallMap = location.Contains("factory4") || location.Contains("laboratory") ||
                             location.Contains("labyrinth");
            var scavDistance = isSmallMap ? 20f : 50f;

            var validSpawnLocations = GetValidSpawnPoints(pmcList, scavList, distance, scavDistance, escortPointCount);

            if (validSpawnLocations.Count >= soloPointCount)
            {
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
                    PmcGroupSpawner.StartSpawnPMCGroup(creationData, wave, spawnParams, followersCount, botZone, validSpawnLocations);
                    
                    __result = true;
                    return false;
                }
            }

            Logger.LogInfo($"No valid spawnpoints found - skipping spawn: {creationData.Profiles[0].Nickname} | WildSpawnType: {wave.BossType} | Count: {1 + wave.EscortCount}");
            
            __result = true;
            return false;
        }

        private static List<ISpawnPoint> GetValidSpawnPoints(IReadOnlyCollection<Player> pmcPlayers, IReadOnlyCollection<Player> scavPlayers, float distance, float scavDistance, int neededPoints)
        {
            if (!Plugin.PmcSpawnAnywhere)
            {
                var validPlayerSpawnPoints = GetPlayerSpawnPoints(pmcPlayers, scavPlayers, distance, scavDistance, neededPoints);
                if (validPlayerSpawnPoints.Count > 0)
                {
                    return validPlayerSpawnPoints;
                }

                // Use fallback anywhere, except cut the distances down to try to get valid. If this fails it'll return an empty list, which stops the spawn
                var fallbackSpawnPoints = GetAnySpawnPoints(pmcPlayers, scavPlayers, distance * 0.75f, scavDistance * 0.75f, true);
                return fallbackSpawnPoints;
            }

            var anywhereSpawnPoints = GetAnySpawnPoints(pmcPlayers, scavPlayers, distance, scavDistance);
            return anywhereSpawnPoints;
        }

        private static List<ISpawnPoint> GetPlayerSpawnPoints(IReadOnlyCollection<Player> pmcPlayers, IReadOnlyCollection<Player> scavPlayers, float distance, float scavDistance, int neededPoints)
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
                    case true when Vector3.Distance(checkPoint.Position, validSpawnPoints[0].Position) <= 10f:
                        validSpawnPoints.Add(checkPoint);
                        break;
                    case false when IsValid(checkPoint, pmcPlayers, distance):
                    {
                        if (IsValid(checkPoint, scavPlayers, scavDistance))
                        {
                            validSpawnPoints.Add(checkPoint);
                            foundInitialPoint = true;
                        }

                        break;
                    }
                }
            }

            return validSpawnPoints;
        }

        private static List<ISpawnPoint> GetAnySpawnPoints(IReadOnlyCollection<Player> pmcPlayers, IReadOnlyCollection<Player> scavPlayers, float distance, float scavDistance, bool backupToPlayer = false)
        {
            var validSpawnPoints = new List<ISpawnPoint>();

            var alternativeList = backupToPlayer ? Utility.BackupPlayerSpawnPoints : Utility.CombinedSpawnPoints;
            alternativeList = alternativeList.OrderBy(_ => GClass856.Random(0f, 1f)).ToList();

            foreach (var checkPoint in alternativeList)
            {
                if (!IsValid(checkPoint, pmcPlayers, distance))
                {
                    continue;
                }

                if (!IsValid(checkPoint, scavPlayers, scavDistance))
                {
                    continue;
                }
                
                validSpawnPoints.Add(checkPoint);
                return validSpawnPoints;
            }

            return validSpawnPoints;
        }

        private static bool IsValid(ISpawnPoint spawnPoint, IReadOnlyCollection<Player> players, float distance)
        {
            if (spawnPoint?.Collider == null)
            {
                return false;
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
            return mapName switch
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
                _ => 50f,
            };
        }
    }
}
