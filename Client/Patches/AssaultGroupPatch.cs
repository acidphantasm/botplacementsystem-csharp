using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace acidphantasm_botplacementsystem.Patches
{
    internal class AssaultGroupPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProfileInfoSettingsClass), nameof(ProfileInfoSettingsClass.TryChangeRoleToAssaultGroup));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ProfileInfoSettingsClass __instance)
        {
            if(__instance.Role == WildSpawnType.assaultGroup)
            {
                __instance.Role = WildSpawnType.assault;
            }
            return false;
        }
    }
}

