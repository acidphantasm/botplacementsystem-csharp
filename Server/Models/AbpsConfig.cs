using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;

namespace _botplacementsystem.Models;

public record AbpsConfig
{
    // export type DifficultyConfig = "easy" | "normal" | "hard" | "impossible";
    [JsonPropertyName("pmcDifficulty")] public Dictionary<string, double> PmcDifficulty { get; set; }
    [JsonPropertyName("pmcType")] public PmcTypeChance PmcType { get; set; }
    [JsonPropertyName("pmcConfig")] public PMCConfig PmcConfig { get; set; }
    [JsonPropertyName("scavConfig")] public ScavConfig ScavConfig { get; set; }
    [JsonPropertyName("bossDifficulty")] public Dictionary<string, double> BossDifficulty { get; set; }
    [JsonPropertyName("weeklyBoss")] public WeeklyBossConfig WeeklyBoss { get; set; }
    [JsonPropertyName("bossConfig")] public BossConfig BossConfig { get; set; }

    [JsonPropertyName("configAppSettings")]
    public ConfigAppSettings ConfigAppSettings { get; set; }
}

public record PmcTypeChance
{
    [JsonPropertyName("usecChance")] public double UsecChance { get; set; }
}
public record WeeklyBossConfig
{
    [JsonPropertyName("enable")] public bool Enable { get; set; } 
}
public record ConfigAppSettings
{
    [JsonPropertyName("showUndo")] public bool ShowUndo { get; set; }
    [JsonPropertyName("showDefault")] public bool ShowDefault { get; set; }
    [JsonPropertyName("disableAnimations")] public bool DisableAnimations { get; set; }
    [JsonPropertyName("allowUpdateChecks")] public bool AllowUpdateChecks { get; set; }
}

public class ValidLocationsMinMax
{
    [JsonPropertyName("bigmap")] public MinMax<int> Customs { get; set; }
    [JsonPropertyName("factory4_day")] public MinMax<int> Factory4Day { get; set; }
    [JsonPropertyName("factory4_night")] public MinMax<int> Factory4Night { get; set; }
    [JsonPropertyName("interchange")] public MinMax<int> Interchange { get; set; }
    [JsonPropertyName("laboratory")] public MinMax<int> Laboratory { get; set; }
    [JsonPropertyName("lighthouse")] public MinMax<int> Lighthouse { get; set; }
    [JsonPropertyName("rezervbase")] public MinMax<int> Reserve { get; set; }
    [JsonPropertyName("sandbox")] public MinMax<int> GroundZero { get; set; }
    [JsonPropertyName("sandbox_high")] public MinMax<int> GroundZeroHigh { get; set; }
    [JsonPropertyName("shoreline")] public MinMax<int> Shoreline { get; set; }
    [JsonPropertyName("tarkovstreets")] public MinMax<int> TarkovStreets { get; set; }
    [JsonPropertyName("woods")] public MinMax<int> Woods { get; set; }
    [JsonPropertyName("labyrinth")] public MinMax<int> Labyrinth { get; set; }
    
    public MinMax<int> this[string key]
    {
        get => key.ToLowerInvariant() switch
        {
            "bigmap" => Customs,
            "factory4_day" => Factory4Day,
            "factory4_night" => Factory4Night,
            "interchange" => Interchange,
            "laboratory" => Laboratory,
            "lighthouse" => Lighthouse,
            "rezervbase" => Reserve,
            "sandbox" => GroundZero,
            "sandbox_high" => GroundZeroHigh,
            "shoreline" => Shoreline,
            "tarkovstreets" => TarkovStreets,
            "woods" => Woods,
            "labyrinth" => Labyrinth,
            _ => throw new KeyNotFoundException($"Map key '{key}' not found.")
        };
        set
        {
            switch (key.ToLowerInvariant())
            {
                case "bigmap": Customs = value; break;
                case "factory4_day": Factory4Day = value; break;
                case "factory4_night": Factory4Night = value; break;
                case "interchange": Interchange = value; break;
                case "laboratory": Laboratory = value; break;
                case "lighthouse": Lighthouse = value; break;
                case "rezervbase": Reserve = value; break;
                case "sandbox": GroundZero = value; break;
                case "sandbox_high": GroundZeroHigh = value; break;
                case "shoreline": Shoreline = value; break;
                case "tarkovstreets": TarkovStreets = value; break;
                case "woods": Woods = value; break;
                case "labyrinth": Labyrinth = value; break;
                default: throw new KeyNotFoundException($"Map key '{key}' not found.");
            }
        }
    }
}

