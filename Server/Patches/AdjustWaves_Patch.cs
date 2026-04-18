using System.Reflection;
using _botplacementsystem.Controllers;
using HarmonyLib;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Location;
using SPTarkov.Server.Core.Services;

namespace _botplacementsystem.Patches;

public class AdjustWaves_Patch : AbstractPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(RaidTimeAdjustmentService),"AdjustWaves");
    }

    [PatchPrefix]
    public static bool Prefix(LocationBase mapBase, RaidChanges raidAdjustments)
    {
        var pmcSpawns = ServiceLocator.ServiceProvider.GetService<PmcSpawns>();
        var scavSpawns = ServiceLocator.ServiceProvider.GetService<ScavSpawns>();
        var locationName = mapBase.Id.ToLowerInvariant();

        var simulatedStart = raidAdjustments.SimulatedRaidStartSeconds ?? 0d;
        var totalRaidSeconds = (raidAdjustments.RaidTimeMinutes ?? 0d) * 60;
        var remainingTime = totalRaidSeconds - simulatedStart;

        if (simulatedStart > 60d)
        {
            var mapBosses = mapBase.BossLocationSpawn
                .Where(x => x.Time == -1
                            && !string.Equals(x.BossName, "pmcUSEC", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(x.BossName, "pmcBEAR", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var newPmcs = pmcSpawns.GenerateScavRaidRemainingPmcs(locationName, remainingTime);
            mapBase.BossLocationSpawn = newPmcs;

            foreach (var boss in mapBosses)
                mapBase.BossLocationSpawn.Add(boss);

            mapBase.Waves = scavSpawns.GetLateStartMapData(locationName);
        }

        return false;
    }
}