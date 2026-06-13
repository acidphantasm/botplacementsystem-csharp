using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;

namespace BotPlacementSystemServer.Models;

public record AbpsConfig
{
    // export type DifficultyConfig = "easy" | "normal" | "hard" | "impossible";
    [JsonPropertyName("pmcDifficulty")] public Dictionary<string, double> PmcDifficulty { get; set; } = new()
    {
        { "easy", 10 },
        { "normal", 50 },
        { "hard", 30 },
        { "impossible", 10 }
    };
    [JsonPropertyName("pmcType")] public PmcTypeChance PmcType { get; set; } = new();
    [JsonPropertyName("pmcConfig")] public PMCConfig PmcConfig { get; set; } = new()
    {
        StartingPMCs = new PMCStartingConfig
        {
            Enable = true,
            IgnoreMaxBotCaps = true,
            GroupChance = 25,
            MaxGroupSize = 3,
            MaxGroupCount = 4,
            MapLimits = new ValidLocationsMinMax
            {
                Customs = new MinMax<int> { Min = 8, Max = 10 },
                Factory4Day = new MinMax<int> { Min = 5, Max = 7 },
                Factory4Night = new MinMax<int> { Min = 5, Max = 7 },
                Interchange = new MinMax<int> { Min = 9, Max = 13 },
                Laboratory = new MinMax<int> { Min = 7, Max = 9 },
                Lighthouse = new MinMax<int> { Min = 7, Max = 10 },
                Reserve = new MinMax<int> { Min = 8, Max = 10 },
                GroundZero = new MinMax<int> { Min = 8, Max = 11 },
                GroundZeroHigh = new MinMax<int> { Min = 8, Max = 11 },
                Shoreline = new MinMax<int> { Min = 9, Max = 13 },
                TarkovStreets = new MinMax<int> { Min = 7, Max = 10 },
                Woods = new MinMax<int> { Min = 9, Max = 13 },
                Labyrinth = new MinMax<int> { Min = 0, Max = 0 }
            }
        },
        Waves = new WaveConfig
        {
            Enable = false,
            AllowPmcsOnLabyrinth = false,
            IgnoreMaxBotCaps = false,
            GroupChance = 10,
            MaxGroupSize = 1,
            MaxGroupCount = 3,
            MaxBotsPerWave = 4,
            DelayBeforeFirstWave = 500,
            SecondsBetweenWaves = 360,
            StopWavesBeforeEndOfRaidLimit = 300
        }
    };
    [JsonPropertyName("scavConfig")] public ScavConfig ScavConfig { get; set; } = new()
    {
        StartingScavs = new ScavStartingConfig
        {
            Enable = true,
            MaxBotSpawns = new ValidLocationInt
            {
                Customs = 5,
                Factory4Day = 3,
                Factory4Night = 3,
                Interchange = 5,
                Laboratory = 0,
                Lighthouse = 5,
                Reserve = 5,
                GroundZero = 5,
                GroundZeroHigh = 5,
                Shoreline = 5,
                TarkovStreets = 5,
                Woods = 5,
                Labyrinth = 0
            },
            StartingMarksman = true
        },
        Waves = new ScavWaveConfig
        {
            Enable = true,
            EnableCustomTimers = true,
            AllowScavsOnLaboratory = false,
            AllowScavsOnLabyrinth = false,
            StartSpawns = 60,
            StopSpawns = 600,
            ActiveTimeMin = 180,
            ActiveTimeMax = 240,
            QuietTimeMin = 120,
            QuietTimeMax = 180,
            CheckToSpawnTimer = 15,
            PendingBotsToTrigger = 1,
            NonWaveSpawnBotsLimitPerPlayer = 20
        }
    };
    [JsonPropertyName("bossDifficulty")] public Dictionary<string, double> BossDifficulty { get; set; } = new()
    {
        { "easy", 0 },
        { "normal", 60 },
        { "hard", 30 },
        { "impossible", 10 }
    };
    [JsonPropertyName("weeklyBoss")] public WeeklyBossConfig WeeklyBoss { get; set; } = new() { Enable = false };
    [JsonPropertyName("bossConfig")] public BossConfig BossConfig { get; set; } = new()
    {
        BossKnight = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 30, Factory4Day = 0, Factory4Night = 0, Interchange = 0, Laboratory = 0,
                Lighthouse = 30, Reserve = 0, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 30,
                TarkovStreets = 0, Woods = 30, Labyrinth = 0
            },
            BossZone = new ValidLocationString
            {
                Customs = "ZoneScavBase", Factory4Day = "", Factory4Night = "", Interchange = "",
                Laboratory = "", Lighthouse = "Zone_TreatmentContainers,Zone_Chalet", Reserve = "",
                GroundZero = "", GroundZeroHigh = "", Shoreline = "ZoneMeteoStation",
                TarkovStreets = "", Woods = "ZoneScavBase2", Labyrinth = ""
            }
        },
        BossBully = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 30, Factory4Day = 0, Factory4Night = 0, Interchange = 0, Laboratory = 0,
                Lighthouse = 0, Reserve = 0, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 0,
                TarkovStreets = 0, Woods = 0, Labyrinth = 0
            },
            BossZone = new ValidLocationString
            {
                Customs = "ZoneDormitory,ZoneGasStation,ZoneScavBase", Factory4Day = "", Factory4Night = "",
                Interchange = "", Laboratory = "", Lighthouse = "", Reserve = "", GroundZero = "",
                GroundZeroHigh = "", Shoreline = "", TarkovStreets = "", Woods = "", Labyrinth = ""
            }
        },
        BossTagilla = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 0, Factory4Day = 30, Factory4Night = 30, Interchange = 0, Laboratory = 0,
                Lighthouse = 0, Reserve = 0, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 0,
                TarkovStreets = 0, Woods = 0, Labyrinth = 0
            },
            BossZone = new ValidLocationString
            {
                Customs = "", Factory4Day = "BotZone", Factory4Night = "BotZone", Interchange = "",
                Laboratory = "", Lighthouse = "", Reserve = "", GroundZero = "", GroundZeroHigh = "",
                Shoreline = "", TarkovStreets = "", Woods = "", Labyrinth = ""
            }
        },
        BossKilla = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 0, Factory4Day = 0, Factory4Night = 0, Interchange = 30, Laboratory = 0,
                Lighthouse = 0, Reserve = 0, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 0,
                TarkovStreets = 0, Woods = 0, Labyrinth = 0
            },
            BossZone = new ValidLocationString
            {
                Customs = "", Factory4Day = "", Factory4Night = "",
                Interchange = "ZoneCenterBot,ZoneCenter,ZoneOLI,ZoneIDEA,ZoneGoshan",
                Laboratory = "", Lighthouse = "", Reserve = "", GroundZero = "", GroundZeroHigh = "",
                Shoreline = "", TarkovStreets = "", Woods = "", Labyrinth = ""
            }
        },
        BossZryachiy = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 0, Factory4Day = 0, Factory4Night = 0, Interchange = 0, Laboratory = 0,
                Lighthouse = 100, Reserve = 0, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 0,
                TarkovStreets = 0, Woods = 0, Labyrinth = 0
            },
            BossZone = new ValidLocationString
            {
                Customs = "", Factory4Day = "", Factory4Night = "", Interchange = "", Laboratory = "",
                Lighthouse = "Zone_Island", Reserve = "", GroundZero = "", GroundZeroHigh = "",
                Shoreline = "", TarkovStreets = "", Woods = "", Labyrinth = ""
            }
        },
        BossGluhar = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 0, Factory4Day = 0, Factory4Night = 0, Interchange = 0, Laboratory = 0,
                Lighthouse = 0, Reserve = 30, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 0,
                TarkovStreets = 0, Woods = 0, Labyrinth = 0
            },
            BossZone = new ValidLocationString
            {
                Customs = "", Factory4Day = "", Factory4Night = "", Interchange = "", Laboratory = "",
                Lighthouse = "", Reserve = "ZoneRailStrorage,ZonePTOR2,ZoneBarrack,ZoneSubStorage",
                GroundZero = "", GroundZeroHigh = "", Shoreline = "", TarkovStreets = "", Woods = "",
                Labyrinth = ""
            }
        },
        BossSanitar = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 0, Factory4Day = 0, Factory4Night = 0, Interchange = 0, Laboratory = 0,
                Lighthouse = 0, Reserve = 0, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 30,
                TarkovStreets = 0, Woods = 0, Labyrinth = 0
            },
            BossZone = new ValidLocationString
            {
                Customs = "", Factory4Day = "", Factory4Night = "", Interchange = "", Laboratory = "",
                Lighthouse = "", Reserve = "", GroundZero = "", GroundZeroHigh = "",
                Shoreline = "ZoneGreenHouses,ZoneSanatorium1,ZoneSanatorium2,ZonePort",
                TarkovStreets = "", Woods = "", Labyrinth = ""
            }
        },
        BossKolontay = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 0, Factory4Day = 0, Factory4Night = 0, Interchange = 0, Laboratory = 0,
                Lighthouse = 0, Reserve = 0, GroundZero = 0, GroundZeroHigh = 30, Shoreline = 0,
                TarkovStreets = 30, Woods = 0, Labyrinth = 0
            },
            BossZone = new ValidLocationString
            {
                Customs = "", Factory4Day = "", Factory4Night = "", Interchange = "", Laboratory = "",
                Lighthouse = "", Reserve = "", GroundZero = "", GroundZeroHigh = "ZoneSandbox",
                Shoreline = "", TarkovStreets = "ZoneClimova,ZoneMvd", Woods = "", Labyrinth = ""
            }
        },
        BossBoar = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 0, Factory4Day = 0, Factory4Night = 0, Interchange = 0, Laboratory = 0,
                Lighthouse = 0, Reserve = 0, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 0,
                TarkovStreets = 30, Woods = 0, Labyrinth = 0
            },
            BossZone = new ValidLocationString
            {
                Customs = "", Factory4Day = "", Factory4Night = "", Interchange = "", Laboratory = "",
                Lighthouse = "", Reserve = "", GroundZero = "", GroundZeroHigh = "", Shoreline = "",
                TarkovStreets = "ZoneCarShowroom", Woods = "", Labyrinth = ""
            }
        },
        BossKojaniy = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 0, Factory4Day = 0, Factory4Night = 0, Interchange = 0, Laboratory = 0,
                Lighthouse = 0, Reserve = 0, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 0,
                TarkovStreets = 0, Woods = 30, Labyrinth = 0
            },
            BossZone = new ValidLocationString
            {
                Customs = "", Factory4Day = "", Factory4Night = "", Interchange = "", Laboratory = "",
                Lighthouse = "", Reserve = "", GroundZero = "", GroundZeroHigh = "", Shoreline = "",
                TarkovStreets = "", Woods = "ZoneWoodCutter", Labyrinth = ""
            }
        },
        BossTagillaAgro = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 0, Factory4Day = 0, Factory4Night = 0, Interchange = 0, Laboratory = 0,
                Lighthouse = 0, Reserve = 0, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 0,
                TarkovStreets = 0, Woods = 0, Labyrinth = 100
            },
            BossZone = new ValidLocationString
            {
                Customs = "", Factory4Day = "", Factory4Night = "", Interchange = "", Laboratory = "",
                Lighthouse = "", Reserve = "", GroundZero = "", GroundZeroHigh = "", Shoreline = "",
                TarkovStreets = "", Woods = "", Labyrinth = ""
            }
        },
        BossKillaAgro = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 0, Factory4Day = 0, Factory4Night = 0, Interchange = 0, Laboratory = 0,
                Lighthouse = 0, Reserve = 0, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 0,
                TarkovStreets = 0, Woods = 0, Labyrinth = 100
            },
            BossZone = new ValidLocationString
            {
                Customs = "", Factory4Day = "", Factory4Night = "", Interchange = "", Laboratory = "",
                Lighthouse = "", Reserve = "", GroundZero = "", GroundZeroHigh = "", Shoreline = "",
                TarkovStreets = "", Woods = "", Labyrinth = ""
            }
        },
        TagillaHelperAgro = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            AddExtraSpawns = false,
            DisableVanillaSpawns = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 0, Factory4Day = 0, Factory4Night = 0, Interchange = 0, Laboratory = 0,
                Lighthouse = 0, Reserve = 0, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 0,
                TarkovStreets = 0, Woods = 0, Labyrinth = 100
            },
            BossZone = new ValidLocationString
            {
                Customs = "", Factory4Day = "", Factory4Night = "", Interchange = "", Laboratory = "",
                Lighthouse = "", Reserve = "", GroundZero = "", GroundZeroHigh = "", Shoreline = "",
                TarkovStreets = "", Woods = "", Labyrinth = ""
            }
        },
        BossPartisan = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 30, Factory4Day = 0, Factory4Night = 0, Interchange = 0, Laboratory = 0,
                Lighthouse = 30, Reserve = 0, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 30,
                TarkovStreets = 0, Woods = 30, Labyrinth = 0
            },
            BossZone = new ValidLocationString
            {
                Customs = "", Factory4Day = "", Factory4Night = "", Interchange = "", Laboratory = "",
                Lighthouse = "", Reserve = "", GroundZero = "", GroundZeroHigh = "", Shoreline = "",
                TarkovStreets = "", Woods = "", Labyrinth = ""
            }
        },
        SectantPriest = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 15, Factory4Day = 0, Factory4Night = 20, Interchange = 0, Laboratory = 0,
                Lighthouse = 0, Reserve = 0, GroundZero = 0, GroundZeroHigh = 44, Shoreline = 15,
                TarkovStreets = 0, Woods = 15, Labyrinth = 0
            },
            BossZone = new ValidLocationString
            {
                Customs = "ZoneScavBase", Factory4Day = "", Factory4Night = "BotZone", Interchange = "",
                Laboratory = "", Lighthouse = "", Reserve = "", GroundZero = "",
                GroundZeroHigh = "ZoneSandbox", Shoreline = "ZoneSanatorium1,ZoneSanatorium2,ZoneForestSpawn",
                TarkovStreets = "", Woods = "ZoneMiniHouse,ZoneBrokenVill", Labyrinth = ""
            }
        },
        ArenaFighterEvent = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 5, Factory4Day = 0, Factory4Night = 0, Interchange = 0, Laboratory = 0,
                Lighthouse = 0, Reserve = 0, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 0,
                TarkovStreets = 0, Woods = 5, Labyrinth = 0
            },
            BossZone = new ValidLocationString
            {
                Customs = "ZoneFactoryCenter,ZoneScavBase", Factory4Day = "", Factory4Night = "",
                Interchange = "", Laboratory = "", Lighthouse = "", Reserve = "", GroundZero = "",
                GroundZeroHigh = "", Shoreline = "", TarkovStreets = "",
                Woods = "ZoneMiniHouse,ZoneClearVill,ZoneRoad,ZoneBrokenVill,ZoneScavBase2", Labyrinth = ""
            }
        },
        PmcBot = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            AddExtraSpawns = true,
            DisableVanillaSpawns = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 0, Factory4Day = 0, Factory4Night = 0, Interchange = 0, Laboratory = 40,
                Lighthouse = 0, Reserve = 0, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 0,
                TarkovStreets = 0, Woods = 0, Labyrinth = 0
            },
            BossZone = new ValidLocationString
            {
                Customs = "", Factory4Day = "", Factory4Night = "", Interchange = "",
                Laboratory = "BotZoneBasement,BotZoneFloor1,BotZoneFloor2", Lighthouse = "", Reserve = "",
                GroundZero = "", GroundZeroHigh = "", Shoreline = "", TarkovStreets = "", Woods = "",
                Labyrinth = ""
            }
        },
        ExUsec = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            AddExtraSpawns = false,
            DisableVanillaSpawns = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 0, Factory4Day = 0, Factory4Night = 0, Interchange = 0, Laboratory = 0,
                Lighthouse = 0, Reserve = 0, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 0,
                TarkovStreets = 0, Woods = 0, Labyrinth = 0
            },
            BossZone = new ValidLocationString
            {
                Customs = "", Factory4Day = "", Factory4Night = "", Interchange = "", Laboratory = "",
                Lighthouse = "", Reserve = "", GroundZero = "", GroundZeroHigh = "", Shoreline = "",
                TarkovStreets = "", Woods = "", Labyrinth = ""
            }
        },
        Gifter = new BossLocationInfo
        {
            Enable = true,
            DisableFollowers = false,
            Time = -1,
            SpawnChance = new ValidLocationInt
            {
                Customs = 0, Factory4Day = 0, Factory4Night = 0, Interchange = 0, Laboratory = 0,
                Lighthouse = 0, Reserve = 0, GroundZero = 0, GroundZeroHigh = 0, Shoreline = 0,
                TarkovStreets = 0, Woods = 0, Labyrinth = 0
            },
            BossZone = new ValidLocationString
            {
                Customs = "", Factory4Day = "", Factory4Night = "", Interchange = "", Laboratory = "",
                Lighthouse = "", Reserve = "", GroundZero = "", GroundZeroHigh = "", Shoreline = "",
                TarkovStreets = "", Woods = "", Labyrinth = ""
            }
        }
    };

    [JsonPropertyName("configAppSettings")]
    public ConfigAppSettings ConfigAppSettings { get; set; } = new()
    {
        ShowUndo = true,
        ShowDefault = false,
        DisableAnimations = false,
        AllowUpdateChecks = false,
        RequireAuthCode = false,
        AuthCode = "Kitten Mittons"
    };
}

