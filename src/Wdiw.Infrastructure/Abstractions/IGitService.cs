using Wdiw.Infrastructure.Models;

namespace Wdiw.Infrastructure.Abstractions;

public interface IGitService
{
    Task<DiffResult> GetStagedChangesAsync();

    Task<bool> CommitAsync(string message);

    Task<bool> IsGitRepositoryAsync();
}