public class ValidLocationInt
{
    [JsonPropertyName("bigmap")] public int Customs { get; set; }
    [JsonPropertyName("factory4_day")] public int Factory4Day { get; set; }
    [JsonPropertyName("factory4_night")] public int Factory4Night { get; set; }
    [JsonPropertyName("interchange")] public int Interchange { get; set; }
    [JsonPropertyName("laboratory")] public int Laboratory { get; set; }
    [JsonPropertyName("lighthouse")] public int Lighthouse { get; set; }
    [JsonPropertyName("rezervbase")] public int Reserve { get; set; }
    [JsonPropertyName("sandbox")] public int GroundZero { get; set; }
    [JsonPropertyName("sandbox_high")] public int GroundZeroHigh { get; set; }
    [JsonPropertyName("shoreline")] public int Shoreline { get; set; }
    [JsonPropertyName("tarkovstreets")] public int TarkovStreets { get; set; }
    [JsonPropertyName("woods")] public int Woods { get; set; }
    [JsonPropertyName("labyrinth")] public int Labyrinth { get; set; }

    [JsonIgnore]
    public int this[string key]
    {
        get => key.ToLowerInvariant() switch
        {
            "bigmap" => Customs,
            "factory4_day" => Factory4Day,
            "factory4_night" => Factory4Night,
            "interchange" => Interchange,
            "laboratory" => Laboratory,
            "lighthouse" => Lighthouse,
            "rezervbase" => Reserve,
            "sandbox" => GroundZero,
            "sandbox_high" => GroundZeroHigh,
            "shoreline" => Shoreline,
            "tarkovstreets" => TarkovStreets,
            "woods" => Woods,
            "labyrinth" => Labyrinth,
            _ => throw new KeyNotFoundException($"Map key '{key}' not found.")
        };
        set
        {
            switch (key.ToLowerInvariant())
            {
                case "bigmap": Customs = value; break;
                case "factory4_day": Factory4Day = value; break;
                case "factory4_night": Factory4Night = value; break;
                case "interchange": Interchange = value; break;
                case "laboratory": Laboratory = value; break;
                case "lighthouse": Lighthouse = value; break;
                case "rezervbase": Reserve = value; break;
                case "sandbox": GroundZero = value; break;
                case "sandbox_high": GroundZeroHigh = value; break;
                case "shoreline": Shoreline = value; break;
                case "tarkovstreets": TarkovStreets = value; break;
                case "woods": Woods = value; break;
                case "labyrinth": Labyrinth = value; break;
                default: throw new KeyNotFoundException($"Map key '{key}' not found.");
            }
        }
    }
    
    public bool TryGetValue(string key, out int value)
    {
        try
        {
            value = this[key];
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }
}


public class ValidLocationString
{
    [JsonPropertyName("bigmap")] public required string Customs { get; set; }
    [JsonPropertyName("factory4_day")] public required string Factory4Day { get; set; }
    [JsonPropertyName("factory4_night")] public required string Factory4Night { get; set; }
    [JsonPropertyName("interchange")] public required string Interchange { get; set; }
    [JsonPropertyName("laboratory")] public required string Laboratory { get; set; }
    [JsonPropertyName("lighthouse")] public required string Lighthouse { get; set; }
    [JsonPropertyName("rezervbase")] public required string Reserve { get; set; }
    [JsonPropertyName("sandbox")] public required string GroundZero { get; set; }
    [JsonPropertyName("sandbox_high")]public required string GroundZeroHigh { get; set; }
    [JsonPropertyName("shoreline")] public required string Shoreline { get; set; }
    [JsonPropertyName("tarkovstreets")] public required string TarkovStreets { get; set; }
    [JsonPropertyName("woods")] public required string Woods { get; set; }
    [JsonPropertyName("labyrinth")] public required string Labyrinth { get; set; }

