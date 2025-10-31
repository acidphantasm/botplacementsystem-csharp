using System.Reflection;
using _botplacementsystem.Controllers;
using _botplacementsystem.Globals;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace _botplacementsystem.Routers;

[Injectable]
public class StaticRouters : StaticRouter
{
    private static JsonUtil _jsonUtil;
    private static HttpResponseUtil _httpResponseUtil;
    private static string? _modPath;
    private static string? _savesPath;
    private static MapSpawns _mapSpawns;
    private static ISptLogger<StaticRouters> _logger;

    public static bool CacheRebuilt = false;
    public static string MapToRebuild = string.Empty;
    public static Dictionary<string, Dictionary<string, CustomizedObject>>? BossTrackingData = null;

    public StaticRouters(
        JsonUtil jsonUtil,
        HttpResponseUtil httpResponseUtil,
        ModHelper modHelper,
        MapSpawns mapSpawns,
        ISptLogger<StaticRouters> logger
    ) : base(
        jsonUtil,
        GetCustomRoutes()
    )
    {
        _jsonUtil = jsonUtil;
        _httpResponseUtil = httpResponseUtil;
        _modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());;
        _savesPath = Path.Join(_modPath, "Data");
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
            new RouteAction<BossTrackingStats>("/botplacementsystem/save",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await SaveBossTrackingData(info)
            ),
            new RouteAction("/botplacementsystem/load",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await new ValueTask<string>(_jsonUtil.Serialize(BossTrackingData))
            )
        ];
    }

    private static ValueTask<string> SaveBossTrackingData(BossTrackingStats info)
    {
        var profileId = info.ProfileId;
        BossTrackingData[profileId] = info.Data;

        Task.Run(() => Save(profileId));
        return new ValueTask<string>(_httpResponseUtil.NullResponse());
    }

    public static async Task Save(string profileId)
    {
        try
        {
            if (!Directory.Exists(_savesPath))
                Directory.CreateDirectory(_savesPath);
            
            if (!BossTrackingData.TryGetValue(profileId, out var data))
            {
                _logger.Warning($"No for profile '{profileId}', skipping");
                return;
            }
            
            var dataToSave = _jsonUtil.Serialize(data, indented: true);
            
            var filename = Path.Join(_savesPath, $"{profileId}.json");
            await File.WriteAllTextAsync(filename, dataToSave);
        }
        catch (Exception e)
        {
            _logger.Critical(e.Message);
            throw;
        }
    }

    private static async Task Load()
    {
        try
        {
            BossTrackingData = new Dictionary<string, Dictionary<string, CustomizedObject>>();
            
            if (!Directory.Exists(_savesPath))
            {
                Directory.CreateDirectory(_savesPath);
                return;
            }

            var profileFilePaths = Directory.EnumerateFiles(_savesPath, "*.json", SearchOption.TopDirectoryOnly);

            foreach (var filePath in profileFilePaths)
            {
                var fullPath = Path.GetFullPath(filePath);
                var profileId = Path.GetFileNameWithoutExtension(fullPath);

                try
                {
                    var data = await _jsonUtil.DeserializeFromFileAsync<Dictionary<string, CustomizedObject>>(filePath);

                    if (data is null)
                    {
                        _logger.Warning($"Skipping '{profileId}' — JSON empty or unreadable.");
                        continue;
                    }

                    BossTrackingData[profileId] = data;
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Failed to load profile '{profileId}' from '{fullPath}' : {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warning($"Failed to load StatTrack Profiles: {ex.Message}");
        }
    }
}

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