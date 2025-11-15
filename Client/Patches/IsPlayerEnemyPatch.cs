using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using acidphantasm_botplacementsystem.Spawning;
using acidphantasm_botplacementsystem.Utils;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace acidphantasm_botplacementsystem.Patches;

internal class IsPlayerEnemyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.IsPlayerEnemy));
    }

    [PatchPrefix]
    private static bool PatchPrefix(BotsGroup __instance, IPlayer player, ref bool __result)
    {
        if (player.IsAI && player.Profile.Info.Settings.Role is WildSpawnType.pmcBEAR or WildSpawnType.pmcUSEC)
        {
            string leaderId = __instance.InitialBot.Profile.ProfileId;
            string thisBotId = player.Profile.ProfileId;
            
            // Check our group mappings - as we spawn bots faster than the group manager can assign groups and handle them
            if (PmcGroupSpawner.allPmcGroups.TryGetValue(leaderId, out var followers))
            {
                if (followers.Contains(thisBotId))
                {
                    __result = false;
                    return false;
                }
            }
            
            if (__instance.InitialBot.BotsGroup.Contains(player.AIData.BotOwner))
            {
                __result = false;
                return false;
            }

            __result = true;
            return false;
        }

        return true;
    }
}