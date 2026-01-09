using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using static acidphantasm_botplacementsystem.Spawning.BossSpawnTracking;

namespace acidphantasm_botplacementsystem.Patches
{
    internal class LocalGameProgressivePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BossSpawnScenario), nameof(BossSpawnScenario.smethod_0));
        }

        [PatchPrefix]
        private static void PatchPrefix(BossLocationSpawn[] bossWaves, Action<BossLocationSpawn> spawnBossAction)
        {
            if (!Plugin.progressiveChances && !Plugin.regressiveChances) return;
            
            foreach (var wave in bossWaves)
            {
                var currentBossLocationSpawn = wave;
                var bossName = currentBossLocationSpawn.BossType.ToString();

                if (!TrackedBosses.Contains(currentBossLocationSpawn.BossType)) continue;

                if (BossInfoForProfile.TryGetValue(bossName, out var info))
                {
                    var didBossSpawnLastRaid = info.SpawnedLastRaid;

                    if (didBossSpawnLastRaid)
                    {
                        if (Plugin.regressiveChances)
                        {
                            info.Chance = Math.Max(Plugin.minimumChance, info.Chance - Plugin.chanceStep);
                        }
                        else
                        {
                            info.Chance = Plugin.minimumChance;
                        }

                        info.SpawnedLastRaid = false;
                    }
                    else if (Plugin.progressiveChances)
                    {
                        info.Chance = Math.Min(Plugin.maximumChance, info.Chance + Plugin.chanceStep);
                    }
                    
                    currentBossLocationSpawn.BossChance = info.Chance;
                        
                    Plugin.LogSource.LogInfo($"Setting chance to {currentBossLocationSpawn.BossChance} for {bossName} - Did spawn last raid? {didBossSpawnLastRaid}");
                }
                else
                {
                    Plugin.LogSource.LogInfo($"Setting chance to {Plugin.minimumChance} for {bossName}");
                    CustomizedObject values = new CustomizedObject();
                    values.SpawnedLastRaid = false;
                    values.Chance = Plugin.minimumChance;
                    BossInfoForProfile.Add(bossName, values);
                    currentBossLocationSpawn.BossChance = values.Chance;
                }
            }
        }
    }
}
