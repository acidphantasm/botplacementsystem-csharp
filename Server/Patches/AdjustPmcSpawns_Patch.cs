using System.Reflection;
using _botplacementsystem.Controllers;
using HarmonyLib;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Location;
using SPTarkov.Server.Core.Services;

namespace _botplacementsystem.Patches;

public class AdjustPmcSpawns_Patch: AbstractPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(RaidTimeAdjustmentService),"AdjustPMCSpawns");
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}