public record PmcTypeChance
{
    [JsonPropertyName("usecChance")] public double UsecChance { get; set; } = 50;
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
    [JsonPropertyName("requireAuthCode")] public bool RequireAuthCode { get; set; }
    [JsonPropertyName("authCode")] public string? AuthCode { get; set; }
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
    [JsonPropertyName("bigmap")] public string Customs { get; set; }
    [JsonPropertyName("factory4_day")] public string Factory4Day { get; set; }
    [JsonPropertyName("factory4_night")] public string Factory4Night { get; set; }
    [JsonPropertyName("interchange")] public string Interchange { get; set; }
    [JsonPropertyName("laboratory")] public string Laboratory { get; set; }
    [JsonPropertyName("lighthouse")] public string Lighthouse { get; set; }
    [JsonPropertyName("rezervbase")] public string Reserve { get; set; }
    [JsonPropertyName("sandbox")] public string GroundZero { get; set; }
    [JsonPropertyName("sandbox_high")]public string GroundZeroHigh { get; set; }
    [JsonPropertyName("shoreline")] public string Shoreline { get; set; }
    [JsonPropertyName("tarkovstreets")] public string TarkovStreets { get; set; }
    [JsonPropertyName("woods")] public string Woods { get; set; }
    [JsonPropertyName("labyrinth")] public string Labyrinth { get; set; }

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
    [JsonPropertyName("disableFollowers")] public bool DisableFollowers { get; set; }
    [JsonPropertyName("addExtraSpawns")] public bool? AddExtraSpawns { get; set; }
    [JsonPropertyName("disableVanillaSpawns")] public bool? DisableVanillaSpawns { get; set; }
    [JsonPropertyName("time")] public long Time { get; set; }
    [JsonPropertyName("spawnChance")] public ValidLocationInt SpawnChance { get; set; }
    [JsonPropertyName("bossZone")] public ValidLocationString BossZone { get; set; }
}

