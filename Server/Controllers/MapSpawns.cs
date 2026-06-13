using _botplacementsystem.Globals;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;

namespace _botplacementsystem.Controllers;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]
public class MapSpawns(
    ISptLogger<MapSpawns> logger,
    BossSpawns bossSpawns,
    ScavSpawns scavSpawns,
    PmcSpawns pmcSpawns,
    VanillaAdjustments vanillaAdjustments,
    ICloner cloner,
    DatabaseService databaseService)
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

    private readonly Dictionary<string, List<BossLocationSpawn>> _botMapCache = new();
    private readonly Dictionary<string, List<Wave>> _scavMapCache = new();
    // Boss spawns injected by OTHER mods (custom factions: ISB / UNTAR / RUAF / BlackDiv ...) captured
    // before ABPS wipes BossLocationSpawn, so they can be restored alongside ABPS's own roster.
    private readonly Dictionary<string, List<BossLocationSpawn>> _externalBossSpawnCache = new();
    private Dictionary<string, Location> _locationData = new();

    private ISptLogger<MapSpawns> _logger = logger;

    public void ConfigureInitialData()
    {
        _locationData = databaseService.GetLocations().GetDictionary();

        // Anything in BossLocationSpawn whose BossName is NOT one of these was injected by another mod
        // (custom factions). ABPS wipes the whole list just below and rebuilds only from its own roster,
        // which used to delete those faction spawns on every boot — they had 100% spawn chance yet never
        // appeared in raids. Capture them per map before the wipe; restore in ReplaceOriginalLocations.
        var managedBossNames = BuildManagedBossNames();

        foreach (var map in _validMaps)
        {
            var actualKey = databaseService.GetLocations().GetMappedKey(map);
            CaptureExternalBossSpawns(map, _locationData[actualKey].Base, managedBossNames);
            _locationData[actualKey].Base.BossLocationSpawn = [];
            _botMapCache[map] = [];
            _scavMapCache[map] = [];
            switch (ModConfig.Config.ScavConfig.Waves.Enable)
            {
                case true when ModConfig.Config.ScavConfig.StartingScavs.Enable:
                    vanillaAdjustments.EnableAllSpawnSystems(_locationData[actualKey].Base);
                    break;
                case false when ModConfig.Config.ScavConfig.StartingScavs.Enable:
                    vanillaAdjustments.DisableNewSpawnSystem(_locationData[actualKey].Base);
                    break;
                case false when !ModConfig.Config.ScavConfig.StartingScavs.Enable:
                    vanillaAdjustments.DisableAllSpawnSystems(_locationData[actualKey].Base);
                    break;
                case true when !ModConfig.Config.ScavConfig.StartingScavs.Enable:
                    vanillaAdjustments.DisableOldSpawnSystem(_locationData[actualKey].Base);
                    break;
            }

            vanillaAdjustments.RemoveExistingWaves(_locationData[actualKey].Base);
            vanillaAdjustments.FixPMCHostility(_locationData[actualKey].Base);
            vanillaAdjustments.AdjustNewWaveSettings(_locationData[actualKey].Base);
        }

        vanillaAdjustments.CheckAndAddScavBrainTypes();
        vanillaAdjustments.DisableVanillaSettings();
        vanillaAdjustments.RemoveCustomPMCWaves();
        BuildInitialCache();
    }

    private void BuildInitialCache()
    {
        BuildBossWaves();
        BuildPmcWaves();
        BuildStartingScavs();
        ReplaceOriginalLocations();
    }

    private void BuildBossWaves()
    {
        foreach (var map in _validMaps)
        {
            var actualKey = databaseService.GetLocations().GetMappedKey(map);
            
            var mapData =
                bossSpawns.GetCustomMapData(map, _locationData[actualKey].Base.EscapeTimeLimit.GetValueOrDefault());

            if (mapData.Count == 0)
            {
                continue;
            }
            
            foreach (var spawn in mapData)
            {
                _botMapCache[map].Add(spawn);
            }
        }
    }

    private void BuildPmcWaves()
    {
        foreach (var map in _validMaps)
        {
            var actualKey = databaseService.GetLocations().GetMappedKey(map);
            
            var mapData = pmcSpawns.GetCustomMapData(map,
                _locationData[actualKey].Base.EscapeTimeLimit.GetValueOrDefault()
            );

            if (mapData.Count == 0)
            {
                continue;
            }
            
            foreach (var spawn in mapData)
            {
                _botMapCache[map].Add(spawn);
            }
        }
    }

    private void BuildStartingScavs()
    {
        foreach (var map in _validMaps)
        {
            if ((map == "laboratory" && !ModConfig.Config.ScavConfig.Waves.AllowScavsOnLaboratory) || 
                (map == "labyrinth" && !ModConfig.Config.ScavConfig.Waves.AllowScavsOnLabyrinth)) continue;
            
            var mapData = scavSpawns.GetCustomMapData(map);

            if (mapData.Count == 0)
            {
                continue;
            }
            
            foreach (var spawn in mapData)
            {
                _scavMapCache[map].Add(spawn);
            }
        }
    }

    private void ReplaceOriginalLocations()
    {
        foreach (var map in _validMaps)
        {
            var actualKey = databaseService.GetLocations().GetMappedKey(map);

            var finalBossSpawns = cloner.Clone(_botMapCache[map]);
            // Re-add the faction / other-mod boss spawns captured before the wipe so they coexist with
            // ABPS's roster instead of being deleted.
            if (_externalBossSpawnCache.TryGetValue(map, out var external) && external.Count > 0)
                finalBossSpawns.AddRange(cloner.Clone(external));

            _locationData[actualKey].Base.BossLocationSpawn = finalBossSpawns;
            _locationData[actualKey].Base.Waves = cloner.Clone(_scavMapCache[map]);
        }
    }

    // Names ABPS regenerates itself: every boss in its BossConfig (enabled or not, so a user-disabled
    // boss is never resurrected) plus its own PMC roles. Everything else found in a location's
    // BossLocationSpawn was put there by another mod and must be preserved.
    private HashSet<string> BuildManagedBossNames()
    {
        var managed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (boss, _) in ModConfig.Config.BossConfig)
            managed.Add(boss);
        managed.Add("pmcUSEC");
        managed.Add("pmcBEAR");
        return managed;
    }

    private void CaptureExternalBossSpawns(string map, LocationBase locationBase, HashSet<string> managedBossNames)
    {
        var external = new List<BossLocationSpawn>();
        var existing = locationBase.BossLocationSpawn;
        if (existing is not null)
        {
            foreach (var spawn in existing)
            {
                if (spawn?.BossName is null) continue;
                if (managedBossNames.Contains(spawn.BossName)) continue; // ABPS owns/regenerates this type
                external.Add(cloner.Clone(spawn));                        // injected by another mod — keep it
            }
        }

        _externalBossSpawnCache[map] = external;
        if (external.Count > 0)
            logger.Info($"[ABPS] Preserving {external.Count} external boss spawn(s) on {map} from other mods: {string.Join(", ", external.Select(s => s.BossName).Distinct())}");
    }
}