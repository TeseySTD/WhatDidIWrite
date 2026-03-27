using System.Text;
using Microsoft.Extensions.AI;
using Wdiw.Infrastructure.Abstractions;
using Wdiw.Infrastructure.Models;

namespace Wdiw.Infrastructure.Services;

public class UniversalAiProvider(IChatClient chatClient) : IAiProvider
{
    public async Task<string[]> GenerateCommitMessagesAsync(DiffResult diff, string style,
        CancellationToken ct = default)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System,
                $"""
                 You are an expert developer. Generate 3 distinct commit messages in '{style}' style.
                 Rules:
                 - Focus on 'why' and 'what' was changed.
                 - Provide ONLY the messages, each on a new line.
                 - No markdown, no numbers, no explanations.
                 """),
            new(ChatRole.User, BuildDiffContext(diff))
        };

        var response = await chatClient.GetResponseAsync(messages, cancellationToken: ct);

        var text = response.Messages.FirstOrDefault()?.Text;

        if (string.IsNullOrWhiteSpace(text))
            return [];

        return text.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
            .Select(m => m.Trim().TrimStart('-', '*', ' '))
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToArray();
    }

    private string BuildDiffContext(DiffResult diff)
    {
        var sb = new StringBuilder();
        foreach (var file in diff.Files)
        {
            sb.AppendLine($"File: {file.Path} ({file.Status})");
            sb.AppendLine(file.Content);
            sb.AppendLine();
        }

        return sb.ToString();
    }
}