    [JsonIgnore]
    public string this[string key]
    {
        get => key.ToLowerInvariant() switch
        {
            "bigmap" => Customs,
            "factory4_day" => Factory4Day,
            "factory4_night" => Factory4Night,
            "interchange" => Interchange,
            "laboratory" => Laboratory,
            "lighthouse" => Lighthouse,
            "rezervbase" => Reserve,
            "sandbox" => GroundZero,
            "sandbox_high" => GroundZeroHigh,
            "shoreline" => Shoreline,
            "tarkovstreets" => TarkovStreets,
            "woods" => Woods,
            "labyrinth" => Labyrinth,
            _ => throw new KeyNotFoundException($"Map key '{key}' not found.")
        };
        set
        {
            switch (key.ToLowerInvariant())
            {
                case "bigmap": Customs = value; break;
                case "factory4_day": Factory4Day = value; break;
                case "factory4_night": Factory4Night = value; break;
                case "interchange": Interchange = value; break;
                case "laboratory": Laboratory = value; break;
                case "lighthouse": Lighthouse = value; break;
                case "rezervbase": Reserve = value; break;
                case "sandbox": GroundZero = value; break;
                case "sandbox_high": GroundZeroHigh = value; break;
                case "shoreline": Shoreline = value; break;
                case "tarkovstreets": TarkovStreets = value; break;
                case "woods": Woods = value; break;
                case "labyrinth": Labyrinth = value; break;
                default: throw new KeyNotFoundException($"Map key '{key}' not found.");
            }
        }
    }
}

public record BossLocationInfo
{
    [JsonPropertyName("enable")] public bool Enable { get; set; }
    [JsonPropertyName("addExtraSpawns")] public bool? AddExtraSpawns { get; set; }
    [JsonPropertyName("disableVanillaSpawns")] public bool? DisableVanillaSpawns { get; set; }
    [JsonPropertyName("time")] public long Time { get; set; }
    [JsonPropertyName("spawnChance")] public ValidLocationInt SpawnChance { get; set; }
    [JsonPropertyName("bossZone")] public ValidLocationString BossZone { get; set; }
}

public class BossConfig
{
    [JsonPropertyName("bossKnight")] public required BossLocationInfo BossKnight { get; set; }
    [JsonPropertyName("bossBully")] public required BossLocationInfo BossBully { get; set; }
    [JsonPropertyName("bossTagilla")] public required BossLocationInfo BossTagilla { get; set; }
    [JsonPropertyName("bossKilla")] public required BossLocationInfo BossKilla { get; set; }
    [JsonPropertyName("bossZryachiy")] public required BossLocationInfo BossZryachiy { get; set; }
    [JsonPropertyName("bossGluhar")] public required BossLocationInfo BossGluhar { get; set; }
    [JsonPropertyName("bossSanitar")] public required BossLocationInfo BossSanitar { get; set; }
    [JsonPropertyName("bossKolontay")] public required BossLocationInfo BossKolontay { get; set; }
    [JsonPropertyName("bossBoar")] public required BossLocationInfo BossBoar { get; set; }
    [JsonPropertyName("bossKojaniy")] public required BossLocationInfo BossKojaniy { get; set; }
    [JsonPropertyName("bossTagillaAgro")] public required BossLocationInfo BossTagillaAgro { get; set; }
    [JsonPropertyName("bossKillaAgro")] public required BossLocationInfo BossKillaAgro { get; set; }
    [JsonPropertyName("tagillaHelperAgro")] public required BossLocationInfo TagillaHelperAgro { get; set; }
    [JsonPropertyName("bossPartisan")] public required BossLocationInfo BossPartisan { get; set; }
    [JsonPropertyName("sectantPriest")] public required BossLocationInfo SectantPriest { get; set; }
    [JsonPropertyName("arenaFighterEvent")] public required BossLocationInfo ArenaFighterEvent { get; set; }
    [JsonPropertyName("pmcBot")] public required BossLocationInfo PmcBot { get; set; }
    [JsonPropertyName("exUsec")] public required BossLocationInfo ExUsec { get; set; }
    [JsonPropertyName("gifter")] public required BossLocationInfo Gifter { get; set; }
    
