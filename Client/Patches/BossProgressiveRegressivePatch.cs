using System;
using System.Reflection;
using System.Threading;
using acidphantasm_botplacementsystem.Spawning;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace acidphantasm_botplacementsystem.Patches;

internal class BossProgressiveRegressivePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotSpawner), nameof(BotSpawner.method_10));
    }

    [PatchPostfix]
    private static void PatchPostfix(BotZone zone, BotCreationDataClass data, Action<BotOwner> callback, CancellationToken cancellationToken)
    {
        if (!data.Profiles[0].Info.Settings.Role.IsBoss())
        {
            return;
        }
        
        var bossName = data.Profiles[0].Info.Settings.Role;
        if (!BossSpawnTracking.TrackedBosses.Contains(bossName) || (!Plugin.ProgressiveChances && !Plugin.RegressiveChances))
        {
            return;
        }
        Logger.LogInfo($"Saving boss as spawned: {bossName}");
        BossSpawnTracking.UpdateBossSpawnChance(bossName);
    }
}