public class BossConfig
{
    [JsonPropertyName("bossKnight")] public BossLocationInfo BossKnight { get; set; }
    [JsonPropertyName("bossBully")] public BossLocationInfo BossBully { get; set; }
    [JsonPropertyName("bossTagilla")] public BossLocationInfo BossTagilla { get; set; }
    [JsonPropertyName("bossKilla")] public BossLocationInfo BossKilla { get; set; }
    [JsonPropertyName("bossZryachiy")] public BossLocationInfo BossZryachiy { get; set; }
    [JsonPropertyName("bossGluhar")] public BossLocationInfo BossGluhar { get; set; }
    [JsonPropertyName("bossSanitar")] public BossLocationInfo BossSanitar { get; set; }
    [JsonPropertyName("bossKolontay")] public BossLocationInfo BossKolontay { get; set; }
    [JsonPropertyName("bossBoar")] public BossLocationInfo BossBoar { get; set; }
    [JsonPropertyName("bossKojaniy")] public BossLocationInfo BossKojaniy { get; set; }
    [JsonPropertyName("bossTagillaAgro")] public BossLocationInfo BossTagillaAgro { get; set; }
    [JsonPropertyName("bossKillaAgro")] public BossLocationInfo BossKillaAgro { get; set; }
    [JsonPropertyName("tagillaHelperAgro")] public BossLocationInfo TagillaHelperAgro { get; set; }
    [JsonPropertyName("bossPartisan")] public BossLocationInfo BossPartisan { get; set; }
    [JsonPropertyName("sectantPriest")] public BossLocationInfo SectantPriest { get; set; }
    [JsonPropertyName("arenaFighterEvent")] public BossLocationInfo ArenaFighterEvent { get; set; }
    [JsonPropertyName("pmcBot")] public BossLocationInfo PmcBot { get; set; }
    [JsonPropertyName("exUsec")] public BossLocationInfo ExUsec { get; set; }
    [JsonPropertyName("gifter")] public BossLocationInfo Gifter { get; set; }
    
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
    
