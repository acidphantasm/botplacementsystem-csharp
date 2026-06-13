using BotPlacementSystemServer.Globals;
using BotPlacementSystemServer.Service;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;

namespace BotPlacementSystemServer.Controllers;

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = OnLoadOrder.PostDBModLoader + 69420)]
public class MapSpawns(
    ISptLogger<MapSpawns> logger,
    BossSpawns bossSpawns,
    ScavSpawns scavSpawns,
    PmcSpawns pmcSpawns,
    VanillaAdjustments vanillaAdjustments,
    ICloner cloner,
    DatabaseService databaseService,
    RaidLifecycleService raidLifecycleService)
    : IOnLoad
{
    private readonly List<string> _validMaps =
    [
        "bigmap",
        "factory4_day",
        "factory4_night",
        "interchange",
        "laboratory",
        "lighthouse",
        "rezervbase",
        "sandbox",
        "sandbox_high",
        "shoreline",
        "tarkovstreets",
        "woods",
        "labyrinth"
    ];

    private readonly Dictionary<string, List<BossLocationSpawn>> _botMapCache = new();
    private readonly Dictionary<string, List<Wave>> _scavMapCache = new();
    private Dictionary<string, Location> _locationData = new();
    
    public Task OnLoad()
    {
        ConfigureInitialData();
        
        ModConfig.ConfigChanged += ConfigureInitialData;
        raidLifecycleService.RaidFinished += RebuildMap;
        
        return Task.CompletedTask;
    }

    private void ConfigureInitialData()
    {
        _locationData = databaseService.GetLocations().GetDictionary();
        
        foreach (var map in _validMaps)
        {
            var actualKey = databaseService.GetLocations().GetMappedKey(map);
            _locationData[actualKey].Base.BossLocationSpawn = [];
            _botMapCache[map] = [];
            _scavMapCache[map] = [];
            switch (ModConfig.Config.ScavConfig.Waves.Enable)
            {
                case true when ModConfig.Config.ScavConfig.StartingScavs.Enable:
                    vanillaAdjustments.EnableAllSpawnSystems(_locationData[actualKey].Base);
                    break;
                case false when ModConfig.Config.ScavConfig.StartingScavs.Enable:
                    vanillaAdjustments.DisableNewSpawnSystem(_locationData[actualKey].Base);
                    break;
                case false when !ModConfig.Config.ScavConfig.StartingScavs.Enable:
                    vanillaAdjustments.DisableAllSpawnSystems(_locationData[actualKey].Base);
                    break;
                case true when !ModConfig.Config.ScavConfig.StartingScavs.Enable:
                    vanillaAdjustments.DisableOldSpawnSystem(_locationData[actualKey].Base);
                    break;
            }

            vanillaAdjustments.RemoveExistingWaves(_locationData[actualKey].Base);
            vanillaAdjustments.FixPMCHostility(_locationData[actualKey].Base);
            vanillaAdjustments.AdjustNewWaveSettings(_locationData[actualKey].Base);
        }

        vanillaAdjustments.CheckAndAddScavBrainTypes();
        vanillaAdjustments.DisableVanillaSettings();
        vanillaAdjustments.RemoveCustomPMCWaves();
        BuildInitialCache();
    }

    private void BuildInitialCache()
    {
        BuildBossWaves();
        BuildPmcWaves();
        BuildStartingScavs();
        ReplaceOriginalLocations();
    }

    private void BuildBossWaves()
    {
        foreach (var map in _validMaps)
        {
            var actualKey = databaseService.GetLocations().GetMappedKey(map);
            
            var mapData =
                bossSpawns.GetCustomMapData(map, _locationData[actualKey].Base.EscapeTimeLimit.GetValueOrDefault());

            if (mapData.Count == 0)
            {
                continue;
            }
            
            foreach (var spawn in mapData)
            {
                _botMapCache[map].Add(spawn);
            }
        }
    }

    private void BuildPmcWaves()
    {
        foreach (var map in _validMaps)
        {
            var actualKey = databaseService.GetLocations().GetMappedKey(map);
            
            var mapData = pmcSpawns.GetCustomMapData(map,
                _locationData[actualKey].Base.EscapeTimeLimit.GetValueOrDefault()
            );

            if (mapData.Count == 0)
            {
                continue;
            }
            
            foreach (var spawn in mapData)
            {
                _botMapCache[map].Add(spawn);
            }
        }
    }

    private void BuildStartingScavs()
    {
        foreach (var map in _validMaps)
        {
            if ((map == "laboratory" && !ModConfig.Config.ScavConfig.Waves.AllowScavsOnLaboratory) || 
                (map == "labyrinth" && !ModConfig.Config.ScavConfig.Waves.AllowScavsOnLabyrinth)) continue;
            
            var mapData = scavSpawns.GetCustomMapData(map);

            if (mapData.Count == 0)
            {
                continue;
            }
            
            foreach (var spawn in mapData)
            {
                _scavMapCache[map].Add(spawn);
            }
        }
    }

    private void ReplaceOriginalLocations()
    {
        foreach (var map in _validMaps)
        {
            var actualKey = databaseService.GetLocations().GetMappedKey(map);
            _locationData[actualKey].Base.BossLocationSpawn = cloner.Clone(_botMapCache[map]);
            _locationData[actualKey].Base.Waves = cloner.Clone(_scavMapCache[map]);
        }
    }

    private void RebuildMap(string location)
    {
        if (!_validMaps.Contains(location)) return;
    
        _botMapCache[location] = [];
        _scavMapCache[location] = [];

        BuildBossWavesForMap(location);
        BuildPmcWavesForMap(location);
        BuildStartingScavsForMap(location);
        ReplaceOriginalLocationForMap(location);
    }
    
    private void BuildBossWavesForMap(string location)
    {
        var actualKey = databaseService.GetLocations().GetMappedKey(location);
        var mapData = bossSpawns.GetCustomMapData(location, _locationData[actualKey].Base.EscapeTimeLimit.GetValueOrDefault());
        foreach (var spawn in mapData)
        {
            _botMapCache[location].Add(spawn);
        }
    }

    private void BuildPmcWavesForMap(string location)
    {
        var actualKey = databaseService.GetLocations().GetMappedKey(location);
        var mapData = pmcSpawns.GetCustomMapData(location, _locationData[actualKey].Base.EscapeTimeLimit.GetValueOrDefault());
        foreach (var spawn in mapData)
        {
            _botMapCache[location].Add(spawn);
        }
    }

    private void BuildStartingScavsForMap(string location)
    {
        if ((location == "laboratory" && !ModConfig.Config.ScavConfig.Waves.AllowScavsOnLaboratory) || (location == "labyrinth" && !ModConfig.Config.ScavConfig.Waves.AllowScavsOnLabyrinth)) 
            return;
        
        var mapData = scavSpawns.GetCustomMapData(location);
        foreach (var spawn in mapData)
        {
            _scavMapCache[location].Add(spawn);
        }
    }

    private void ReplaceOriginalLocationForMap(string location)
    {
        var actualKey = databaseService.GetLocations().GetMappedKey(location);
        _locationData[actualKey].Base.BossLocationSpawn = cloner.Clone(_botMapCache[location]);
        _locationData[actualKey].Base.Waves = cloner.Clone(_scavMapCache[location]);
    }
}