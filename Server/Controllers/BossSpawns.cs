using _botplacementsystem.Globals;
using _botplacementsystem.Models;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace _botplacementsystem.Controllers;

[Injectable]
public class BossSpawns
{
    private ISptLogger<BossSpawns> _logger;
    private RandomUtil _randomUtil;
    private ICloner _cloner;
    private WeightedRandomHelper _weightedRandomHelper;
    private readonly BotConfig _botConfig;

    public BossSpawns
    (
        ISptLogger<BossSpawns> logger,
        ICloner cloner,
        WeightedRandomHelper weightedRandomHelper,
        RandomUtil randomUtil,
        ConfigServer configServer
    )
    {
        _logger = logger;
        _cloner = cloner;
        _weightedRandomHelper = weightedRandomHelper;
        _randomUtil = randomUtil;
        _botConfig = configServer.GetConfig<BotConfig>();
    }

    public List<BossLocationSpawn> GetCustomMapData(string location, double escapeTimeLimit)
    {
        return GetConfigValueForLocation(location, escapeTimeLimit);
    }

    private List<BossLocationSpawn> GetConfigValueForLocation(string location, double escapeTimeLimit)
    {
        var bossesForMap = new List<BossLocationSpawn>();

        foreach (var (boss, bossData) in ModConfig.Config.BossConfig)
        {
            var bossDefaultData = _cloner.Clone(GetDefaultValuesForBoss(boss, location));
            var difficultyWeights = ModConfig.Config.BossDifficulty;

            if (!bossData.Enable) continue;

            if (boss == "exUsec" && !(bossData.DisableVanillaSpawns ?? false) && location == "lighthouse" ||
                boss == "pmcBot" && !(bossData.DisableVanillaSpawns ?? false) && (location == "laboratory" || location == "rezervbase") ||
                boss == "tagillaHelperAgro" && !(bossData.DisableVanillaSpawns ?? false) && location == "labyrinth")
            {
                foreach (var bossSpawn in bossDefaultData)
                {
                    bossDefaultData[0].BossDifficulty = _weightedRandomHelper.GetWeightedValue(difficultyWeights);
                    bossesForMap.Add(bossSpawn);
                }
                if (!(bossData.AddExtraSpawns ?? false)) continue;
            }

            if (bossData.SpawnChance[location] == 0) continue;

            if (location.Contains("factory")) bossData.BossZone[location] = "BotZone";
            if (location.Contains("labyrinth")) bossData.BossZone[location] = "";
            if ((boss == "pmcBot") && (bossData.AddExtraSpawns ?? false))
            {
                bossesForMap.AddRange(GenerateBossWaves(location, escapeTimeLimit));
                continue;
            }

            if (!Enum.TryParse<WildSpawnType>(boss, ignoreCase: true, out var bossType))
            {
                _logger.Warning($"Boss: {boss} is not a valid WildSpawnType. Report this.");
                bossDefaultData[0].BossChance = bossData.SpawnChance[location];
            }
            else
            {
                if (ModConfig.Config.WeeklyBoss.Enable)
                {
                    var isWeeklyBoss = IsWeeklyBoss(bossType);
                    if (isWeeklyBoss)
                    {
                        _logger.Warning($"Weekly Boss: {boss} | 100% Chance on {location}");
                        bossDefaultData[0].ShowOnTarkovMap = true;
                        bossDefaultData[0].ShowOnTarkovMapPvE = true;
                        bossDefaultData[0].BossChance = 100;
                    }
                    else bossDefaultData[0].BossChance = bossData.SpawnChance[location];
                }
                else bossDefaultData[0].BossChance = bossData.SpawnChance[location];
            }
            bossDefaultData[0].BossZone = (string?)bossData.BossZone[location];
            bossDefaultData[0].BossDifficulty = _weightedRandomHelper.GetWeightedValue(difficultyWeights);
            bossDefaultData[0].BossEscortDifficulty = _weightedRandomHelper.GetWeightedValue(difficultyWeights);
            bossDefaultData[0].Time = bossData.Time;
            bossesForMap.Add(bossDefaultData[0]);
        }

        return bossesForMap;
    }

    private bool IsWeeklyBoss(WildSpawnType bossType)
    {
        var bossList = _botConfig.WeeklyBoss.BossPool;
        var startOfWeek = DateTime.Today.GetMostRecentPreviousDay(DayOfWeek.Monday);

        var seed = startOfWeek.Year * 1000 + startOfWeek.DayOfYear;
        var random =  new Random(seed);

        var boss = bossList[random.Next(0, bossList.Count)];

        return boss == bossType;
    }

