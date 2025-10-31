using System.Text.Json.Serialization;

namespace _progressiveBotSystem.Models;

public class ReleaseInformation
{
    [JsonPropertyName("tag_name")]
    public required string Version { get; init; }

    [JsonPropertyName("html_url")]
    public required string DownloadUrl { get; init; }

    [JsonPropertyName("published_at")]
    public required DateTime ReleaseDate { get; init; }
}