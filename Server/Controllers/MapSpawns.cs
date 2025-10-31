using _botplacementsystem.Globals;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;

namespace _botplacementsystem.Controllers;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]
public class MapSpawns
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

    public Dictionary<string, List<BossLocationSpawn>> BotMapCache = new();
    public Dictionary<string, List<Wave>> ScavMapCache = new();
    public Dictionary<string, Location> LocationData = new();

    private ISptLogger<MapSpawns> _logger;
    private BossSpawns _bossSpawns;
    private ScavSpawns _scavSpawns;
    private PmcSpawns _pmcSpawns;
    private VanillaAdjustments _vanillaAdjustments;
    private ICloner _cloner;
    private DatabaseService _databaseService;

    public MapSpawns
    (
        ISptLogger<MapSpawns> logger,
        BossSpawns bossSpawns,
        ScavSpawns scavSpawns,
        PmcSpawns pmcSpawns,
        VanillaAdjustments vanillaAdjustments,
        ICloner cloner,
        DatabaseService databaseService
    )
    {
        _logger = logger;
        _bossSpawns = bossSpawns;
        _scavSpawns = scavSpawns;
        _pmcSpawns = pmcSpawns;
        _vanillaAdjustments = vanillaAdjustments;
        _cloner = cloner;
        _databaseService = databaseService;
    }

    public void ConfigureInitialData()
    {
        LocationData = _databaseService.GetLocations().GetDictionary();

        foreach (var map in _validMaps)
        {
            var actualkey = _databaseService.GetLocations().GetMappedKey(map);
            LocationData[actualkey].Base.BossLocationSpawn = [];
            BotMapCache[map] = [];
            ScavMapCache[map] = [];
            if (ModConfig.Config.ScavConfig.Waves.Enable && ModConfig.Config.ScavConfig.StartingScavs.Enable)
            {
                _vanillaAdjustments.EnableAllSpawnSystems(LocationData[actualkey].Base);
            }
            else if (!ModConfig.Config.ScavConfig.Waves.Enable && ModConfig.Config.ScavConfig.StartingScavs.Enable)
            {
                _vanillaAdjustments.DisableNewSpawnSystem(LocationData[actualkey].Base);
            }
            else if (!ModConfig.Config.ScavConfig.Waves.Enable && !ModConfig.Config.ScavConfig.StartingScavs.Enable)
            {
                _vanillaAdjustments.DisableAllSpawnSystems(LocationData[actualkey].Base);
            }
            else if (ModConfig.Config.ScavConfig.Waves.Enable && !ModConfig.Config.ScavConfig.StartingScavs.Enable)
            {
                _vanillaAdjustments.DisableOldSpawnSystem(LocationData[actualkey].Base);
            }

            _vanillaAdjustments.RemoveExistingWaves(LocationData[actualkey].Base);
            _vanillaAdjustments.FixPMCHostility(LocationData[actualkey].Base);
            _vanillaAdjustments.AdjustNewWaveSettings(LocationData[actualkey].Base);
        }

        _vanillaAdjustments.CheckAndAddScavBrainTypes();
        _vanillaAdjustments.DisableVanillaSettings();
        _vanillaAdjustments.RemoveCustomPMCWaves();
        BuildInitialCache();
    }

    public void BuildInitialCache()
    {
        BuildBossWaves();
        BuildPMCWaves();
        BuildStartingScavs();
        ReplaceOriginalLocations();
    }

    private void BuildBossWaves()
    {
        foreach (var map in _validMaps)
        {
            var actualKey = _databaseService.GetLocations().GetMappedKey(map);
            
            var mapData =
                _bossSpawns.GetCustomMapData(map, LocationData[actualKey].Base.EscapeTimeLimit.GetValueOrDefault());

            if (mapData.Any())
            {
                foreach (var spawn in mapData)
                {
                    BotMapCache[map].Add(spawn);
                }
            }
        }
    }

    private void BuildPMCWaves()
    {
        foreach (var map in _validMaps)
        {
            var actualKey = _databaseService.GetLocations().GetMappedKey(map);
            
            var mapData = _pmcSpawns.GetCustomMapData(map,
                LocationData[actualKey].Base.EscapeTimeLimit.GetValueOrDefault()
            );
            
            if (mapData.Any())
            {
                foreach (var spawn in mapData)
                {
                    BotMapCache[map].Add(spawn);
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
            
            var mapData = _scavSpawns.GetCustomMapData(map);
            
            if (mapData.Any())
            {
                foreach (var spawn in mapData)
                {
                    ScavMapCache[map].Add(spawn);
                }
            }
        }
    }

    private void ReplaceOriginalLocations()
    {
        foreach (var map in _validMaps)
        {
            var actualKey = _databaseService.GetLocations().GetMappedKey(map);
            LocationData[actualKey].Base.BossLocationSpawn = _cloner.Clone(BotMapCache[map]);
            LocationData[actualKey].Base.Waves = _cloner.Clone(ScavMapCache[map]);
        }
    }
}