    private List<BossLocationSpawn> GenerateBossWaves(string location, double escapeTimeLimit)
    {
        var pmcWaveSpawnInfo = new List<BossLocationSpawn>();

        var difficultyWeights = ModConfig.Config.BossDifficulty;
        var waveMaxPMCCount = location != "laboratory" ? 4 : 10;
        var waveGroupLimit = 3;
        var waveGroupSize = 2;
        var waveGroupChance = 100;
        var waveTimer = 450;
        var endWavesAtRemainingTime = 600;
        var waveCount = Math.Floor((((escapeTimeLimit * 60) - endWavesAtRemainingTime)) / waveTimer);
        var currentWaveTime = waveTimer;
        var bossConfigData = (BossLocationInfo)ModConfig.Config.BossConfig["pmcBot"];

        //this.logger.warning(`[Boss Waves] Generating ${waveCount} waves for Raiders`)
        for (var i = 1; i <= waveCount; i++)
        {
            if (i == 1) currentWaveTime = -1;

            var currentPMCCount = 0;
            var groupCount = 0;
            while (currentPMCCount < waveMaxPMCCount)
            {
                if (groupCount >= waveGroupLimit) break;
                var groupSize = 0;
                var remainingSpots = waveMaxPMCCount - currentPMCCount;
                var isAGroup = remainingSpots > 1 ? _randomUtil.GetChance100(waveGroupChance) : false;
                if (isAGroup)
                {
                    groupSize = Math.Min(remainingSpots - 1, _randomUtil.GetInt(1, waveGroupSize));
                }

                var bossDefaultData = _cloner.Clone(GetDefaultValuesForBoss("pmcBot", ""));

                bossDefaultData[0].BossChance = (double?) bossConfigData.SpawnChance[location];
                bossDefaultData[0].BossZone = (string?) bossConfigData.BossZone[location];
                bossDefaultData[0].BossEscortAmount = groupSize.ToString();
                bossDefaultData[0].BossDifficulty = _weightedRandomHelper.GetWeightedValue(difficultyWeights);
                bossDefaultData[0].BossEscortDifficulty = _weightedRandomHelper.GetWeightedValue(difficultyWeights);
                bossDefaultData[0].IgnoreMaxBots = false;
                bossDefaultData[0].Time = currentWaveTime;
                currentPMCCount += groupSize + 1;
                groupCount++;
                pmcWaveSpawnInfo.Add(bossDefaultData[0]);
            }
            
            currentWaveTime += waveTimer;
        }

        return pmcWaveSpawnInfo;
    }

    private List<BossLocationSpawn> GetDefaultValuesForBoss(string boss, string location)
    {
        switch (boss)
        {
            case "bossKnight":
                return ModConfig.BossWaveDefaults["bossKnightData"];
            case "bossBully":
                return ModConfig.BossWaveDefaults["bossBullyData"];
            case "bossTagilla":
                return ModConfig.BossWaveDefaults["bossTagillaData"];
            case "bossKilla":
                return ModConfig.BossWaveDefaults["bossKillaData"];
            case "bossZryachiy":
                return ModConfig.BossWaveDefaults["bossZryachiyData"];
            case "bossGluhar":
                return ModConfig.BossWaveDefaults["bossGluharData"];
            case "bossSanitar":
                return ModConfig.BossWaveDefaults["bossSanitarData"];
            case "bossKolontay":
                return ModConfig.BossWaveDefaults["bossKolontayData"];
            case "bossBoar":
                return ModConfig.BossWaveDefaults["bossBoarData"];
            case "bossKojaniy":
                return ModConfig.BossWaveDefaults["bossKojaniyData"];
            case "bossTagillaAgro":
                return ModConfig.BossWaveDefaults["bossTagillaAgroData"];
            case "bossKillaAgro":
                if (location == "labyrinth") return ModConfig.BossWaveDefaults["bossKillaAgroData"];
                return ModConfig.BossWaveDefaults["bossKillaAgroNonLabyData"];
            case "tagillaHelperAgro":
                if (location == "labyrinth") return ModConfig.BossWaveDefaults["tagillaHelperAgroData"];
                return ModConfig.BossWaveDefaults["tagillaHelperAgroNonLabyData"];
            case "bossPartisan":
                return ModConfig.BossWaveDefaults["bossPartisanData"];
            case "sectantPriest":
                return ModConfig.BossWaveDefaults["sectantPriestData"];
            case "arenaFighterEvent":
                return ModConfig.BossWaveDefaults["arenaFighterEventData"];
            case "pmcBot": // Requires Triggers + Has Multiple Zones
                if (location == "rezervbase") return ModConfig.BossWaveDefaults["pmcBotReserveData"];
                if (location == "laboratory") return ModConfig.BossWaveDefaults["pmcBotLaboratoryData"];
                else return ModConfig.BossWaveDefaults["pmcBotData"];
            case "exUsec": // Has Multiple Zones
                return ModConfig.BossWaveDefaults["exUsecData"];
            case "gifter":
                return ModConfig.BossWaveDefaults["gifterData"];
            default:
                _logger.Error($"[ABPS] Boss not found in config {boss}");
                return null;
        }
    }
}