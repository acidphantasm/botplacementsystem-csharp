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
        if (raidAdjustments.SimulatedRaidStartSeconds > 60)
        {
            var mapBosses = mapBase.BossLocationSpawn.Where((x) =>
                x.Time == -1 && x.BossName != "pmcUSEC" && x.BossName != "pmcBEAR");

            mapBase.BossLocationSpawn = mapBase.BossLocationSpawn.Where((x) =>
                x.Time > raidAdjustments.SimulatedRaidStartSeconds &&
                (x.BossName == "pmcUSEC" || x.BossName == "pmcBEAR")).ToList();

            foreach (var bossWave in mapBase.BossLocationSpawn)
            {
                bossWave.Time -= Math.Max(raidAdjustments.SimulatedRaidStartSeconds.GetValueOrDefault(), 0);
            }

            var totalRemainingTime = raidAdjustments.RaidTimeMinutes * 60;
            var newStartingPMCs = pmcSpawns.GenerateScavRaidRemainingPMCs(locationName,
                totalRemainingTime.GetValueOrDefault());

            foreach (var spawn in newStartingPMCs)
            {
                mapBase.BossLocationSpawn.Add(spawn);
            }

            foreach (var spawn in mapBosses)
            {
                mapBase.BossLocationSpawn.Add(spawn);
            }

            var newStartingScavs = scavSpawns.GenerateStartingScavs(locationName, "assault", true, mapBase.Waves.Count);

            foreach (var spawn in newStartingScavs)
            {
                mapBase.Waves.Add(spawn);
            }
        }
        
        return false;
    }
}