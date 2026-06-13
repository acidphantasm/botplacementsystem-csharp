using System.Reflection;
using System.Text.Json;
using BotPlacementSystemServer.Controllers;
using BotPlacementSystemServer.Helpers;
using BotPlacementSystemServer.Models;
using BotPlacementSystemServer.Models.Enums;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Loaders;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace BotPlacementSystemServer.Globals;

[Injectable (InjectionType.Singleton, TypePriority = OnLoadOrder.PreSptModLoader)]
public class ModConfig : IOnLoad
{
    private static JsonUtil _jsonUtil;
    private static FileUtil _fileUtil;
    private static ISptLogger<ModConfig> _logger;
    
    public static MapZoneDefaults? MapZoneDefaults { get; private set; }
    public static BossWaveDefaults? BossWaveDefaults { get; private set; }
    public static PmcDefaults? PmcDefaults { get; private set; }
    public static ScavDefaults? ScavDefaults { get; private set; }
    public static HostilityDefaults? HostilityDefaults { get; private set; }
    public static AbpsConfig Config {get; private set;} = null!;
    public static AbpsConfig OriginalConfig {get; private set;} = null!;
    
    private static int _isActivelyProcessingFlag = 0;
    
    public static string? _modPath;
    
    public static event Action? ConfigChanged;

    public ModConfig(
        JsonUtil jsonUtil,
        FileUtil fileUtil,
        ModHelper modHelper,
        ISptLogger<ModConfig> logger)
    {
        _modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        _jsonUtil = jsonUtil;
        _fileUtil = fileUtil;
        _logger = logger;
    }
    
    public async Task OnLoad()
    {
        var configPath = Path.Combine(_modPath, "config.json");
        var defaultConfigPath = Path.Combine(_modPath, "Defaults", "config.default.json");
        
        if (!File.Exists(configPath))
        {
            File.Copy(defaultConfigPath, configPath);
        }
        
        var rawConfig = await _fileUtil.ReadFileAsync(configPath);
        var rawDefaultConfig = await _fileUtil.ReadFileAsync(defaultConfigPath);
        
        Config = _jsonUtil.Deserialize<AbpsConfig>(rawConfig) ?? throw new ArgumentNullException();
        
        if (ConfigHelper.IsJsonOutdated(rawConfig, rawDefaultConfig, Config))
        {
            await _fileUtil.WriteFileAsync(configPath, _jsonUtil.Serialize(Config, true)!);
            _logger.Success("[ABPS] Config updated and/or repaired.");
        }
        
        OriginalConfig = DeepClone(Config);
        
        MapZoneDefaults = await _jsonUtil.DeserializeFromFileAsync<MapZoneDefaults>(_modPath + "/Defaults/MapZones.json") ?? throw new ArgumentNullException();
        BossWaveDefaults = await _jsonUtil.DeserializeFromFileAsync<BossWaveDefaults>(_modPath + "/Defaults/Bosses.json") ?? throw new ArgumentNullException();
        PmcDefaults = await _jsonUtil.DeserializeFromFileAsync<PmcDefaults>(_modPath + "/Defaults/PMCs.json") ?? throw new ArgumentNullException();
        ScavDefaults = await _jsonUtil.DeserializeFromFileAsync<ScavDefaults>(_modPath + "/Defaults/Scavs.json") ?? throw new ArgumentNullException();
        HostilityDefaults = await _jsonUtil.DeserializeFromFileAsync<HostilityDefaults>(_modPath + "/Defaults/Hostility.json") ?? throw new ArgumentNullException();
    }
    
    public static async Task<ConfigOperationResult> ReloadConfig()
    {
        if (Interlocked.CompareExchange(ref _isActivelyProcessingFlag, 1, 0) != 0)
            return ConfigOperationResult.ActiveProcess;

        try
        {
            var configPath = Path.Combine(_modPath, "config.json");
            var configTask = _jsonUtil.DeserializeFromFileAsync<AbpsConfig>(configPath);

            await Task.WhenAll(configTask);

            Config = configTask.Result ?? throw new ArgumentNullException(nameof(Config));
            OriginalConfig = DeepClone(Config);

            ConfigChanged?.Invoke();

            return ConfigOperationResult.Success;
        }
        catch (Exception ex)
        {
            return ConfigOperationResult.Failure;
        }
        finally
        {
            Interlocked.Exchange(ref _isActivelyProcessingFlag, 0);
        }
    }
    
    public static async Task<ConfigOperationResult> SaveConfig()
    {
        if (Interlocked.CompareExchange(ref _isActivelyProcessingFlag, 1, 0) != 0)
            return ConfigOperationResult.ActiveProcess;

        try
        {
            var configPath = Path.Combine(_modPath, "config.json");

            var serializedConfigTask = Task.Run(() => _jsonUtil.Serialize(Config, true));
            await Task.WhenAll(serializedConfigTask);

            var writeConfigTask = _fileUtil.WriteFileAsync(configPath, serializedConfigTask.Result!);
            await Task.WhenAll(writeConfigTask);

            ConfigChanged?.Invoke();
            
            // Update 'Original' config stuff since we've saved so the 'Undo' function works
            OriginalConfig = DeepClone(Config);

            return ConfigOperationResult.Success;
        }
        catch (Exception ex)
        {
            return ConfigOperationResult.Failure;
        }
        finally
        {
            Interlocked.Exchange(ref _isActivelyProcessingFlag, 0);
        }
    }
    
    private static T DeepClone<T>(T source)
    {
        var json = _jsonUtil.Serialize(source);
        return _jsonUtil.Deserialize<T>(json)!;
    }
}