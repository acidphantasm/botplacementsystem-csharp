using _botplacementsystem.Controllers;
using _botplacementsystem.Globals;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace _botplacementsystem.Routers;

[Injectable]
public class StaticRouters : StaticRouter
{
    private static JsonUtil _jsonUtil;
    private static HttpResponseUtil _httpResponseUtil;
    private static MapSpawns _mapSpawns;
    private static ISptLogger<StaticRouters> _logger;

    public static bool CacheRebuilt = false;
    public static string MapToRebuild = string.Empty;
    public static BossTrackingData? BossTrackingData = null;

    public StaticRouters(
        JsonUtil jsonUtil,
        HttpResponseUtil httpResponseUtil,
        MapSpawns mapSpawns,
        ISptLogger<StaticRouters> logger
    ) : base(
        jsonUtil,
        GetCustomRoutes()
    )
    {
        _jsonUtil = jsonUtil;
        _httpResponseUtil = httpResponseUtil;
        _mapSpawns = mapSpawns;
        _logger = logger;
        Load();
    }

    private static List<RouteAction> GetCustomRoutes()
    {
        return
        [
            new RouteAction("/client/match/local/start",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) =>
                {
                    var data = (StartLocalRaidRequestData)info;
                    RaidInformation.IsInRaid = true;
                    MapToRebuild = data.Location;
                    if (CacheRebuilt)
                    {
                        CacheRebuilt = false;
                    }

                    return output;
                },
                typeof(StartLocalRaidRequestData)
            ),
            new RouteAction("/client/match/local/end",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) =>
                {
                    RaidInformation.IsInRaid = false;
                    if (!CacheRebuilt)
                    {
                        _mapSpawns.ConfigureInitialData();
                        CacheRebuilt = true;
                    }

                    return output;
                },
                typeof(EndLocalRaidRequestData)
            ),
            new RouteAction("/abps/save",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await SaveBossTrackingData(info as BossTrackingData),
                typeof(BossTrackingData)
            ),
            new RouteAction("/abps/load",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => _jsonUtil.Serialize(BossTrackingData)
            )
        ];
    }

    private static async ValueTask<string> SaveBossTrackingData(BossTrackingData info)
    {
        if (info is null)
        {
            return null;
        }
        else
        {
            BossTrackingData = info;
        }

        await Save();
        return _jsonUtil.Serialize(new { success = true }); // FORKING DIGUSTING BUT CBA RN
    }

    private static async Task Save()
    {
        try
        {
            var filename = Path.Join(ModConfig._modPath, "bossTrackingData.json");
            await File.WriteAllTextAsync(filename, _jsonUtil.Serialize(BossTrackingData));
        }
        catch (Exception e)
        {
            _logger.Critical("[ABPS] Failed to save boss tracking data!");
            _logger.Critical(e.Message);
            throw;
        }
    }

    private static async Task Load()
    {
        try
        {
            var filename = Path.Join(ModConfig._modPath, "bossTrackingData.json");
            if (File.Exists(filename))
            {
                BossTrackingData =
                    _jsonUtil.DeserializeFromFile<BossTrackingData>(filename);
            }
            else
            {
                BossTrackingData = new BossTrackingData();
                await Save();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}

public record BossTrackingInfo
{
    public bool SpawnedLastRaid { get; set; }
    public int Chance { get; set; }
}

public class BossTrackingData : Dictionary<string, Dictionary<string, BossTrackingInfo>>;