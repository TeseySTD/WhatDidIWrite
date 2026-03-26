namespace Wdiw.Infrastructure.Models;

public enum FileChangeStatus
{
    Added,
    Modified,
    Deleted,
    Renamed,
    Changed
}

public record FileChange(
    string Path,
    FileChangeStatus Status,
    string Content,
    long LinesAdded,
    long LinesDeleted
);

public record DiffResult(
    List<FileChange> Files,
    string RawSummary
)
{
    public bool IsEmpty => !Files.Any();
    public long TotalInsertions => Files.Sum(f => f.LinesAdded);
    public long TotalDeletions => Files.Sum(f => f.LinesDeleted);
}