    [JsonPropertyName("allowPmcsOnLabyrinth")] public bool AllowPmcsOnLabyrinth { get; set; }
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
    [JsonPropertyName("nonWaveSpawnBotsLimitPerPlayer")] public int NonWaveSpawnBotsLimitPerPlayer { get; set; }
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
    public required List<BossLocationSpawn> PmcUSEC { get; set; }
    [JsonPropertyName("pmcBEAR")]
    public required List<BossLocationSpawn> PmcBEAR { get; set; }
}

public record ScavDefaults : Wave
{
}

public record MapZoneDefaults
{
    public required List<string> CustomsSpawnZones { get; set; }
    public required List<string> CustomsSnipeSpawnZones { get; set; }
    public required List<string> FactorySpawnZones { get; set; }
    public required List<string> InterchangeSpawnZones { get; set; }
    public required List<string> LabsGateSpawnZones { get; set; }
    public required List<string> LabsNonGateSpawnZones { get; set; }
    public required List<string> LighthouseNonWaterTreatmentSpawnZones { get; set; }
    public required List<string> LighthouseWaterTreatmentSpawnZones { get; set; }
    public required List<string> LighthouseSnipeSpawnZones { get; set; }
    public required List<string> ReserveSpawnZones { get; set; }
    public required List<string> GroundZeroSpawnZones { get; set; }
    public required List<string> GroundZeroSnipeSpawnZones { get; set; }
    public required List<string> ShorelineSpawnZones { get; set; }
    public required List<string> ShorelineSnipeSpawnZones { get; set; }
    public required List<string> StreetsSpawnZones { get; set; }
    public required List<string> StreetsSnipeSpawnZones { get; set; }
    public required List<string> WoodsSpawnZones { get; set; }
    public required List<string> WoodsSnipeSpawnZones { get; set; }
    public required List<string> LabyrinthSpawnZones { get; set; }
}