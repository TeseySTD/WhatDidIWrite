using System.Diagnostics;
using System.Text;
using Wdiw.Infrastructure.Abstractions;
using Wdiw.Infrastructure.Models;

namespace Wdiw.Infrastructure.Services;

public class GitCliService : IGitService
{
    private string? _gitRoot;

    public async Task<DiffResult> GetStagedChangesAsync()
    {
        _gitRoot ??= await GetGitRootAsync();

        var statusTask = ExecuteGitAsync("status", "--porcelain", "-z");
        var statsTask = GetNumstatMapAsync();
        var fullDiffTask = ExecuteGitAsync("diff", "--cached");

        await Task.WhenAll(statusTask, statsTask, fullDiffTask);

        var statusRaw = await statusTask;
        var statsMap = await statsTask;
        var fullDiff = await fullDiffTask;

        var diffsByFile = ParseBulkDiff(fullDiff);

        var files = new List<FileChange>();
        var entries = statusRaw.Split('\0');

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry.Length < 4) continue;

            char stagedStatus = entry[0];
            if (stagedStatus == ' ' || stagedStatus == '?') continue;

            var path = entry[3..];
            if (stagedStatus == 'R' || stagedStatus == 'C') i++;

            if (IsExcluded(path)) continue;

            diffsByFile.TryGetValue(path, out var fileContent);
            statsMap.TryGetValue(path, out var stats);

            files.Add(new FileChange(
                path,
                MapStatus(stagedStatus),
                fileContent ?? "[No content]",
                stats.Added,
                stats.Deleted));
        }

        return new DiffResult(files, "Staged analysis complete (optimized)");
    }

    private Dictionary<string, string> ParseBulkDiff(string fullDiff)
    {
        var map = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(fullDiff)) return map;

        var parts = fullDiff.Split(["diff --git "], StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var fullPart = "diff --git " + part;
            var lines = fullPart.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) continue;

            var header = lines[0];
            int bStart = header.LastIndexOf(" b/", StringComparison.Ordinal);
            if (bStart == -1) continue;

            var path = header[(bStart + 3)..].Trim().Trim('"');
            map[path] = fullPart;
        }

        return map;
    }

    private async Task<string> GetGitRootAsync()
    {
        var res = await ExecuteGitAsync("rev-parse", "--show-toplevel");
        return string.IsNullOrWhiteSpace(res) ? Directory.GetCurrentDirectory() : res;
    }

    private async Task<string> ExecuteGitAsync(params string[] args)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "git",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            WorkingDirectory = _gitRoot ?? Directory.GetCurrentDirectory()
        };

        foreach (var arg in args) process.StartInfo.ArgumentList.Add(arg);

        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var output = await outputTask;
        var error = await errorTask;

        if (!string.IsNullOrWhiteSpace(error) && string.IsNullOrWhiteSpace(output))
            return $"GIT ERROR: {error.Trim()}";

        return output.Trim();
    }

    private async Task<Dictionary<string, (long Added, long Deleted)>> GetNumstatMapAsync()
    {
        var output = await ExecuteGitAsync("diff", "--cached", "--numstat", "-z");
        var map = new Dictionary<string, (long, long)>();

        var parts = output.Split('\0', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < parts.Length; i++)
        {
            var statLine = parts[i];
            var stats = statLine.Split('\t', StringSplitOptions.RemoveEmptyEntries);
            if (stats.Length < 2) continue;

            long.TryParse(stats[0], out var a);
            long.TryParse(stats[1], out var d);

            string path;
            if (stats.Length > 2)
            {
                path = stats[2]; 
            }
            else
            {
                i++; // Skip old_path
                path = parts[++i];
            }

            map[path] = (a, d);
        }

        return map;
    }


    public async Task<bool> IsGitRepositoryAsync()
    {
        var res = await ExecuteGitAsync("rev-parse", "--is-inside-work-tree");
        return res.Contains("true");
    }

    private bool IsExcluded(string path) =>
        path.Contains(".idea/") || path.Contains(".vs/") || path.EndsWith(".user") || path.Contains("bin/") ||
        path.Contains("obj/");

    private FileChangeStatus MapStatus(char s) => s switch
    {
        'A' => FileChangeStatus.Added,
        'M' => FileChangeStatus.Modified,
        'D' => FileChangeStatus.Deleted,
        'R' => FileChangeStatus.Renamed,
        _ => FileChangeStatus.Changed
    };

    public Task<bool> CommitAsync(string msg) => Task.FromResult(true);
}