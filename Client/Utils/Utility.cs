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
        private static string _mainProfileID = string.Empty;
        public static string MapName = string.Empty;
        public static List<IPlayer> AllPmcs = new();
        public static readonly List<IPlayer> AllBots = new();
        public static List<IPlayer> AllScavs = new();
        public static List<ISpawnPoint> AllSpawnPoints = new();
        public static List<ISpawnPoint> PlayerSpawnPoints = new();
        public static List<ISpawnPoint> BackupPlayerSpawnPoints = new();
        public static List<ISpawnPoint> CombinedSpawnPoints = new();
        public static List<BotZone> CurrentMapZones = new();
        public static double BotsSpawnedPerPlayer;
        public static int ConnectedPlayerCount;
        public static List<BotZone> CachedNonSnipeZones;

        public static readonly Dictionary<string, string[]> MapHotSpots = new()
        {
            {"rezervbase", ["ZoneSubStorage", "ZoneBarrack"]},
            {"shoreline", ["ZoneSanatorium1", "ZoneSanatorium2"]},
            {"lighthouse", ["Zone_LongRoad", "Zone_Chalet", "Zone_Village"]},
            {"interchange", ["ZoneCenter", "ZoneCenterBot"]},
            {"bigmap", ["ZoneDormitory", "ZoneScavBase", "ZoneOldAZS", "ZoneGasStation"]}
        };

        public void Awake()
        {
            _mainProfileID = GetPlayerProfile().ProfileId;
            Plugin.LogSource.LogInfo(_mainProfileID);
        }

        public static Profile GetPlayerProfile()
        {
            return ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile;
        }

        public static string CurrentLocation
        {
            get
            {
                if (MapName != string.Empty) return MapName;

                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld != null)
                {
                    MapName = gameWorld.LocationId;
                    return MapName;
                }
                return "default";
            }
        }

        public static List<IPlayer> GetAllPMCs()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld != null)
            {
                AllPmcs = gameWorld.RegisteredPlayers
                    .Where(x => x.Profile.Side == EPlayerSide.Bear || x.Profile.Side == EPlayerSide.Usec)
                    .ToList();
            }
            return AllPmcs;
        }

        public static List<IPlayer> GetAllScavs()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld != null)
            {
                AllScavs = gameWorld.RegisteredPlayers
                    .Where(x => x.Profile.Info.Settings.Role == WildSpawnType.assault)
                    .ToList();
            }
            return AllScavs;
        }
        
        public static List<IPlayer> GetAllCachedBots()
        {
            return GetAllPMCs()
                .Concat(GetAllScavs())
                .ToList();
        }

        public static List<ISpawnPoint> GetAllSpawnPoints()
        {
            if (AllSpawnPoints.Count == 0)
            {
                AllSpawnPoints = SpawnPointManagerClass.CreateFromScene().ToList();
            }
            return AllSpawnPoints;
        }

        public static List<ISpawnPoint> GetPlayerSpawnPoints()
        {
            if (PlayerSpawnPoints.Count == 0)
            {
                PlayerSpawnPoints = GetAllSpawnPoints()
                    .Where(x => x.Categories.ContainPlayerCategory())
                    .Where(x => x.Infiltration != null)
                    .ToList();
            }
            return PlayerSpawnPoints;
        }

        public static List<ISpawnPoint> GetBotNoBossNoSnipeSpawnPoints()
        {
            if (BackupPlayerSpawnPoints.Count == 0)
            {
                BackupPlayerSpawnPoints = GetAllSpawnPoints()
                    .Where(x => x.Categories.ContainBotCategory())
                    .Where(x => !x.Categories.ContainBossCategory())
                    .Where(x => !x.IsSnipeZone)
                    .ToList();
            }
            return BackupPlayerSpawnPoints;
        }
        
        public static List<ISpawnPoint> GetCombinedPlayerAndBotSpawnPoints()
        {
            if (CombinedSpawnPoints.Count == 0)
            {
                CombinedSpawnPoints = GetPlayerSpawnPoints()
                    .Concat(GetBotNoBossNoSnipeSpawnPoints())
                    .Distinct()
                    .ToList();
            }

            return CombinedSpawnPoints;
        }
        
        public static List<BotZone> GetMapBotZones()
        {
            List<BotZone> shuffledList = CurrentMapZones.OrderBy(_ => Guid.NewGuid()).ToList();
            return shuffledList;
        }
    }
}
