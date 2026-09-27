using System.Reflection;
using BotPlacementSystemServer.Controllers;
using HarmonyLib;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Location;

namespace BotPlacementSystemServer.Patches;

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Services.InRaid;

[Injectable]
public class AdjustWavesPatch : AbstractPatch
{
    private static PmcSpawns _pmcSpawns = null!;
    private static ScavSpawns _scavSpawns = null!;

    public AdjustWavesPatch(PmcSpawns pmcSpawns, ScavSpawns scavSpawns)
    {
        _pmcSpawns = pmcSpawns;
        _scavSpawns = scavSpawns;
    }
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(RaidTimeAdjustmentService),"AdjustWaves");
    }

    [PatchPrefix]
    public static bool Prefix(LocationBase mapBase, RaidChanges raidAdjustments)
    {
        var locationName = mapBase.Id.ToLowerInvariant();

        var simulatedStart = raidAdjustments.SimulatedRaidStartSeconds ?? 0d;
        var totalRaidSeconds = (raidAdjustments.RaidTimeMinutes ?? 0d) * 60;
        
        if (simulatedStart > 60d)
        {
            var mapBosses = mapBase.BossLocationSpawn
                .Where(x => (x.Time == -1
                             && !string.Equals(x.BossName, "pmcUSEC", StringComparison.OrdinalIgnoreCase)
                             && !string.Equals(x.BossName, "pmcBEAR", StringComparison.OrdinalIgnoreCase))
                            || MapSpawns.IsCustomBotTypeSpawn(x))
                .ToList();

            var newPmcs = _pmcSpawns.GenerateScavRaidRemainingPmcs(locationName, totalRaidSeconds);
            mapBase.BossLocationSpawn = newPmcs;

            foreach (var boss in mapBosses)
                mapBase.BossLocationSpawn.Add(boss);

            mapBase.Waves = _scavSpawns.GetLateStartMapData(locationName, totalRaidSeconds);
        }

        return false;
    }
}