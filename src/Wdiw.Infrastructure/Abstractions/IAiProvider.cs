using Wdiw.Infrastructure.Models;

namespace Wdiw.Infrastructure.Abstractions;

public interface IAiProvider
{
    Task<string[]> GenerateCommitMessagesAsync(DiffResult diff, string style, CancellationToken ct = default);
}