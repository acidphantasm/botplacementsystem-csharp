using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace acidphantasm_botplacementsystem.Utils
{
    internal class Utility
    {
        public static string mainProfileID = string.Empty;
        public static string mapName = string.Empty;
        public static List<IPlayer> allPMCs = new List<IPlayer>();
        public static List<IPlayer> allBots = new List<IPlayer>();
        public static List<IPlayer> allScavs = new List<IPlayer>();
        public static List<ISpawnPoint> allSpawnPoints = new List<ISpawnPoint>();
        public static List<ISpawnPoint> playerSpawnPoints = new List<ISpawnPoint>();
        public static List<BotZone> currentMapZones = new List<BotZone>();

        public static Dictionary<string, string[]> mapHotSpots = new Dictionary<string, string[]>()
        {
            {"rezervbase", ["ZoneSubStorage", "ZoneBarrack"]},
            {"shoreline", ["ZoneSanatorium1", "ZoneSanatorium2"]},
            {"lighthouse", ["Zone_LongRoad", "Zone_Chalet", "Zone_Village"]},
            {"interchange", ["ZoneCenter", "ZoneCenterBot"]},
            {"bigmap", ["ZoneDormitory", "ZoneScavBase"]}
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

        public static string GetCurrentLocation()
        {
            if (mapName != string.Empty) return mapName;

            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld != null)
            {
                mapName = gameWorld.MainPlayer.Location;
                return mapName;
            }
            return "default";
        }
        public static List<IPlayer> GetAllPMCs()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld != null)
            {
                allPMCs = gameWorld.RegisteredPlayers
                    .Where(x => x.Profile.Side == EPlayerSide.Bear || x.Profile.Side == EPlayerSide.Usec)
                    .ToList();
                return allPMCs;
            }
            return new List<IPlayer>();
        }
        public static List<IPlayer> GetAllScavs()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld != null)
            {
                allScavs = gameWorld.RegisteredPlayers
                    .Where(x => x.Profile.Info.Settings.Role == WildSpawnType.assault)
                    .ToList();
                return allScavs;
            }
            return new List<IPlayer>();
        }
        public static List<IPlayer> GetAllBots()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld != null)
            {
                allBots = gameWorld.RegisteredPlayers.ToList();
                return allBots;
            }
            return new List<IPlayer>();
        }

        public static List<ISpawnPoint> GetAllSpawnPoints()
        {
            if (allSpawnPoints.Count == 0)
            {
                Plugin.LogSource.LogInfo("Getting All SpawnPoints");
                allSpawnPoints = SpawnPointManagerClass.CreateFromScene().ToList();
            }
            return allSpawnPoints;
        }

        public static List<ISpawnPoint> GetPlayerSpawnPoints()
        {
            if (playerSpawnPoints.Count == 0 || allSpawnPoints.Count == 0)
            {
                Plugin.LogSource.LogInfo("Getting All Player SpawnPoints");
                playerSpawnPoints = GetAllSpawnPoints()
                .Where(x => x.Categories.ContainPlayerCategory())
                .Where(x => x.Infiltration != null)
                .ToList();
            }
            return playerSpawnPoints;
        }
        public static List<BotZone> GetMapBotZones()
        {
            List<BotZone> shuffledList = currentMapZones.OrderBy(_ => Guid.NewGuid()).ToList();
            return shuffledList;
        }
    }
}
