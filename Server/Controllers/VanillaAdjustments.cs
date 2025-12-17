using _botplacementsystem.Globals;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace _botplacementsystem.Controllers;

[Injectable]
public class VanillaAdjustments(
    ICloner cloner,
    ConfigServer configServer,
    DatabaseServer databaseServer)
{
    private readonly LocationConfig _locationConfig = configServer.GetConfig<LocationConfig>();
    private readonly PmcConfig _pmcConfig = configServer.GetConfig<PmcConfig>();
    private readonly BotConfig _botConfig = configServer.GetConfig<BotConfig>();

    public void DisableVanillaSettings()
    {
        // LocationConfig.SplitWaveIntoSingleSpawnSettins.Enabled = false;
        _locationConfig.RogueLighthouseSpawnTimeSettings.Enabled = false;
        _locationConfig.AddOpenZonesToAllMaps = false;
        _locationConfig.AddCustomBotWavesToMaps = false;
        _locationConfig.EnableBotTypeLimits = false;
    }

    public void DisableNewSpawnSystem(LocationBase locationBase)
    {
        locationBase.NewSpawn = false;
        locationBase.OfflineNewSpawn = false;
        locationBase.OldSpawn = true;
        locationBase.OfflineOldSpawn = true;
    }

    public void DisableOldSpawnSystem(LocationBase locationBase)
    {
        if ((locationBase.Id.ToLowerInvariant() == "laboratory" && !ModConfig.Config.ScavConfig.Waves.AllowScavsOnLaboratory) || 
            (locationBase.Id.ToLowerInvariant() == "labyrinth" && !ModConfig.Config.ScavConfig.Waves.AllowScavsOnLabyrinth)) return;
        
        locationBase.NewSpawn = true;
        locationBase.OfflineNewSpawn = true;
        locationBase.OldSpawn = false;
        locationBase.OfflineOldSpawn = false;
    }

    public void EnableAllSpawnSystems(LocationBase locationBase)
    {
        if ((locationBase.Id.ToLowerInvariant() == "laboratory" && !ModConfig.Config.ScavConfig.Waves.AllowScavsOnLaboratory) || 
            (locationBase.Id.ToLowerInvariant() == "labyrinth" && !ModConfig.Config.ScavConfig.Waves.AllowScavsOnLabyrinth)) return;
        
        locationBase.NewSpawn = true;
        locationBase.OfflineNewSpawn = true;
        locationBase.OldSpawn = true;
        locationBase.OfflineOldSpawn = true;
        if (locationBase.NonWaveGroupScenario is not null)
        {
            locationBase.NonWaveGroupScenario.Chance = 0;
        }
    }

    public void DisableAllSpawnSystems(LocationBase locationBase)
    {
        locationBase.NewSpawn = false;
        locationBase.OfflineNewSpawn = false;
        locationBase.OldSpawn = false;
        locationBase.OfflineOldSpawn = false;
    }

    public void AdjustNewWaveSettings(LocationBase locationBase)
    {
        if ((locationBase.Id == "laboratory" && !ModConfig.Config.ScavConfig.Waves.AllowScavsOnLaboratory) || 
            (locationBase.Id == "labyrinth" && !ModConfig.Config.ScavConfig.Waves.AllowScavsOnLabyrinth)) return;

        if (ModConfig.Config.ScavConfig.Waves.EnableCustomTimers && (locationBase.Id.Contains("factory") || locationBase.Id.Contains("labyrinth") || locationBase.Id.Contains("laboratory")))
        {
            // Start-Stop Time for spawns
            locationBase.BotStart = ModConfig.Config.ScavConfig.Waves.StartSpawns;
            locationBase.BotStop =
                (int)locationBase.EscapeTimeLimit * 60 - ModConfig.Config.ScavConfig.Waves.StopSpawns;

            // Start-Stop wave times for active spawning
            locationBase.BotSpawnTimeOnMin = 10;
            locationBase.BotSpawnTimeOnMax = 30;

            // Start-Stop wave wait times between active spawning
            locationBase.BotSpawnTimeOffMin = 120;
            locationBase.BotSpawnTimeOffMax = 180;

            // Probably how often it checks to spawn while active spawning
            locationBase.BotSpawnPeriodCheck = 15;

            // Bot count required to trigger a spawn
            locationBase.BotSpawnCountStep = 3;
        }
        else
        {
            // Start-Stop Time for spawns
            locationBase.BotStart = ModConfig.Config.ScavConfig.Waves.StartSpawns;
            locationBase.BotStop =
                (int)locationBase.EscapeTimeLimit * 60 - ModConfig.Config.ScavConfig.Waves.StopSpawns;

            // Start-Stop wave times for active spawning
            locationBase.BotSpawnTimeOnMin = ModConfig.Config.ScavConfig.Waves.ActiveTimeMin;
            locationBase.BotSpawnTimeOnMax = ModConfig.Config.ScavConfig.Waves.ActiveTimeMax;

            // Start-Stop wave wait times between active spawning
            locationBase.BotSpawnTimeOffMin = ModConfig.Config.ScavConfig.Waves.QuietTimeMin;
            locationBase.BotSpawnTimeOffMax = ModConfig.Config.ScavConfig.Waves.QuietTimeMax;

            // Probably how often it checks to spawn while active spawning
            locationBase.BotSpawnPeriodCheck = ModConfig.Config.ScavConfig.Waves.CheckToSpawnTimer;

            // Bot count required to trigger a spawn
            locationBase.BotSpawnCountStep = ModConfig.Config.ScavConfig.Waves.PendingBotsToTrigger;
        }
        
        locationBase.BotLocationModifier.NonWaveSpawnBotsLimitPerPlayer = ModConfig.Config.ScavConfig.Waves.NonWaveSpawnBotsLimitPerPlayer;
        locationBase.BotLocationModifier.NonWaveSpawnBotsLimitPerPlayerPvE = ModConfig.Config.ScavConfig.Waves.NonWaveSpawnBotsLimitPerPlayer;
    }

    public void RemoveExistingWaves(LocationBase locationBase)
    {
        locationBase.Waves = [];
    }

    public void CheckAndAddScavBrainTypes()
    {
        if (!_botConfig.PlayerScavBrainType.ContainsKey("labyrinth"))
        {
            _botConfig.PlayerScavBrainType["labyrinth"] = cloner.Clone(_botConfig.PlayerScavBrainType["laboratory"]);
        }
        
        if (!_botConfig.AssaultBrainType.ContainsKey("labyrinth"))
        {
            _botConfig.AssaultBrainType["labyrinth"] = cloner.Clone(_botConfig.AssaultBrainType["laboratory"]);
        }
    }

    public void FixPMCHostility(LocationBase locationBase)
    {
        var hostility = locationBase.BotLocationModifier?.AdditionalHostilitySettings.ToList();
        if (hostility is not null || hostility.Any())
        {
            for (var bot = 0; bot < hostility.Count; bot++)
            {
                if (hostility[bot].BotRole == "pmcUSEC" || hostility[bot].BotRole == "pmcBEAR")
                {
                    var newHostilitySettings = cloner.Clone(ModConfig.HostilityDefaults);
                    newHostilitySettings.BotRole = hostility[bot].BotRole;
                    hostility[bot] = newHostilitySettings;
                }

                // Fix scav hostility settings for every map
                if (hostility[bot].BotRole == "assault" || hostility[bot].BotRole == "marksman")
                {
                    var newHostilitySettings = cloner.Clone(ModConfig.HostilityDefaults);
                    newHostilitySettings.BotRole = hostility[bot].BotRole;
                    foreach (var botType in newHostilitySettings.AlwaysEnemies)
                    {
                        if (botType == "pmcBEAR" || botType == "pmcUSEC") continue;
                        
                        newHostilitySettings.AlwaysFriends.Add(botType);
                        newHostilitySettings.AlwaysEnemies.Remove(botType);
                    }
                    hostility[bot] = newHostilitySettings;
                }
            }
        }

        var databaseBots = databaseServer.GetTables().Bots.Types;
        foreach (var (bot, data) in databaseBots)
        {
            if (bot.Contains("assault") || bot.Contains("marksman"))
            {
                foreach (var (difficulty, dataSet) in databaseBots[bot].BotDifficulty)
                {
                    if (databaseBots[bot].BotDifficulty[difficulty].Mind.EnemyBotTypes is null)
                    {
                        databaseBots[bot].BotDifficulty[difficulty].Mind.EnemyBotTypes = new List<WildSpawnType>();
                    }
                    databaseBots[bot].BotDifficulty[difficulty].Mind.EnemyBotTypes.Add(WildSpawnType.pmcUSEC);
                    databaseBots[bot].BotDifficulty[difficulty].Mind.EnemyBotTypes.Add(WildSpawnType.pmcBEAR);
                }
            }
        }

        foreach (var (bot, data) in _pmcConfig.HostilitySettings)
        {
            if (_pmcConfig.HostilitySettings[bot].AdditionalEnemyTypes is not null)
            {
                if (!_pmcConfig.HostilitySettings[bot].AdditionalEnemyTypes.Contains("assault")) 
                    _pmcConfig.HostilitySettings[bot].AdditionalEnemyTypes.Add("assault");
                
                if (!_pmcConfig.HostilitySettings[bot].AdditionalEnemyTypes.Contains("pmcBEAR")) 
                    _pmcConfig.HostilitySettings[bot].AdditionalEnemyTypes.Add("pmcBEAR");
                
                if (!_pmcConfig.HostilitySettings[bot].AdditionalEnemyTypes.Contains("pmcUSEC")) 
                    _pmcConfig.HostilitySettings[bot].AdditionalEnemyTypes.Add("pmcUSEC");
            }
            _pmcConfig.HostilitySettings[bot].SavageEnemyChance = 100;
            _pmcConfig.HostilitySettings[bot].BearEnemyChance = 100;
            _pmcConfig.HostilitySettings[bot].UsecEnemyChance = 100;
            _pmcConfig.HostilitySettings[bot].SavagePlayerBehaviour = "AlwaysEnemies";

            foreach (var chancedEnemy in _pmcConfig.HostilitySettings[bot].ChancedEnemies)
            {
                chancedEnemy.EnemyChance = 100;
            }
        }
    }
    
    public void RemoveCustomPMCWaves()
    {
        _pmcConfig.RemoveExistingPmcWaves = false;
        _pmcConfig.CustomPmcWaves = new Dictionary<string, List<BossLocationSpawn>>();
    }
}