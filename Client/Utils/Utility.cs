using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace acidphantasm_botplacementsystem.Utils
{
    internal class Utility
    {
        public static string mainProfileID = string.Empty;
        public static string mapName = string.Empty;
        public static List<IPlayer> allPMCs = new();
        public static List<IPlayer> allBots = new();
        public static List<IPlayer> allScavs = new();
        public static List<ISpawnPoint> allSpawnPoints = new();
        public static List<ISpawnPoint> playerSpawnPoints = new();
        public static List<ISpawnPoint> backupPlayerSpawnPoints = new();
        public static List<ISpawnPoint> combinedSpawnPoints = new();
        public static List<BotZone> currentMapZones = new();
        public static double botsSpawnedPerPlayer;
        public static int connectedPlayerCount;

        public static Dictionary<string, string[]> mapHotSpots = new()
        {
            {"rezervbase", ["ZoneSubStorage", "ZoneBarrack"]},
            {"shoreline", ["ZoneSanatorium1", "ZoneSanatorium2"]},
            {"lighthouse", ["Zone_LongRoad", "Zone_Chalet", "Zone_Village"]},
            {"interchange", ["ZoneCenter", "ZoneCenterBot"]},
            {"bigmap", ["ZoneDormitory", "ZoneScavBase", "ZoneOldAZS", "ZoneGasStation"]}
        };

        public void Awake()
        {
            mainProfileID = GetPlayerProfile().ProfileId;
            Plugin.LogSource.LogInfo(mainProfileID);
        }

        public static Profile GetPlayerProfile()
        {
            return ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile;
        }

        public static string CurrentLocation
        {
            get
            {
                if (mapName != string.Empty) return mapName;

                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld != null)
                {
                    mapName = gameWorld.LocationId;
                    return mapName;
                }
                return "default";
            }
        }

        public static List<IPlayer> GetAllPMCs()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld != null)
            {
                allPMCs = gameWorld.RegisteredPlayers
                    .Where(x => x.Profile.Side == EPlayerSide.Bear || x.Profile.Side == EPlayerSide.Usec)
                    .ToList();
            }
            return allPMCs;
        }

        public static List<IPlayer> GetAllScavs()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld != null)
            {
                allScavs = gameWorld.RegisteredPlayers
                    .Where(x => x.Profile.Info.Settings.Role == WildSpawnType.assault)
                    .ToList();
            }
            return allScavs;
        }
        
        public static List<IPlayer> GetAllCachedBots()
        {
            return GetAllPMCs()
                .Concat(GetAllScavs())
                .ToList();
        }

        public static List<ISpawnPoint> GetAllSpawnPoints()
        {
            if (allSpawnPoints.Count == 0)
            {
                allSpawnPoints = SpawnPointManagerClass.CreateFromScene().ToList();
            }
            return allSpawnPoints;
        }

        public static List<ISpawnPoint> GetPlayerSpawnPoints()
        {
            if (playerSpawnPoints.Count == 0)
            {
                playerSpawnPoints = GetAllSpawnPoints()
                    .Where(x => x.Categories.ContainPlayerCategory())
                    .Where(x => x.Infiltration != null)
                    .ToList();
            }
            return playerSpawnPoints;
        }

        public static List<ISpawnPoint> GetBotNoBossNoSnipeSpawnPoints()
        {
            if (backupPlayerSpawnPoints.Count == 0)
            {
                backupPlayerSpawnPoints = GetAllSpawnPoints()
                    .Where(x => x.Categories.ContainBotCategory())
                    .Where(x => !x.Categories.ContainBossCategory())
                    .Where(x => !x.IsSnipeZone)
                    .ToList();
            }
            return backupPlayerSpawnPoints;
        }
        
        public static List<ISpawnPoint> GetCombinedPlayerAndBotSpawnPoints()
        {
            if (combinedSpawnPoints.Count == 0)
            {
                combinedSpawnPoints = GetPlayerSpawnPoints()
                    .Concat(GetBotNoBossNoSnipeSpawnPoints())
                    .Distinct()
                    .ToList();
            }

            return combinedSpawnPoints;
        }
        
        public static List<BotZone> GetMapBotZones()
        {
            List<BotZone> shuffledList = currentMapZones.OrderBy(_ => Guid.NewGuid()).ToList();
            return shuffledList;
        }
    }
}
