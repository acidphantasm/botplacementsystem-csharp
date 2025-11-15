using _botplacementsystem.Globals;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Server.Core.Utils.Collections;

namespace _botplacementsystem.Controllers;

[Injectable]
public class ScavSpawns
{
    private ISptLogger<ScavSpawns> _logger;
    private RandomUtil _randomUtil;
    private ICloner _cloner;
    private DatabaseService _databaseService;

    public ScavSpawns
    (
        ISptLogger<ScavSpawns> logger,
        ICloner cloner,
        RandomUtil randomUtil,
        DatabaseService databaseService,
        JsonUtil jsonUtil
    )
    {
        _logger = logger;
        _cloner = cloner;
        _randomUtil = randomUtil;
        _databaseService = databaseService;
    }

    public List<Wave> GetCustomMapData(string location)
    {
        return GetConfigValueForLocation(location);
    }

    private List<Wave> GetConfigValueForLocation(string location)
    {
        var scavSpawnInfo = new List<Wave>();
        if (ModConfig.Config.ScavConfig.StartingScavs.StartingMarksman)
        {
            var marksmanSpawn = GenerateStartingScavs(location, "marksman");
            if (marksmanSpawn.Any())
                scavSpawnInfo.AddRange(marksmanSpawn);
        }

        if (ModConfig.Config.ScavConfig.StartingScavs.Enable)
        {
            var assaultSpawn = GenerateStartingScavs(location, "assault", false, scavSpawnInfo.Count);
            if (assaultSpawn.Any())
                scavSpawnInfo.AddRange(assaultSpawn);
        }

        return scavSpawnInfo;
    }

    public List<Wave> GenerateStartingScavs(string location, string? botRole = "assault", bool latestart = false, int currentCount = 0)
    {
        var scavWaveSpawnInfo = new List<Wave>();

        if (!_databaseService.GetLocations().GetDictionary().TryGetValue(_databaseService.GetLocations().GetMappedKey(location), out var locationData))
        {
            _logger.Error($"unable to find location: {location}");
            return scavWaveSpawnInfo;
        }

        var waveLength = locationData.Base.Waves.Count;
        if (!ModConfig.Config.ScavConfig.StartingScavs.MaxBotSpawns.TryGetValue(location, out var maxStartingSpawns))
        {
            _logger.Error($"unable to find location MaxBotSpawns: {location}");
            return scavWaveSpawnInfo;
        }
        
        var scavCap = latestart ? maxStartingSpawns * 0.75 : maxStartingSpawns;
        var playerScavChance = latestart ? 60 : 10;

        var availableSpawnZones = botRole == "assault"
            ? new ExhaustableArray<string>(GetNonMarksmanSpawnZones(location), _randomUtil, _cloner)
            : new ExhaustableArray<string>(GetMarksmanSpawnZones(location), _randomUtil, _cloner);

        var marksmanCount = 0;
        var assaultScavsCount = waveLength + 1;

        while (currentCount < scavCap)
        {
            if (currentCount >= maxStartingSpawns) break;
            var scavDefaultData = _cloner.Clone(ModConfig.ScavDefaults);
            var selectedSpawnZone =
                location.Contains("factory") || (botRole == "assault" && location.Contains("sandbox")) || location.Contains("labyrinth") || !availableSpawnZones.HasValues()
                    ? ""
                    : availableSpawnZones.GetRandomValue();

            if (botRole != "assault")
            {
                if (!availableSpawnZones.HasValues() && (selectedSpawnZone == String.Empty || selectedSpawnZone is null)) break;
                if (marksmanCount >= 3) break;
                marksmanCount++;
            }

            scavDefaultData.SlotsMin = botRole == "assault" ? 0 : 1;
            scavDefaultData.SlotsMax = botRole == "assault" ? 1 : 2;
            scavDefaultData.TimeMin = -1;
            scavDefaultData.TimeMax = -1;
            scavDefaultData.Number = currentCount;
            scavDefaultData.WildSpawnType = botRole == "assault" ? WildSpawnType.assault : WildSpawnType.marksman;
            scavDefaultData.IsPlayers = botRole == "assault" && _randomUtil.GetChance100(playerScavChance);
            scavDefaultData.SpawnPoints = selectedSpawnZone;

            if (botRole.Contains("assault")) assaultScavsCount++;
            currentCount++;
            scavWaveSpawnInfo.Add(scavDefaultData);
        }

        return scavWaveSpawnInfo;
    }

    private List<string>? GetMarksmanSpawnZones(string location)
    {
        switch (location)
        {
            case "bigmap":
                return ModConfig.MapZoneDefaults.CustomsSnipeSpawnZones;
            case "lighthouse":
                return ModConfig.MapZoneDefaults.LighthouseSnipeSpawnZones;
            case "sandbox":
            case "sandbox_high":
                return ModConfig.MapZoneDefaults.GroundZeroSnipeSpawnZones;
            case "shoreline":
                return ModConfig.MapZoneDefaults.ShorelineSnipeSpawnZones;
            case "tarkovstreets":
                return ModConfig.MapZoneDefaults.StreetsSnipeSpawnZones;
            case "woods":
                return ModConfig.MapZoneDefaults.WoodsSnipeSpawnZones;
            case "labyrinth":
            case "laboratory":
            case "interchange":
            case "rezervbase":
            case "factory4_day":
            case "factory4_night":
            default:
                return null;
        }
    }

    private List<string>? GetNonMarksmanSpawnZones(string location)
    {
        switch (location)
        {
            case "bigmap":
                return ModConfig.MapZoneDefaults.CustomsSpawnZones;
            case "factory4_day":
            case "factory4_night":
                return ModConfig.MapZoneDefaults.FactorySpawnZones;
            case "interchange":
                return ModConfig.MapZoneDefaults.InterchangeSpawnZones;
            case "laboratory":
                return ModConfig.MapZoneDefaults.LabsNonGateSpawnZones;
            case "lighthouse":
                return ModConfig.MapZoneDefaults.LighthouseNonWaterTreatmentSpawnZones;
            case "rezervbase":
                return ModConfig.MapZoneDefaults.ReserveSpawnZones;
            case "sandbox":
            case "sandbox_high":
                return ModConfig.MapZoneDefaults.GroundZeroSpawnZones;
            case "shoreline":
                return ModConfig.MapZoneDefaults.ShorelineSpawnZones;
            case "tarkovstreets":
                return ModConfig.MapZoneDefaults.StreetsSpawnZones;
            case "woods":
                return ModConfig.MapZoneDefaults.WoodsSpawnZones;
            case "labyrinth":
                return ModConfig.MapZoneDefaults.LabyrinthSpawnZones;
            default:
                return null;
        }
    }
}