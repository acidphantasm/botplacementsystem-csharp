using System.Reflection;
using acidphantasm_botplacementsystem.Utils;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace acidphantasm_botplacementsystem.Patches
{
    internal class BotOwnerCreationPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.Create));
        }

        [PatchPostfix]
        private static void PatchPostfix(Player player)
        {
            if (Utility.IsPlayerHeadless(player) || !player.IsAI)
            {
                Plugin.LogSource.LogInfo($"Player hitting botOwner.Create is a player or headless");
                return;
            }
            
            if (player.Profile.Side is EPlayerSide.Bear or EPlayerSide.Usec)
            {
                Utility.CachedPmcs.Add(player);
                return;
            }
            if (player.Profile.Info.Settings.Role is WildSpawnType.assault)
            {
                Utility.CachedAssaultBots.Add(player);
                return;
            }
            if (player.Profile.Info.Settings.IsBossOrFollower())
            {
                Utility.CachedBosses.Add(player);
                return;
            }
        }
    }
}