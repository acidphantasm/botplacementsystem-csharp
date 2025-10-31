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
            if (Plugin.progressiveChances)
            {
                foreach (var wave in bossWaves)
                {
                    BossLocationSpawn currentBossSpawn = wave;
                    string bossName = currentBossSpawn.BossType.ToString();

                    if (!TrackedBosses.Contains(currentBossSpawn.BossType)) continue;

                    float chance = currentBossSpawn.BossChance;

                    if (BossInfoForProfile.ContainsKey(bossName))
                    {
                        int newChance = 0;
                        bool didBossSpawnLastRaid = BossInfoForProfile[bossName].SpawnedLastRaid;

                        if (didBossSpawnLastRaid)
                        {
                            BossInfoForProfile[bossName].SpawnedLastRaid = false;
                            newChance = Plugin.minimumChance;
                        }
                        else
                        {
                            if (BossInfoForProfile[bossName].Chance + Plugin.chanceStep > Plugin.maximumChance)
                            {
                                BossInfoForProfile[bossName].Chance = Plugin.maximumChance;
                                newChance = Plugin.maximumChance;
                            }
                            else
                            {
                                BossInfoForProfile[bossName].Chance += Plugin.chanceStep;
                                newChance = BossInfoForProfile[bossName].Chance;
                            }
                        }

                        chance = newChance;
                        Plugin.LogSource.LogInfo($"Setting chance to {newChance} for {bossName}");
                    }
                    else
                    {
                        Plugin.LogSource.LogInfo($"Setting chance to {Plugin.minimumChance} for {bossName}");
                        CustomizedObject values = new CustomizedObject();
                        values.SpawnedLastRaid = false;
                        values.Chance = Plugin.minimumChance;
                        BossInfoForProfile.Add(bossName, values);
                        chance = values.Chance;
                    }
                }
            }
        }
    }
}
