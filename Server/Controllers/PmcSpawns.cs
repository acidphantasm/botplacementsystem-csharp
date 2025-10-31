using _botplacementsystem.Globals;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace _botplacementsystem.Controllers;

[Injectable]
public class PmcSpawns
{
    private ISptLogger<PmcSpawns> _logger;
    private RandomUtil _randomUtil;
    private ICloner _cloner;
    private WeightedRandomHelper _weightedRandomHelper;
    private DatabaseService _databaseService;

    public PmcSpawns
    (
        ISptLogger<PmcSpawns> logger,
        ICloner cloner,
        WeightedRandomHelper weightedRandomHelper,
        RandomUtil randomUtil,
        DatabaseService databaseService
    )
    {
        _logger = logger;
        _cloner = cloner;
        _weightedRandomHelper = weightedRandomHelper;
        _randomUtil = randomUtil;
        _databaseService = databaseService;
    }

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
            pmcSpawnInfo = pmcSpawnInfo.Concat(GenerateStartingPMCWaves(location)).ToList();
        }

        if (ModConfig.Config.PmcConfig.Waves.Enable)
        {
            pmcSpawnInfo = pmcSpawnInfo.Concat(GeneratePMCWaves(location, escapeTimeLimit)).ToList();
        }
        return pmcSpawnInfo;
    }

    private List<BossLocationSpawn> GeneratePMCWaves(string location, double escapeTimeLimit)
    {
        var pmcWaveSpawnInfo = new List<BossLocationSpawn>();
        var ignoreMaxBotCaps = ModConfig.Config.PmcConfig.Waves.IgnoreMaxBotCaps;
        var difficultyWeights = ModConfig.Config.PmcDifficulty;
        var waveMaxPMCCount = location.Contains("factory") || location.Contains("labyrinth") ? Math.Min(2, ModConfig.Config.PmcConfig.Waves.MaxBotsPerWave - 2) : ModConfig.Config.PmcConfig.Waves.MaxBotsPerWave;
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
            var currentPMCCount = 0;
            var groupCount = 0;
            while (currentPMCCount < waveMaxPMCCount)
            {
                var canBeAGroup = groupCount >= waveGroupLimit ? false : true;
                var groupSize = 0;
                var remainingSpots = waveMaxPMCCount - currentPMCCount;
                var isAGroup = remainingSpots > 1 ? _randomUtil.GetChance100(waveGroupChance) : false;
                if (isAGroup && canBeAGroup) 
                {
                    groupSize = Math.Min(remainingSpots - 1, _randomUtil.GetInt(1, waveGroupSize));
                    groupCount++;
                }

                var pmcType = _randomUtil.GetChance100(50) ? "pmcUSEC" : "pmcBEAR";
                var bossDefaultData = _cloner.Clone(GetDefaultValuesForBoss(pmcType));

                bossDefaultData[0].BossEscortAmount = groupSize.ToString();
                bossDefaultData[0].Time = currentWaveTime;
                bossDefaultData[0].BossDifficulty = _weightedRandomHelper.GetWeightedValue(difficultyWeights);
                bossDefaultData[0].BossEscortDifficulty = _weightedRandomHelper.GetWeightedValue(difficultyWeights);
                bossDefaultData[0].BossZone = "";
                bossDefaultData[0].IgnoreMaxBots = ignoreMaxBotCaps;
                currentPMCCount += groupSize + 1;
                pmcWaveSpawnInfo.Add(bossDefaultData[0]);
            }
            
            currentWaveTime += waveTimer;
        }

        return pmcWaveSpawnInfo;
    }

    private List<BossLocationSpawn> GenerateStartingPMCWaves(string location)
    {
        var startingPMCWaveInfo = new List<BossLocationSpawn>();
        var ignoreMaxBotCaps = ModConfig.Config.PmcConfig.StartingPMCs.IgnoreMaxBotCaps;
        var mapAsMinMax = ModConfig.Config.PmcConfig.StartingPMCs.MapLimits[location];
        var minPMCCount = mapAsMinMax.Min;
        var maxPMCCount = mapAsMinMax.Max;
        var generatedPMCCount = _randomUtil.GetInt(minPMCCount, maxPMCCount);
        var groupChance = ModConfig.Config.PmcConfig.StartingPMCs.GroupChance;
        var groupLimit = ModConfig.Config.PmcConfig.StartingPMCs.MaxGroupCount;
        var groupMaxSize = ModConfig.Config.PmcConfig.StartingPMCs.MaxGroupSize;
        var difficultyWeights = ModConfig.Config.PmcDifficulty;

        var currentPMCCount = 0;
        var groupCount = 0;

        while (currentPMCCount < generatedPMCCount)
        {
            var canBeAGroup = groupCount >= groupLimit ? false : true;
            var groupSize = 0;
            var remainingSpots = generatedPMCCount - currentPMCCount;

            var isAGroup = remainingSpots > 1 ? _randomUtil.GetChance100(groupChance) : false;
            if (isAGroup && canBeAGroup) 
            {
                groupSize = Math.Min(remainingSpots - 1, _randomUtil.GetInt(1, groupMaxSize));
                groupCount++;
            }

            var pmcType = _randomUtil.GetChance100(50) ? "pmcUSEC" : "pmcBEAR";
            var bossDefaultData = _cloner.Clone(this.GetDefaultValuesForBoss(pmcType));

            bossDefaultData[0].BossEscortAmount = groupSize.ToString();
            bossDefaultData[0].BossDifficulty = _weightedRandomHelper.GetWeightedValue(difficultyWeights);
            bossDefaultData[0].BossEscortDifficulty = _weightedRandomHelper.GetWeightedValue(difficultyWeights);
            bossDefaultData[0].BossZone = "";
            bossDefaultData[0].IgnoreMaxBots = ignoreMaxBotCaps;
            currentPMCCount += groupSize + 1;
            startingPMCWaveInfo.Add(bossDefaultData[0]);
        }
        
        return startingPMCWaveInfo;
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
                _logger.Error($"[ABPS] PMC not found in config {boss}");
                return null;
        }
    }
    
    public List<BossLocationSpawn> GenerateScavRaidRemainingPMCs(string location, double remainingRaidTime)
    {
        location = location.ToLowerInvariant();
        
        var startingPMCWaveInfo = new List<BossLocationSpawn>();
        var ignoreMaxBotCaps = ModConfig.Config.PmcConfig.StartingPMCs.IgnoreMaxBotCaps;
        var MapMinMax = ModConfig.Config.PmcConfig.StartingPMCs.MapLimits[location];
        var minPMCCount = MapMinMax.Min;
        var maxPMCCount = MapMinMax.Max;
        var generatedPMCCount = _randomUtil.GetInt(minPMCCount, maxPMCCount);
        var groupChance = ModConfig.Config.PmcConfig.StartingPMCs.GroupChance;
        var groupLimit = ModConfig.Config.PmcConfig.StartingPMCs.MaxGroupCount;
        var groupMaxSize = ModConfig.Config.PmcConfig.StartingPMCs.MaxGroupSize;
        var difficultyWeights = ModConfig.Config.PmcDifficulty;

        var currentPMCCount = 0;
        var groupCount = 0;

        if (remainingRaidTime < 600) generatedPMCCount = _randomUtil.GetInt(1, 3);
        if (remainingRaidTime < 1200) generatedPMCCount = _randomUtil.GetInt(1, 6);
        if (remainingRaidTime < 1800) generatedPMCCount = _randomUtil.GetInt(4, 9);

        if (location.Contains("factory") && generatedPMCCount > 5) generatedPMCCount -= 2;

        while (currentPMCCount < generatedPMCCount)
        {
            var canBeAGroup = groupCount >= groupLimit ? false : true;
            var groupSize = 0;
            var remainingSpots = generatedPMCCount - currentPMCCount;
            var isAGroup = remainingSpots > 1 ? _randomUtil.GetChance100(groupChance) : false;
            if (isAGroup && canBeAGroup) 
            {
                groupSize = Math.Min(remainingSpots - 1, _randomUtil.GetInt(1, groupMaxSize));
                groupCount++;
            }

            var pmcType = _randomUtil.GetChance100(50) ? "pmcUSEC" : "pmcBEAR";
            var bossDefaultData = _cloner.Clone(GetDefaultValuesForBoss(pmcType));

            bossDefaultData[0].BossEscortAmount = groupSize.ToString();
            bossDefaultData[0].BossDifficulty = _weightedRandomHelper.GetWeightedValue(difficultyWeights);
            bossDefaultData[0].BossEscortDifficulty = _weightedRandomHelper.GetWeightedValue(difficultyWeights);
            bossDefaultData[0].BossZone = "";
            bossDefaultData[0].IgnoreMaxBots = ignoreMaxBotCaps;
            currentPMCCount += groupSize + 1;
            startingPMCWaveInfo.Add(bossDefaultData[0]);
        }
        
        return startingPMCWaveInfo;
    }
}