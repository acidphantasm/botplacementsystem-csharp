using BotPlacementSystemServer.Globals;
using BotPlacementSystemServer.Models;
using BotPlacementSystemServer.Models.Enums;
using BotPlacementSystemServer.Service;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Utils.Cloners;

namespace BotPlacementSystemServer.Controllers;

using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Tables;

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = OnLoadOrder.PostLoad + 69420)]
public class MapSpawns(
    ISptLogger<MapSpawns> logger,
    BossSpawns bossSpawns,
    ScavSpawns scavSpawns,
    PmcSpawns pmcSpawns,
    VanillaAdjustments vanillaAdjustments,
    ICloner cloner,
    LocationTable locationTable,
    RaidLifecycleService raidLifecycleService,
    SeasonalEventConfig seasonalEventConfig)
    : IOnLoad
{
    private readonly List<string> _validMaps =
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
    private Dictionary<string, Location> _locationData = new();
    
    // Boss spawns ABPS does not manage (seasonal events, vanilla extras, other mods), captured before ABPS
    // first wipes BossLocationSpawn. Re-added on every rebuild when their toggle in AdditionalSpawns is on.
    private readonly Dictionary<string, List<PreservedSpawn>> _preservedSpawns = new();
    // Exact instances ABPS last wrote to each map, so anything else found there later was injected after ABPS loaded.
    private readonly Dictionary<string, HashSet<BossLocationSpawn>> _writtenByAbps = new();
    private bool _preservedSpawnsCaptured;

    private sealed record PreservedSpawn(PreservedSpawnSource Source, string? EventKey, BossLocationSpawn Spawn);

    public Task OnLoadAsync(CancellationToken token)
    {
        ConfigureInitialData();
        
        ModConfig.ConfigChanged += ConfigureInitialData;
        raidLifecycleService.RaidFinished += RebuildMap;
        
        return Task.CompletedTask;
    }

    private void ConfigureInitialData()
    {
        _locationData = locationTable.GetDictionary();
        
        foreach (var map in _validMaps)
        {
            var actualKey = locationTable.GetMappedKey(map);
            CapturePreservedSpawns(map, actualKey);
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
            vanillaAdjustments.FixPmcHostility(_locationData[actualKey].Base);
            vanillaAdjustments.AdjustNewWaveSettings(_locationData[actualKey].Base);
        }

        vanillaAdjustments.CheckAndAddScavBrainTypes();
        vanillaAdjustments.DisableVanillaSettings();
        vanillaAdjustments.RemoveCustomPmcWaves();
        BuildInitialCache();
        _preservedSpawnsCaptured = true;
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
            var actualKey = locationTable.GetMappedKey(map);
            
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
            var actualKey = locationTable.GetMappedKey(map);
            
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
            var actualKey = locationTable.GetMappedKey(map);
            WriteBossSpawns(map, actualKey);
            _locationData[actualKey].Base.Waves = cloner.Clone(_scavMapCache[map]);
        }
    }

    private void RebuildMap(string location)
    {
        if (!_validMaps.Contains(location)) return;
    
        CapturePreservedSpawns(location, locationTable.GetMappedKey(location));
        _botMapCache[location] = [];
        _scavMapCache[location] = [];

        BuildBossWavesForMap(location);
        BuildPmcWavesForMap(location);
        BuildStartingScavsForMap(location);
        ReplaceOriginalLocationForMap(location);
    }
    
    private void BuildBossWavesForMap(string location)
    {
        var actualKey = locationTable.GetMappedKey(location);
        var mapData = bossSpawns.GetCustomMapData(location, _locationData[actualKey].Base.EscapeTimeLimit.GetValueOrDefault());
        foreach (var spawn in mapData)
        {
            _botMapCache[location].Add(spawn);
        }
    }

    private void BuildPmcWavesForMap(string location)
    {
        var actualKey = locationTable.GetMappedKey(location);
        var mapData = pmcSpawns.GetCustomMapData(location, _locationData[actualKey].Base.EscapeTimeLimit.GetValueOrDefault());
        foreach (var spawn in mapData)
        {
            _botMapCache[location].Add(spawn);
        }
    }

    private void BuildStartingScavsForMap(string location)
    {
        if ((location == "laboratory" && !ModConfig.Config.ScavConfig.Waves.AllowScavsOnLaboratory) || (location == "labyrinth" && !ModConfig.Config.ScavConfig.Waves.AllowScavsOnLabyrinth)) 
            return;
        
        var mapData = scavSpawns.GetCustomMapData(location);
        foreach (var spawn in mapData)
        {
            _scavMapCache[location].Add(spawn);
        }
    }

    private void ReplaceOriginalLocationForMap(string location)
    {
        var actualKey = locationTable.GetMappedKey(location);
        WriteBossSpawns(location, actualKey);
        _locationData[actualKey].Base.Waves = cloner.Clone(_scavMapCache[location]);
    }

    /// <summary>
    /// Writes ABPS's generated boss spawns plus any preserved spawns whose toggle is enabled.
    /// With every AdditionalSpawns toggle off this is identical to writing only ABPS's spawns.
    /// </summary>
    private void WriteBossSpawns(string map, string actualKey)
    {
        List<BossLocationSpawn> bossSpawns = [..cloner.Clone(_botMapCache[map])!, ..GetEnabledPreservedSpawns(map)];
        _locationData[actualKey].Base.BossLocationSpawn = bossSpawns;
        _writtenByAbps[map] = new HashSet<BossLocationSpawn>(bossSpawns, ReferenceEqualityComparer.Instance);
    }

    private List<BossLocationSpawn> GetEnabledPreservedSpawns(string map)
    {
        if (!_preservedSpawns.TryGetValue(map, out var preserved)) return [];

        var settings = ModConfig.Config.AdditionalSpawns;
        return preserved
            .Where(p => p.Source switch
            {
                PreservedSpawnSource.SeasonalEvent => settings.SeasonalEvents.IsEventEnabled(p.EventKey ?? string.Empty),
                PreservedSpawnSource.VanillaExtra => settings.VanillaExtras.IsEnabled(p.Spawn.BossName ?? string.Empty),
                PreservedSpawnSource.OtherMod => settings.OtherMods,
                _ => false
            })
            .Select(p => cloner.Clone(p.Spawn)!)
            .ToList();
    }

    /// <summary>
    /// On the first run, records every boss spawn on the map that ABPS does not manage, before ABPS wipes the list.
    /// On later runs (raid end, config change), records only spawns injected since ABPS last wrote the map.
    /// </summary>
    private void CapturePreservedSpawns(string map, string actualKey)
    {
        var current = _locationData[actualKey].Base.BossLocationSpawn;
        if (!_preservedSpawns.TryGetValue(map, out var preserved))
        {
            preserved = [];
            _preservedSpawns[map] = preserved;
        }

        if (current is null) return;

        _writtenByAbps.TryGetValue(map, out var written);
        // Only late injections are de-duplicated (against what was already preserved), so a mod re-adding the same
        // spawn every raid doesn't pile up. The first capture keeps everything: SPT's zombie event has many identical entries.
        var alreadyPreserved = _preservedSpawnsCaptured ? preserved.ToList() : [];
        var added = new List<PreservedSpawn>();

        foreach (var spawn in current)
        {
            if (spawn is null) continue;
            if (_preservedSpawnsCaptured && written is not null && written.Contains(spawn)) continue;

            var classified = ClassifySpawn(map, spawn);
            if (classified is null) continue;
            if (alreadyPreserved.Any(p => p.Source == classified.Value.Source && IsSameSpawn(p.Spawn, spawn))) continue;

            var entry = new PreservedSpawn(classified.Value.Source, classified.Value.EventKey, cloner.Clone(spawn)!);
            preserved.Add(entry);
            added.Add(entry);
        }

        if (added.Count == 0) return;

        var summary = string.Join(", ", added
            .GroupBy(p => p.Source == PreservedSpawnSource.SeasonalEvent ? $"seasonal:{p.EventKey}" : p.Source.ToString())
            .Select(g => $"{g.Key} [{string.Join(", ", g.Select(p => p.Spawn.BossName).Distinct())}]"));
        logger.Info($"[ABPS] Found {added.Count} boss spawn(s) on {map} not managed by ABPS: {summary}");
    }

    /// <summary>
    /// Returns where a spawn came from, or null when ABPS manages it (and regenerates it itself).
    /// Seasonal event spawns are checked first, as some reuse a boss name ABPS manages (e.g. bossTagilla on Labs).
    /// </summary>
    private (PreservedSpawnSource Source, string? EventKey)? ClassifySpawn(string map, BossLocationSpawn spawn)
    {
        foreach (var (eventKey, spawnsByMap) in seasonalEventConfig.EventBossSpawns)
        {
            if (spawnsByMap is null || !spawnsByMap.TryGetValue(map, out var eventSpawns) || eventSpawns is null) continue;
            if (eventSpawns.Any(e => ReferenceEquals(e, spawn) || IsSameSpawn(e, spawn)))
                return (PreservedSpawnSource.SeasonalEvent, eventKey);
        }

        if (IsManagedByAbps(spawn)) return null;

        if (VanillaExtraSpawnsConfig.BossNames.Contains(spawn.BossName ?? string.Empty, StringComparer.OrdinalIgnoreCase))
            return (PreservedSpawnSource.VanillaExtra, null);

        return (PreservedSpawnSource.OtherMod, null);
    }

    private static bool IsSameSpawn(BossLocationSpawn a, BossLocationSpawn b)
    {
        return string.Equals(a.BossName, b.BossName, StringComparison.OrdinalIgnoreCase)
               && string.Equals(a.BossZone, b.BossZone, StringComparison.OrdinalIgnoreCase)
               && string.Equals(a.TriggerName ?? string.Empty, b.TriggerName ?? string.Empty, StringComparison.OrdinalIgnoreCase)
               && string.Equals(a.TriggerId ?? string.Empty, b.TriggerId ?? string.Empty, StringComparison.OrdinalIgnoreCase)
               && a.Time == b.Time
               && a.BossChance == b.BossChance;
    }

    /// <summary>
    /// True for the boss names ABPS generates itself: its PMCs and every boss in its BossConfig (enabled or not,
    /// so a boss the user disabled in ABPS is never brought back by preservation).
    /// </summary>
    internal static bool IsManagedByAbps(BossLocationSpawn spawn)
    {
        if (string.Equals(spawn.BossName, "pmcUSEC", StringComparison.OrdinalIgnoreCase)
            || string.Equals(spawn.BossName, "pmcBEAR", StringComparison.OrdinalIgnoreCase)) return true;

        foreach (var (boss, _) in ModConfig.Config.BossConfig)
        {
            if (string.Equals(spawn.BossName, boss, StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }
}