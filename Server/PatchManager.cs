using _botplacementsystem.Patches;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace _botplacementsystem;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 50)]
public class PatchManager() : IOnLoad
{
    public Task OnLoad()
    {
        new AdjustWaves_Patch().Enable();
        new AdjustPMCSpawns_Patch().Enable();
        
        return Task.CompletedTask;
    }
}