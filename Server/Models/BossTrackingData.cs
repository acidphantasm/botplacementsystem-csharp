using SPTarkov.Server.Core.Models.Utils;

namespace BotPlacementSystemServer.Models;

public record CustomizedObject
{
    public bool SpawnedLastRaid { get; set; }
    public int Chance { get; set; }
}

public record BossTrackingStats : IRequestData
{
    public Dictionary<string, CustomizedObject> Data { get; set; }
    public string ProfileId { get; set; }
}