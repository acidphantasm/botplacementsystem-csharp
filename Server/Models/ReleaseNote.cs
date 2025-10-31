namespace _progressiveBotSystem.Models;

public class ReleaseNote
{
    public required string Version { get; init; }
    public required string ReleaseDate { get; init; }
    public required bool IsLatest { get; init; }
    public List<string>? NewFeatures { get; init; }
    public List<string>? Changes { get; init; }
    public List<string>? BugFixes { get; init; }
    public List<string>? KnownIssues { get; init; }
}