    public BossLocationInfo this[string key]
    {
        get => key.ToLowerInvariant() switch
        {
            "bossknight" => BossKnight,
            "bossbully" => BossBully,
            "bosstagilla" => BossTagilla,
            "bosskilla" => BossKilla,
            "bosszryachiy" => BossZryachiy,
            "bossgluhar" => BossGluhar,
            "bosssanitar" => BossSanitar,
            "bosskolontay" => BossKolontay,
            "bossboar" => BossBoar,
            "bosskojaniy" => BossKojaniy,
            "bossTagillaAgro" => BossTagillaAgro,
            "bossKillaAgro" => BossKillaAgro,
            "tagillaHelperAgro" => TagillaHelperAgro,
            "bosspartisan" => BossPartisan,
            "sectantpriest" => SectantPriest,
            "arenafighterevent" => ArenaFighterEvent,
            "pmcbot" => PmcBot,
            "exusec" => ExUsec,
            "gifter" => Gifter,
            _ => throw new KeyNotFoundException($"Boss key '{key}' not found.")
        };
        set
        {
            switch (key.ToLowerInvariant())
            {
                case "bossknight": BossKnight = value; break;
                case "bossbully": BossBully = value; break;
                case "bosstagilla": BossTagilla = value; break;
                case "bosskilla": BossKilla = value; break;
                case "bosszryachiy": BossZryachiy = value; break;
                case "bossgluhar": BossGluhar = value; break;
                case "bosssanitar": BossSanitar = value; break;
                case "bosskolontay": BossKolontay = value; break;
                case "bossboar": BossBoar = value; break;
                case "bosskojaniy": BossKojaniy = value; break;
                case "bossTagillaAgro": BossTagillaAgro = value; break;
                case "bossKillaAgro": BossKillaAgro = value; break;
                case "tagillaHelperAgro": TagillaHelperAgro = value; break;
                case "bosspartisan": BossPartisan = value; break;
                case "sectantpriest": SectantPriest = value; break;
                case "arenafighterevent": ArenaFighterEvent = value; break;
                case "pmcbot": PmcBot = value; break;
                case "exusec": ExUsec = value; break;
                case "gifter": Gifter = value; break;
                default: throw new KeyNotFoundException($"Boss key '{key}' not found.");
            }
        }
    }
    
    public IEnumerator<KeyValuePair<string, BossLocationInfo>> GetEnumerator()
    {
        yield return new("bossKnight", BossKnight);
        yield return new("bossBully", BossBully);
        yield return new("bossTagilla", BossTagilla);
        yield return new("bossKilla", BossKilla);
        yield return new("bossZryachiy", BossZryachiy);
        yield return new("bossGluhar", BossGluhar);
        yield return new("bossSanitar", BossSanitar);
        yield return new("bossKolontay", BossKolontay);
        yield return new("bossBoar", BossBoar);
        yield return new("bossKojaniy", BossKojaniy);
        yield return new("bossTagillaAgro", BossTagillaAgro);
        yield return new("bossKillaAgro", BossKillaAgro);
        yield return new("tagillaHelperAgro", TagillaHelperAgro);
        yield return new("bossPartisan", BossPartisan);
        yield return new("sectantPriest", SectantPriest);
        yield return new("arenaFighterEvent", ArenaFighterEvent);
        yield return new("pmcBot", PmcBot);
        yield return new("exUsec", ExUsec);
        yield return new("gifter", Gifter);
    }
}

public record WaveConfig
{
    [JsonPropertyName("enable")] public bool Enable { get; set; }
    [JsonPropertyName("ignoreMaxBotCaps")] public bool IgnoreMaxBotCaps { get; set; }
    [JsonPropertyName("groupChance")] public int GroupChance { get; set; }
    [JsonPropertyName("maxGroupSize")] public int MaxGroupSize { get; set; }
    [JsonPropertyName("maxGroupCount")] public int MaxGroupCount { get; set; }
    [JsonPropertyName("maxBotsPerWave")] public int MaxBotsPerWave { get; set; }

    [JsonPropertyName("delayBeforeFirstWave")] public int DelayBeforeFirstWave { get; set; }

    [JsonPropertyName("secondsBetweenWaves")] public int SecondsBetweenWaves { get; set; }

