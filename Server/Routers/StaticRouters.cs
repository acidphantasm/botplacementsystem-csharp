using System.Reflection;
using BotPlacementSystemServer.Controllers;
using BotPlacementSystemServer.Models;
using BotPlacementSystemServer.Service;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace BotPlacementSystemServer.Routers;

[Injectable(TypePriority = OnLoadOrder.PreSptModLoader)]
public class StaticRouters : StaticRouter
{
    private static JsonUtil _jsonUtil;
    private static HttpResponseUtil _httpResponseUtil;
    private static RaidLifecycleService _raidLifecycleService;
    private static string? _modPath;
    private static string? _savesPath;
    private static ISptLogger<StaticRouters> _logger;

    private static Dictionary<string, Dictionary<string, CustomizedObject>>? _bossTrackingData = null;

    public StaticRouters(
        JsonUtil jsonUtil,
        HttpResponseUtil httpResponseUtil,
        ModHelper modHelper,
        RaidLifecycleService raidLifecycleService,
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
        _logger = logger;
        _raidLifecycleService = raidLifecycleService;
        _ = Load();
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
                    _raidLifecycleService.RaidStarted(data.Location.ToLowerInvariant());

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
                    if (!_raidLifecycleService.CacheRebuilt && !string.IsNullOrEmpty(_raidLifecycleService.CurrentMap))
                    {
                        _raidLifecycleService.RaidEnded();
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
                ) => await new ValueTask<string>(_jsonUtil.Serialize(_bossTrackingData))
            )
        ];
    }

    private static ValueTask<string> SaveBossTrackingData(BossTrackingStats info)
    {
        var profileId = info.ProfileId;
        _bossTrackingData[profileId] = info.Data;

        Task.Run(() => Save(profileId));
        return new ValueTask<string>(_httpResponseUtil.NullResponse());
    }

    private static async Task Save(string profileId)
    {
        try
        {
            if (!Directory.Exists(_savesPath))
                Directory.CreateDirectory(_savesPath);
            
            if (!_bossTrackingData.TryGetValue(profileId, out var data))
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
            _bossTrackingData = new Dictionary<string, Dictionary<string, CustomizedObject>>();
            
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

                    _bossTrackingData[profileId] = data;
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