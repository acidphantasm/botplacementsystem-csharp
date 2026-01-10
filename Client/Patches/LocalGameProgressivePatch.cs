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

                if (currentBossLocationSpawn.BossChance >= 99.9f)
                {
                    Plugin.LogSource.LogInfo($"{bossName} is 100% chance. Skipping progressive/regressive changes. Probably a weekly boss? Or you set it to 100% in the config.");
                    continue;
                }
                
                if (BossInfoForProfile.TryGetValue(bossName, out var info))
                {
                    var didBossSpawnLastRaid = info.SpawnedLastRaid;

                    if (didBossSpawnLastRaid)
                    {
                        info.Chance = Plugin.regressiveChances ? Math.Max(Plugin.minimumChance, info.Chance - Plugin.chanceStep) : Plugin.minimumChance;

                        currentBossLocationSpawn.BossChance = info.Chance;
                        Plugin.LogSource.LogInfo($"{bossName} spawned last raid. New Chance: {currentBossLocationSpawn.BossChance}");
                    }
                    else if (Plugin.progressiveChances)
                    {
                        info.Chance = Math.Min(Plugin.maximumChance, info.Chance + Plugin.chanceStep);
                        currentBossLocationSpawn.BossChance = info.Chance;
                        Plugin.LogSource.LogInfo($"{bossName} did not spawn last raid. New Chance: {currentBossLocationSpawn.BossChance}");
                    }
                    
                    info.SpawnedLastRaid = false;
                }
                else
                {
                    var initialChance = (int)Math.Round(currentBossLocationSpawn.BossChance);

                    Plugin.LogSource.LogInfo($"Storing {bossName} with existing chance {initialChance}");

                    CustomizedObject values = new CustomizedObject
                    {
                        SpawnedLastRaid = false,
                        Chance = initialChance
                    };

                    BossInfoForProfile.Add(bossName, values);
                }
            }
        }
    }
}