    [JsonPropertyName("stopWavesBeforeEndOfRaidLimit")] public int StopWavesBeforeEndOfRaidLimit { get; set; }
}

public record PMCStartingConfig
{
    [JsonPropertyName("enable")] public bool Enable { get; set; }
    [JsonPropertyName("ignoreMaxBotCaps")] public bool IgnoreMaxBotCaps { get; set; }
    [JsonPropertyName("groupChance")] public int GroupChance { get; set; }
    [JsonPropertyName("maxGroupSize")] public int MaxGroupSize { get; set; }
    [JsonPropertyName("maxGroupCount")] public int MaxGroupCount { get; set; }
    [JsonPropertyName("mapLimits")] public ValidLocationsMinMax MapLimits { get; set; }
}

public record ScavWaveConfig
{
    [JsonPropertyName("enable")] public bool Enable { get; set; }
    [JsonPropertyName("enableCustomTimers")] public bool EnableCustomTimers { get; set; }
    [JsonPropertyName("allowScavsOnLaboratory")] public bool AllowScavsOnLaboratory { get; set; }
    [JsonPropertyName("allowScavsOnLabyrinth")] public bool AllowScavsOnLabyrinth { get; set; }
    [JsonPropertyName("startSpawns")] public int StartSpawns { get; set; }
    [JsonPropertyName("stopSpawns")] public int StopSpawns { get; set; }
    [JsonPropertyName("activeTimeMin")] public int ActiveTimeMin { get; set; }
    [JsonPropertyName("activeTimeMax")] public int ActiveTimeMax { get; set; }
    [JsonPropertyName("quietTimeMin")] public int QuietTimeMin { get; set; }
    [JsonPropertyName("quietTimeMax")] public int QuietTimeMax { get; set; }
    [JsonPropertyName("checkToSpawnTimer")] public int CheckToSpawnTimer { get; set; }
    [JsonPropertyName("pendingBotsToTrigger")] public int PendingBotsToTrigger { get; set; }
}

public record ScavStartingConfig
{
    [JsonPropertyName("enable")] public bool Enable { get; set; }
    [JsonPropertyName("maxBotSpawns")] public ValidLocationInt MaxBotSpawns { get; set; }
    [JsonPropertyName("startingMarksman")] public bool StartingMarksman { get; set; }
}

public record ScavConfig
{
    [JsonPropertyName("startingScavs")] public ScavStartingConfig StartingScavs { get; set; }
    [JsonPropertyName("waves")] public ScavWaveConfig Waves { get; set; }
}

public record PMCConfig
{
    [JsonPropertyName("startingPMCs")] public PMCStartingConfig StartingPMCs { get; set; }
    [JsonPropertyName("waves")] public WaveConfig Waves { get; set; }
}

public class BossWaveDefaults : Dictionary<string, List<BossLocationSpawn>>
{
}

public record HostilityDefaults : AdditionalHostilitySettings
{
}

public record PmcDefaults
{
    [JsonPropertyName("pmcUSEC")]
    public List<BossLocationSpawn> PmcUSEC { get; set; }
    [JsonPropertyName("pmcBEAR")]
    public List<BossLocationSpawn> PmcBEAR { get; set; }
}

public record ScavDefaults : Wave
{
}

public record MapZoneDefaults
{
    public List<string> CustomsSpawnZones { get; set; }
    public List<string> CustomsSnipeSpawnZones { get; set; }
    public List<string> FactorySpawnZones { get; set; }
    public List<string> InterchangeSpawnZones { get; set; }
    public List<string> LabsGateSpawnZones { get; set; }
    public List<string> LabsNonGateSpawnZones { get; set; }
    public List<string> LighthouseNonWaterTreatmentSpawnZones { get; set; }
    public List<string> LighthouseWaterTreatmentSpawnZones { get; set; }
    public List<string> LighthouseSnipeSpawnZones { get; set; }
    public List<string> ReserveSpawnZones { get; set; }
    public List<string> GroundZeroSpawnZones { get; set; }
    public List<string> GroundZeroSnipeSpawnZones { get; set; }
    public List<string> ShorelineSpawnZones { get; set; }
    public List<string> ShorelineSnipeSpawnZones { get; set; }
    public List<string> StreetsSpawnZones { get; set; }
    public List<string> StreetsSnipeSpawnZones { get; set; }
    public List<string> WoodsSpawnZones { get; set; }
    public List<string> WoodsSnipeSpawnZones { get; set; }
    public List<string> LabyrinthSpawnZones { get; set; }
}