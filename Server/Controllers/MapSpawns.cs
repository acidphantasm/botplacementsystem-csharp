using _botplacementsystem.Globals;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;

namespace _botplacementsystem.Controllers;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]
public class MapSpawns(
    ISptLogger<MapSpawns> logger,
    BossSpawns bossSpawns,
    ScavSpawns scavSpawns,
    PmcSpawns pmcSpawns,
    VanillaAdjustments vanillaAdjustments,
    ICloner cloner,
    DatabaseService databaseService)
{
    private List<string> _validMaps =
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

    private ISptLogger<MapSpawns> _logger = logger;

    public void ConfigureInitialData()
    {
        _locationData = databaseService.GetLocations().GetDictionary();

        foreach (var map in _validMaps)
        {
            var actualKey = databaseService.GetLocations().GetMappedKey(map);
            _locationData[actualKey].Base.BossLocationSpawn = [];
            _botMapCache[map] = [];
            _scavMapCache[map] = [];
            if (ModConfig.Config.ScavConfig.Waves.Enable && ModConfig.Config.ScavConfig.StartingScavs.Enable)
            {
                vanillaAdjustments.EnableAllSpawnSystems(_locationData[actualKey].Base);
            }
            else if (!ModConfig.Config.ScavConfig.Waves.Enable && ModConfig.Config.ScavConfig.StartingScavs.Enable)
            {
                vanillaAdjustments.DisableNewSpawnSystem(_locationData[actualKey].Base);
            }
            else if (!ModConfig.Config.ScavConfig.Waves.Enable && !ModConfig.Config.ScavConfig.StartingScavs.Enable)
            {
                vanillaAdjustments.DisableAllSpawnSystems(_locationData[actualKey].Base);
            }
            else if (ModConfig.Config.ScavConfig.Waves.Enable && !ModConfig.Config.ScavConfig.StartingScavs.Enable)
            {
                vanillaAdjustments.DisableOldSpawnSystem(_locationData[actualKey].Base);
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

            if (mapData.Any())
            {
                foreach (var spawn in mapData)
                {
                    _botMapCache[map].Add(spawn);
                }
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
            
            if (mapData.Any())
            {
                foreach (var spawn in mapData)
                {
                    _botMapCache[map].Add(spawn);
                }
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
            
            if (mapData.Any())
            {
                foreach (var spawn in mapData)
                {
                    _scavMapCache[map].Add(spawn);
                }
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
}