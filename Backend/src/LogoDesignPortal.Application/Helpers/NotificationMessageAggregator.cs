using System.Text.RegularExpressions;

namespace LogoDesignPortal.Application.Helpers;

/// <summary>
/// Transforms notification messages for aggregation display.
/// Example: "Preview delivered for order #ORD-1024" -> "3 previews delivered for order #ORD-1024"
/// </summary>
public static class NotificationMessageAggregator
{
    /// <summary>
    /// Builds an aggregated message for the given count and original message.
    /// </summary>
    public static string BuildAggregatedMessage(string originalMessage, int count)
    {
        if (string.IsNullOrWhiteSpace(originalMessage) || count <= 1)
        {
            return originalMessage ?? string.Empty;
        }

        // Match first word (noun/verb) to pluralize
        var match = Regex.Match(originalMessage.Trim(), @"^(\w+)(\s+.*)?$");
        if (!match.Success)
        {
            return $"{count} {originalMessage}";
        }

        var firstWord = match.Groups[1].Value;
        var rest = match.Groups[2].Success ? match.Groups[2].Value : string.Empty;

        var pluralized = Pluralize(firstWord);
        return $"{count} {pluralized}{rest}";
    }

    private static string Pluralize(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;

        var lower = word.ToLowerInvariant();

        // Common irregular plurals
        if (lower == "preview") return "previews";
        if (lower == "file") return "files";
        if (lower == "message") return "messages";
        if (lower == "invoice") return "invoices";
        if (lower == "order") return "orders";
        if (lower == "revision") return "revisions";
        if (lower == "upload") return "uploads";
        if (lower == "update") return "updates";
        if (lower == "change") return "changes";

        // Standard rules
        if (lower.EndsWith("s") || lower.EndsWith("x") || lower.EndsWith("ch") || lower.EndsWith("sh"))
        {
            return word + "es";
        }
        if (lower.EndsWith("y") && word.Length > 1 && !IsVowel(word[^2]))
        {
            return word[..^1] + "ies";
        }

        return word + "s";
    }

    private static bool IsVowel(char c)
    {
        return c is 'a' or 'e' or 'i' or 'o' or 'u';
    }
}
