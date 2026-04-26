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
        private static string _mapName = string.Empty;
        
        public static bool Initialized;
        
        // Spawn Points
        private static List<ISpawnPoint> _allSpawnPoints = new();
        public static List<ISpawnPoint> PlayerSpawnPoints = new();
        public static List<ISpawnPoint> BackupPlayerSpawnPoints = new();
        public static List<ISpawnPoint> CombinedSpawnPoints = new();
        private static Dictionary<string, List<ISpawnPoint>> _cachedZoneSpawnPoints = new();
        
        // Zones
        public static List<BotZone> CurrentMapZones = new();
        public static List<BotZone> CachedNonSnipeZones = new();
        
        // Bot Trackers
        public static List<Player> CachedPmcs = new();
        public static List<Player> CachedAssaultBots = new();
        public static List<Player> CachedBosses = new();
        public static List<Player> CachedConnectedPlayers = new();
        public static double BotsSpawnedPerPlayer = 0.0d;

        public static readonly Dictionary<string, string[]> MapHotSpots = new()
        {
            {"rezervbase", ["ZoneSubStorage", "ZoneBarrack"]},
            {"shoreline", ["ZoneSanatorium1", "ZoneSanatorium2"]},
            {"lighthouse", ["Zone_LongRoad", "Zone_Chalet", "Zone_Village"]},
            {"interchange", ["ZoneCenter", "ZoneCenterBot"]},
            {"bigmap", ["ZoneDormitory", "ZoneScavBase", "ZoneOldAZS", "ZoneGasStation"]}
        };

        public static Profile GetPlayerProfile()
        {
            return ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile;
        }

        public static string CurrentLocation
        {
            get
            {
                if (_mapName != string.Empty) return _mapName;

                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld != null)
                {
                    _mapName = gameWorld.LocationId;
                    return _mapName;
                }
                return "default";
            }
        }
        
        public static void InitializeSpawnPoints(BotZone[] allBotZones)
        {
            _mapName = string.Empty;
            
            _allSpawnPoints.Clear();
            PlayerSpawnPoints.Clear();
            BackupPlayerSpawnPoints.Clear();
            CombinedSpawnPoints.Clear();
            
            CachedNonSnipeZones.Clear();
            CurrentMapZones.Clear();
            
            CachedPmcs.Clear();
            CachedAssaultBots.Clear();
            CachedBosses.Clear();
            CachedConnectedPlayers.Clear();
            
            _cachedZoneSpawnPoints.Clear();
            
            BotsSpawnedPerPlayer = 0.0;
            
            // Recache spawn points now
            _allSpawnPoints = LocationScene.GetAllObjectsAndWhenISayAllIActuallyMeanIt<SpawnPointMarker>()
                .Select(x => x.SpawnPoint)
                .ToList();
    
            PlayerSpawnPoints = _allSpawnPoints
                .Where(x => x.Categories.ContainPlayerCategory() && x.Infiltration != null)
                .ToList();
        
            BackupPlayerSpawnPoints = _allSpawnPoints
                .Where(x => x.Categories.ContainBotCategory() 
                            && !x.Categories.ContainBossCategory() 
                            && !x.IsSnipeZone)
                .ToList();
        
            CombinedSpawnPoints = PlayerSpawnPoints
                .Concat(BackupPlayerSpawnPoints)
                .ToList();
            
            foreach (var botZone in allBotZones)
            {
                var zoneName = botZone.NameZone;
                foreach (var spawnPoint in botZone.SpawnPoints)
                {
                    if (spawnPoint.Categories != ESpawnCategoryMask.All && !spawnPoint.Categories.ContainBotCategory())
                    {
                        continue;
                    }
                    if (!_cachedZoneSpawnPoints.TryGetValue(zoneName, out var list))
                    {
                        list = new List<ISpawnPoint>();
                        _cachedZoneSpawnPoints[zoneName] = list;
                    }

                    list.Add(spawnPoint);
                }
            }
            
            Initialized = true;
        }
        
        public static List<ISpawnPoint> GetZoneSpawnPoints(BotZone botZone)
        {
            return _cachedZoneSpawnPoints.TryGetValue(botZone.NameZone, out var points) ? points : new List<ISpawnPoint>();
        }
        
        public static BotZone GetNewValidBotZone()
        {
            var randomIndex = UnityEngine.Random.Range(0, CachedNonSnipeZones.Count);
            return CachedNonSnipeZones[randomIndex];
        }

        public static bool IsPlayerHeadless(Player player)
        {
            return player.Profile.Info.MemberCategory == EMemberCategory.UnitTest;
        }

        public static bool IsPlayerHeadless(IPlayer player)
        {
            return player.Profile.Info.MemberCategory == EMemberCategory.UnitTest;
        }
    }
}
