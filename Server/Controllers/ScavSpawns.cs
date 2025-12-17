using _botplacementsystem.Globals;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Server.Core.Utils.Collections;

namespace _botplacementsystem.Controllers;

[Injectable]
public class ScavSpawns(
    ISptLogger<ScavSpawns> logger,
    ICloner cloner,
    RandomUtil randomUtil,
    DatabaseService databaseService)
{
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

        if (!databaseService.GetLocations().GetDictionary().TryGetValue(databaseService.GetLocations().GetMappedKey(location), out var locationData))
        {
            logger.Error($"unable to find location: {location}");
            return scavWaveSpawnInfo;
        }

        var waveLength = locationData.Base.Waves.Count;
        if (!ModConfig.Config.ScavConfig.StartingScavs.MaxBotSpawns.TryGetValue(location, out var maxStartingSpawns))
        {
            logger.Error($"unable to find location MaxBotSpawns: {location}");
            return scavWaveSpawnInfo;
        }
        
        var scavCap = latestart ? maxStartingSpawns * 0.75 : maxStartingSpawns;
        var playerScavChance = latestart ? 60 : 10;

        var availableSpawnZones = botRole == "assault"
            ? new ExhaustableArray<string>(GetNonMarksmanSpawnZones(location), randomUtil, cloner)
            : new ExhaustableArray<string>(GetMarksmanSpawnZones(location), randomUtil, cloner);

        var marksmanCount = 0;

        while (currentCount < scavCap)
        {
            if (currentCount >= maxStartingSpawns) break;
            var scavDefaultData = cloner.Clone(ModConfig.ScavDefaults);
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

            if (scavDefaultData is null) continue;
            
            scavDefaultData.SlotsMin = botRole == "assault" ? 0 : 1;
            scavDefaultData.SlotsMax = botRole == "assault" ? 1 : 2;
            scavDefaultData.TimeMin = -1;
            scavDefaultData.TimeMax = -1;
            scavDefaultData.Number = currentCount;
            scavDefaultData.WildSpawnType = botRole == "assault" ? WildSpawnType.assault : WildSpawnType.marksman;
            scavDefaultData.IsPlayers = botRole == "assault" && randomUtil.GetChance100(playerScavChance);
            scavDefaultData.SpawnPoints = selectedSpawnZone;

            currentCount++;
            scavWaveSpawnInfo.Add(scavDefaultData);
        }

        return scavWaveSpawnInfo;
    }

    private List<string>? GetMarksmanSpawnZones(string location)
    {
        return location switch
        {
            "bigmap" => ModConfig.MapZoneDefaults.CustomsSnipeSpawnZones,
            "lighthouse" => ModConfig.MapZoneDefaults.LighthouseSnipeSpawnZones,
            "sandbox" or "sandbox_high" => ModConfig.MapZoneDefaults.GroundZeroSnipeSpawnZones,
            "shoreline" => ModConfig.MapZoneDefaults.ShorelineSnipeSpawnZones,
            "tarkovstreets" => ModConfig.MapZoneDefaults.StreetsSnipeSpawnZones,
            "woods" => ModConfig.MapZoneDefaults.WoodsSnipeSpawnZones,
            _ => null
        };
    }

    private List<string>? GetNonMarksmanSpawnZones(string location)
    {
        return location switch
        {
            "bigmap" => ModConfig.MapZoneDefaults.CustomsSpawnZones,
            "factory4_day" or "factory4_night" => ModConfig.MapZoneDefaults.FactorySpawnZones,
            "interchange" => ModConfig.MapZoneDefaults.InterchangeSpawnZones,
            "laboratory" => ModConfig.MapZoneDefaults.LabsNonGateSpawnZones,
            "lighthouse" => ModConfig.MapZoneDefaults.LighthouseNonWaterTreatmentSpawnZones,
            "rezervbase" => ModConfig.MapZoneDefaults.ReserveSpawnZones,
            "sandbox" or "sandbox_high" => ModConfig.MapZoneDefaults.GroundZeroSpawnZones,
            "shoreline" => ModConfig.MapZoneDefaults.ShorelineSpawnZones,
            "tarkovstreets" => ModConfig.MapZoneDefaults.StreetsSpawnZones,
            "woods" => ModConfig.MapZoneDefaults.WoodsSpawnZones,
            "labyrinth" => ModConfig.MapZoneDefaults.LabyrinthSpawnZones,
            _ => null
        };
    }
}