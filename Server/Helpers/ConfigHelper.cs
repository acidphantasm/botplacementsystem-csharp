using System.Text;
using System.Text.Json;
using BotPlacementSystemServer.Models;

namespace BotPlacementSystemServer.Helpers;

public static class ConfigHelper
{
    private static readonly Dictionary<string, int> RequiredArrayLengths = new();

    public static bool IsJsonOutdated(string rawJson, string rawDefaultJson, AbpsConfig? config = null)
    {
        var (diskKeys, diskArrayLengths) = ParseDiskJson(rawJson);
        var defaultKeys = ParseDefaultJson(rawDefaultJson);

        var hasMissingKeys = defaultKeys.Any(k => !diskKeys.Contains(k));
        var hasExtraKeys = diskKeys.Any(k => !defaultKeys.Contains(k));
        var hasInvalidArrays = RequiredArrayLengths.Any(kvp => diskArrayLengths.TryGetValue(kvp.Key, out var diskLength) && diskLength != kvp.Value);

        if (config != null && hasMissingKeys)
        {
            var defaults = new AbpsConfig();
            foreach (var kvp in defaults.PmcDifficulty)
                config.PmcDifficulty.TryAdd(kvp.Key, kvp.Value);
            foreach (var kvp in defaults.BossDifficulty)
                config.BossDifficulty.TryAdd(kvp.Key, kvp.Value);
        }
        
        return hasMissingKeys || hasInvalidArrays || hasExtraKeys;
    }

    /// <summary>
    /// Bruh this is confusing as shit and I re-read this doc like 6 times, but it finally works
    /// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/use-utf8jsonreader
    /// </summary>
    /// <param name="json"></param>
    private static (HashSet<string> keyPaths, Dictionary<string, int> arrayLengths) ParseDiskJson(string json)
    {
        var keyPaths = new HashSet<string>();
        var arrayLengths = new Dictionary<string, int>();
        var pathStack = new Stack<string>();
        string? currentProperty = null;
        var arrayCountStack = new Stack<(string path, int count)>();
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    currentProperty = reader.GetString()!;
                    var path = pathStack.Count > 0
                        ? $"{string.Join(".", pathStack.Reverse())}.{currentProperty}"
                        : currentProperty;
                    keyPaths.Add(path);
                    break;
                case JsonTokenType.StartObject:
                    if (currentProperty != null)
                    {
                        pathStack.Push(currentProperty);
                        currentProperty = null;
                    }
                    break;
                case JsonTokenType.EndObject:
                    if (pathStack.Count > 0)
                        pathStack.Pop();
                    break;
                case JsonTokenType.StartArray:
                    if (currentProperty != null)
                    {
                        var arrayPath = pathStack.Count > 0
                            ? $"{string.Join(".", pathStack.Reverse())}.{currentProperty}"
                            : currentProperty;
                        if (RequiredArrayLengths.ContainsKey(arrayPath))
                            arrayCountStack.Push((arrayPath, 0));
                        currentProperty = null;
                    }
                    break;
                case JsonTokenType.EndArray:
                    if (arrayCountStack.Count > 0)
                    {
                        var (p, count) = arrayCountStack.Pop();
                        arrayLengths[p] = count;
                    }
                    break;
                case JsonTokenType.String:
                case JsonTokenType.Number:
                case JsonTokenType.True:
                case JsonTokenType.False:
                case JsonTokenType.Null:
                    if (arrayCountStack.Count > 0)
                    {
                        var (p, count) = arrayCountStack.Pop();
                        arrayCountStack.Push((p, count + 1));
                    }
                    break;
            }
        }
        return (keyPaths, arrayLengths);
    }
    
    /// <summary>
    /// Bruh this is confusing as shit and I re-read this doc like 6 times, but it finally works
    /// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/use-utf8jsonreader
    /// </summary>
    /// <param name="json"></param>
    private static HashSet<string> ParseDefaultJson(string json)
    {
        var keyPaths = new HashSet<string>();
        var pathStack = new Stack<string>();
        string? currentProperty = null;
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    currentProperty = reader.GetString()!;
                    var path = pathStack.Count > 0
                        ? $"{string.Join(".", pathStack.Reverse())}.{currentProperty}"
                        : currentProperty;
                    keyPaths.Add(path);
                    break;
                case JsonTokenType.StartObject:
                    if (currentProperty != null)
                    {
                        pathStack.Push(currentProperty);
                        currentProperty = null;
                    }
                    break;
                case JsonTokenType.EndObject:
                    if (pathStack.Count > 0)
                        pathStack.Pop();
                    break;
            }
        }
        return keyPaths;
    }
}