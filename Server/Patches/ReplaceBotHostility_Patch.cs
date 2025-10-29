using System.Reflection;
using HarmonyLib;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Services;

namespace _botplacementsystem.Patches;

public class ReplaceBotHostility_Patch: AbstractPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(SeasonalEventService),"ReplaceBotHostility");
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}