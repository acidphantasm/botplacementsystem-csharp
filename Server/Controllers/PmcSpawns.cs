using _botplacementsystem.Globals;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace _botplacementsystem.Controllers;

[Injectable]
public class PmcSpawns(
    ISptLogger<PmcSpawns> logger,
    ICloner cloner,
    WeightedRandomHelper weightedRandomHelper,
    RandomUtil randomUtil)
{
    public List<BossLocationSpawn> GetCustomMapData(string location, double escapeTimeLimit)
    {
        return GetConfigValueForLocation(location, escapeTimeLimit);
    }

    private List<BossLocationSpawn> GetConfigValueForLocation(string location, double escapeTimeLimit)
    {
        location = location.ToLowerInvariant();

        var pmcSpawnInfo = new List<BossLocationSpawn>();

        if (ModConfig.Config.PmcConfig.StartingPMCs.Enable)
        {
            pmcSpawnInfo.AddRange(GenerateStartingPmcWaves(location));
        }

        var canGenerateWaves = ModConfig.Config.PmcConfig.Waves.Enable && (location != "labyrinth" || ModConfig.Config.PmcConfig.Waves.AllowPmcsOnLabyrinth);
        if (canGenerateWaves)
        {
            pmcSpawnInfo.AddRange(GeneratePmcWaves(location, escapeTimeLimit));
        }

        return pmcSpawnInfo;
    }

    private List<BossLocationSpawn> GeneratePmcWaves(string location, double escapeTimeLimit)
    {
        var pmcWaveSpawnInfo = new List<BossLocationSpawn>();
        var ignoreMaxBotCaps = ModConfig.Config.PmcConfig.Waves.IgnoreMaxBotCaps;
        var difficultyWeights = ModConfig.Config.PmcDifficulty;
        var waveMaxPmcCount = location.Contains("factory") || location.Contains("labyrinth") ? Math.Min(2, ModConfig.Config.PmcConfig.Waves.MaxBotsPerWave - 2) : ModConfig.Config.PmcConfig.Waves.MaxBotsPerWave;
        var waveGroupLimit = ModConfig.Config.PmcConfig.Waves.MaxGroupCount;
        var waveGroupSize = ModConfig.Config.PmcConfig.Waves.MaxGroupSize;
        var waveGroupChance = ModConfig.Config.PmcConfig.Waves.GroupChance;
        var firstWaveTimer = ModConfig.Config.PmcConfig.Waves.DelayBeforeFirstWave;
        var waveTimer = ModConfig.Config.PmcConfig.Waves.SecondsBetweenWaves;
        var endWavesAtRemainingTime = ModConfig.Config.PmcConfig.Waves.StopWavesBeforeEndOfRaidLimit;
        var waveCount = Math.Floor((double) (((escapeTimeLimit * 60) - endWavesAtRemainingTime) - firstWaveTimer) / waveTimer);
        var currentWaveTime = firstWaveTimer;
        
        for (var i = 1; i <= waveCount; i++)
        {
            var currentPmcCount = 0;
            var groupCount = 0;
            while (currentPmcCount < waveMaxPmcCount)
            {
                var canBeAGroup = groupCount < waveGroupLimit;
                var groupSize = 0;
                var remainingSpots = waveMaxPmcCount - currentPmcCount;
                var isAGroup = remainingSpots > 1 && randomUtil.GetChance100(waveGroupChance);
                if (isAGroup && canBeAGroup) 
                {
                    groupSize = Math.Min(remainingSpots - 1, randomUtil.GetInt(1, waveGroupSize));
                    groupCount++;
                }

                var pmcType = randomUtil.GetChance100(ModConfig.Config.PmcType.UsecChance) ? "pmcUSEC" : "pmcBEAR";
                var bossDefaultData = cloner.Clone(GetDefaultValuesForBoss(pmcType));

                if (bossDefaultData is null) continue;
                
                bossDefaultData[0].BossEscortAmount = groupSize.ToString();
                bossDefaultData[0].Time = currentWaveTime;
                bossDefaultData[0].BossDifficulty = weightedRandomHelper.GetWeightedValue(difficultyWeights);
                bossDefaultData[0].BossEscortDifficulty = weightedRandomHelper.GetWeightedValue(difficultyWeights);
                bossDefaultData[0].BossZone = "";
                bossDefaultData[0].IgnoreMaxBots = ignoreMaxBotCaps;
                currentPmcCount += groupSize + 1;
                pmcWaveSpawnInfo.Add(bossDefaultData[0]);
            }
            
            currentWaveTime += waveTimer;
        }

        return pmcWaveSpawnInfo;
    }

    private List<BossLocationSpawn> GenerateStartingPmcWaves(string location)
    {
        var startingPmcWaveInfo = new List<BossLocationSpawn>();
        var ignoreMaxBotCaps = ModConfig.Config.PmcConfig.StartingPMCs.IgnoreMaxBotCaps;
        var mapAsMinMax = ModConfig.Config.PmcConfig.StartingPMCs.MapLimits[location];
        var minPmcCount = mapAsMinMax.Min;
        var maxPmcCount = mapAsMinMax.Max;
        var generatedPmcCount = randomUtil.GetInt(minPmcCount, maxPmcCount);
        var groupChance = ModConfig.Config.PmcConfig.StartingPMCs.GroupChance;
        var groupLimit = ModConfig.Config.PmcConfig.StartingPMCs.MaxGroupCount;
        var groupMaxSize = ModConfig.Config.PmcConfig.StartingPMCs.MaxGroupSize;
        var difficultyWeights = ModConfig.Config.PmcDifficulty;

        var currentPmcCount = 0;
        var groupCount = 0;

        while (currentPmcCount < generatedPmcCount)
        {
            var canBeAGroup = groupCount < groupLimit;
            var groupSize = 0;
            var remainingSpots = generatedPmcCount - currentPmcCount;

            var isAGroup = remainingSpots > 1 && randomUtil.GetChance100(groupChance);
            if (isAGroup && canBeAGroup) 
            {
                groupSize = Math.Min(remainingSpots - 1, randomUtil.GetInt(1, groupMaxSize));
                groupCount++;
            }

            var isTrue = randomUtil.GetChance100(ModConfig.Config.PmcType.UsecChance);
            var pmcType = randomUtil.GetChance100(ModConfig.Config.PmcType.UsecChance) ? "pmcUSEC" : "pmcBEAR";
            var bossDefaultData = cloner.Clone(this.GetDefaultValuesForBoss(pmcType));

            if (bossDefaultData is null) continue;
            
            bossDefaultData[0].BossEscortAmount = groupSize.ToString();
            bossDefaultData[0].BossDifficulty = weightedRandomHelper.GetWeightedValue(difficultyWeights);
            bossDefaultData[0].BossEscortDifficulty = weightedRandomHelper.GetWeightedValue(difficultyWeights);
            bossDefaultData[0].BossZone = "";
            bossDefaultData[0].IgnoreMaxBots = ignoreMaxBotCaps;
            currentPmcCount += groupSize + 1;
            startingPmcWaveInfo.Add(bossDefaultData[0]);
        }
        return startingPmcWaveInfo;
    }

    private List<BossLocationSpawn>? GetDefaultValuesForBoss(string boss)
    {
        switch (boss)
        {
            case "pmcUSEC":
                return ModConfig.PmcDefaults.PmcUSEC;
            case "pmcBEAR":
                return ModConfig.PmcDefaults.PmcBEAR;
            default:
                logger.Error($"[ABPS] PMC not found in config {boss}");
                return null;
        }
    }
    
    public List<BossLocationSpawn> GenerateScavRaidRemainingPmcs(string location, double remainingRaidTime)
    {
        location = location.ToLowerInvariant();
        
        var startingPmcWaveInfo = new List<BossLocationSpawn>();
        var ignoreMaxBotCaps = ModConfig.Config.PmcConfig.StartingPMCs.IgnoreMaxBotCaps;
        var mapMinMax = ModConfig.Config.PmcConfig.StartingPMCs.MapLimits[location];
        var minPmcCount = mapMinMax.Min;
        var maxPmcCount = mapMinMax.Max;
        var generatedPmcCount = randomUtil.GetInt(minPmcCount, maxPmcCount);
        var groupChance = ModConfig.Config.PmcConfig.StartingPMCs.GroupChance;
        var groupLimit = ModConfig.Config.PmcConfig.StartingPMCs.MaxGroupCount;
        var groupMaxSize = ModConfig.Config.PmcConfig.StartingPMCs.MaxGroupSize;
        var difficultyWeights = ModConfig.Config.PmcDifficulty;

        var currentPmcCount = 0;
        var groupCount = 0;

        if (remainingRaidTime < 600) generatedPmcCount = randomUtil.GetInt(1, 3);
        if (remainingRaidTime < 1200) generatedPmcCount = randomUtil.GetInt(1, 6);
        if (remainingRaidTime < 1800) generatedPmcCount = randomUtil.GetInt(4, 9);

        if (location.Contains("factory") && generatedPmcCount > 5) generatedPmcCount -= 2;

        while (currentPmcCount < generatedPmcCount)
        {
            var canBeAGroup = groupCount < groupLimit;
            var groupSize = 0;
            var remainingSpots = generatedPmcCount - currentPmcCount;
            var isAGroup = remainingSpots > 1 && randomUtil.GetChance100(groupChance);
            if (isAGroup && canBeAGroup) 
            {
                groupSize = Math.Min(remainingSpots - 1, randomUtil.GetInt(1, groupMaxSize));
                groupCount++;
            }

            var pmcType = randomUtil.GetChance100(50) ? "pmcUSEC" : "pmcBEAR";
            var bossDefaultData = cloner.Clone(GetDefaultValuesForBoss(pmcType));

            if (bossDefaultData is null) continue;
            
            bossDefaultData[0].BossEscortAmount = groupSize.ToString();
            bossDefaultData[0].BossDifficulty = weightedRandomHelper.GetWeightedValue(difficultyWeights);
            bossDefaultData[0].BossEscortDifficulty = weightedRandomHelper.GetWeightedValue(difficultyWeights);
            bossDefaultData[0].BossZone = "";
            bossDefaultData[0].IgnoreMaxBots = ignoreMaxBotCaps;
            currentPmcCount += groupSize + 1;
            startingPmcWaveInfo.Add(bossDefaultData[0]);
        }
        
        return startingPmcWaveInfo;
    }
}