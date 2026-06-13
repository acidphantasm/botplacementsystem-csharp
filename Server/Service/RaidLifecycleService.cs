using BotPlacementSystemServer.Controllers;
using SPTarkov.DI.Annotations;

namespace BotPlacementSystemServer.Service;

[Injectable(InjectionType.Singleton)]
public class RaidLifecycleService
{
    public event Action<string>? RaidFinished;

    public bool CacheRebuilt { get; private set; } = false;
    public string CurrentMap { get; private set; } = string.Empty;

    public void RaidStarted(string map)
    {
        CurrentMap = map;
        CacheRebuilt = false;
    }

    public void RaidEnded()
    {
        if (CacheRebuilt)
            return;

        if (!string.IsNullOrWhiteSpace(CurrentMap))
        {
            RaidFinished?.Invoke(CurrentMap);
        }

        CacheRebuilt = true;
    }
}