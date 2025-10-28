using acidphantasm_botplacementsystem.Spawning;
using Comfort.Common;
using HarmonyLib;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace acidphantasm_botplacementsystem.Patches
{
    public class MenuLoadPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            Type type = PatchConstants.EftTypes.Single(
                t => !t.IsAbstract &&
                typeof(ProfileEndpointFactoryAbstractClass).IsAssignableFrom(t) &&
                t.GetMethod("RequestBuilds") != null);
            return AccessTools.Method(type, "RequestBuilds");
        }

        [PatchPostfix]
        public static async void Postfix(Task<IResult> __result)
        {
            await __result;
            BossSpawnTracking.LoadFromServer();
        }
    }
}
