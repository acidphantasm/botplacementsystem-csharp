using System.Reflection;
using acidphantasm_botplacementsystem.Utils;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace acidphantasm_botplacementsystem.Patches
{
    internal class PlayerOnDeadPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.OnDead));
        }

        [PatchPrefix]
        private static void PatchPrefix(Player __instance)
        {
            if (__instance == null || Utility.IsPlayerHeadless(__instance)) 
                return;
            
            if (!__instance.IsAI)
            {
                Utility.CachedConnectedPlayers.Remove(__instance);
                return;
            }
            if (__instance.Profile.Side is EPlayerSide.Bear or EPlayerSide.Usec)
            {
                Utility.CachedPmcs.Remove(__instance);
                return;
            }
            if (__instance.Profile.Info.Settings.Role is WildSpawnType.assault or WildSpawnType.assaultGroup)
            {
                Utility.CachedAssaultBots.Remove(__instance);
                return;
            }
            if (__instance.Profile.Info.Settings.IsBossOrFollower())
            {
                Utility.CachedBosses.Remove(__instance);
                return;
            }
        }
    }
}