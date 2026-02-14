using _botplacementsystem.Globals;
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
public class BossSpawns(
    ISptLogger<BossSpawns> logger,
    ICloner cloner,
    WeightedRandomHelper weightedRandomHelper,
    RandomUtil randomUtil,
    ConfigServer configServer)
{
    private readonly BotConfig _botConfig = configServer.GetConfig<BotConfig>();

    public List<BossLocationSpawn> GetCustomMapData(string location, double escapeTimeLimit)
    {
        return GetConfigValueForLocation(location, escapeTimeLimit);
    }

    private List<BossLocationSpawn> GetConfigValueForLocation(string location, double escapeTimeLimit)
    {
        var bossesForMap = new List<BossLocationSpawn>();

        foreach (var (boss, bossData) in ModConfig.Config.BossConfig)
        {
            var bossDefaultData = cloner.Clone(GetDefaultValuesForBoss(boss, location));
            var difficultyWeights = ModConfig.Config.BossDifficulty;

            if (!bossData.Enable) continue;
            if (bossDefaultData is null) continue;
            if (bossData.DisableFollowers)
            {
                bossDefaultData[0].BossEscortAmount = "0";
                bossDefaultData[0].BossEscortType = bossDefaultData[0].BossName;
                bossDefaultData[0].Supports = null!;
            }
            
            if (boss == "exUsec" && !(bossData.DisableVanillaSpawns ?? false) && location == "lighthouse" ||
                boss == "pmcBot" && !(bossData.DisableVanillaSpawns ?? false) && (location == "laboratory" || location == "rezervbase") ||
                boss == "tagillaHelperAgro" && !(bossData.DisableVanillaSpawns ?? false) && location == "labyrinth")
            {
                foreach (var bossSpawn in bossDefaultData)
                {
                    bossDefaultData[0].BossDifficulty = weightedRandomHelper.GetWeightedValue(difficultyWeights);
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
                logger.Warning($"Boss: {boss} is not a valid WildSpawnType. Report this.");
                bossDefaultData[0].BossChance = bossData.SpawnChance[location];
            }
            else
            {
                if (ModConfig.Config.WeeklyBoss.Enable)
                {
                    var isWeeklyBoss = IsWeeklyBoss(bossType);
                    if (isWeeklyBoss)
                    {
                        logger.Warning($"Weekly Boss: {boss} | 100% Chance on {location}");
                        bossDefaultData[0].ShowOnTarkovMap = true;
                        bossDefaultData[0].ShowOnTarkovMapPvE = true;
                        bossDefaultData[0].BossChance = 100;
                    }
                    else bossDefaultData[0].BossChance = bossData.SpawnChance[location];
                }
                else bossDefaultData[0].BossChance = bossData.SpawnChance[location];
            }

            bossDefaultData[0].BossZone = (string?)bossData.BossZone[location];
            bossDefaultData[0].BossDifficulty = weightedRandomHelper.GetWeightedValue(difficultyWeights);
            bossDefaultData[0].BossEscortDifficulty = weightedRandomHelper.GetWeightedValue(difficultyWeights);
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
        var waveMaxBotCount = location != "laboratory" ? 4 : 10;
        var waveGroupLimit = 3;
        var waveGroupSize = 2;
        var waveGroupChance = 100;
        var waveTimer = 450;
        var endWavesAtRemainingTime = 600;
        var waveCount = Math.Floor((((escapeTimeLimit * 60) - endWavesAtRemainingTime)) / waveTimer);
        var currentWaveTime = waveTimer;
        var bossConfigData = ModConfig.Config.BossConfig["pmcBot"];

        for (var i = 1; i <= waveCount; i++)
        {
            if (i == 1) currentWaveTime = -1;

            var currentBotCount = 0;
            var groupCount = 0;
            while (currentBotCount < waveMaxBotCount)
            {
                if (groupCount >= waveGroupLimit) break;
                var groupSize = 0;
                var remainingSpots = waveMaxBotCount - currentBotCount;
                var isAGroup = remainingSpots > 1 && randomUtil.GetChance100(waveGroupChance);
                if (isAGroup)
                {
                    groupSize = Math.Min(remainingSpots - 1, randomUtil.GetInt(1, waveGroupSize));
                }

                var bossDefaultData = cloner.Clone(GetDefaultValuesForBoss("pmcBot", ""));

                if (bossDefaultData is null) continue;
                
                bossDefaultData[0].BossChance = bossConfigData.SpawnChance[location];
                bossDefaultData[0].BossZone = bossConfigData.BossZone[location];
                bossDefaultData[0].BossEscortAmount = groupSize.ToString();
                bossDefaultData[0].BossDifficulty = weightedRandomHelper.GetWeightedValue(difficultyWeights);
                bossDefaultData[0].BossEscortDifficulty = weightedRandomHelper.GetWeightedValue(difficultyWeights);
                bossDefaultData[0].IgnoreMaxBots = false;
                bossDefaultData[0].Time = currentWaveTime;
                currentBotCount += groupSize + 1;
                groupCount++;
                
                if (bossConfigData.DisableFollowers)
                {
                    bossDefaultData[0].BossEscortAmount = "0";
                    bossDefaultData[0].BossEscortType = bossDefaultData[0].BossName;
                    bossDefaultData[0].Supports = null!;
                }
                
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
                return location == "labyrinth" ? ModConfig.BossWaveDefaults["bossKillaAgroData"] : ModConfig.BossWaveDefaults["bossKillaAgroNonLabyData"];
            case "tagillaHelperAgro":
                return location == "labyrinth" ? ModConfig.BossWaveDefaults["tagillaHelperAgroData"] : ModConfig.BossWaveDefaults["tagillaHelperAgroNonLabyData"];
            case "bossPartisan":
                return ModConfig.BossWaveDefaults["bossPartisanData"];
            case "sectantPriest":
                return ModConfig.BossWaveDefaults["sectantPriestData"];
            case "arenaFighterEvent":
                return ModConfig.BossWaveDefaults["arenaFighterEventData"];
            case "pmcBot": // Requires Triggers + Has Multiple Zones
                return location switch
                {
                    "rezervbase" => ModConfig.BossWaveDefaults["pmcBotReserveData"],
                    "laboratory" => ModConfig.BossWaveDefaults["pmcBotLaboratoryData"],
                    _ => ModConfig.BossWaveDefaults["pmcBotData"]
                };
            case "exUsec": // Has Multiple Zones
                return ModConfig.BossWaveDefaults["exUsecData"];
            case "gifter":
                return ModConfig.BossWaveDefaults["gifterData"];
            default:
                logger.Error($"[ABPS] Boss not found in config {boss}");
                return null;
        }
    }
}