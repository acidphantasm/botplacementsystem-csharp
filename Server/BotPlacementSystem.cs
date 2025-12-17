using _botplacementsystem.Controllers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Web;

namespace _botplacementsystem;

public record ModMetadata : AbstractModMetadata, IModWebMetadata
{
    public override string ModGuid { get; init; } = "com.acidphantasm.botplacementsystem";
    public override string Name { get; init; } = "Acid's Bot Placement System";
    public override string Author { get; init; } = "acidphantasm";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("2.0.9");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.3");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string? License { get; init; } = "BY-NC-ND 4.0";
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 69420)]
public class BotPlacementSystem(
    ISptLogger<BotPlacementSystem> logger,
    MapSpawns mapSpawns)
    : IOnLoad
{
   
    public Task OnLoad()
    {
        mapSpawns.ConfigureInitialData();
        
        return Task.CompletedTask;
    }
}
