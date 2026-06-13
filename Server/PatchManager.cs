using BotPlacementSystemServer.Patches;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace BotPlacementSystemServer;

[Injectable(TypePriority = OnLoadOrder.PreSptModLoader + 50)]
public class PatchManager() : IOnLoad
{
    public Task OnLoad()
    {
        new AdjustWaves_Patch().Enable();
        new AdjustPmcSpawns_Patch().Enable();
        new ReplaceBotHostility_Patch().Enable();
        
        return Task.CompletedTask;
    }
}