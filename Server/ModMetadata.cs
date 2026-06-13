using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web;

namespace BotPlacementSystemServer;

public record ModMetadata : AbstractModMetadata, IModWebMetadata
{
    public override string ModGuid { get; init; } = "com.acidphantasm.botplacementsystem";
    public override string Name { get; init; } = "Acid's Bot Placement System";
    public override string Author { get; init; } = "acidphantasm";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("2.0.19");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.3");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string? License { get; init; } = "BY-NC-ND 4.0";
}