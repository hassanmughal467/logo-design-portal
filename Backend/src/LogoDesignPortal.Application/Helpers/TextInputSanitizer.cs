using System.Text.RegularExpressions;

namespace LogoDesignPortal.Application.Helpers;

/// <summary>Lightweight HTML/script stripping for user-visible text fields (comments, descriptions).</summary>
public static partial class TextInputSanitizer
{
    public static string? SanitizePlainText(string? input, int maxLength = 8000)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var trimmed = input.Trim();
        if (trimmed.Length > maxLength)
            trimmed = trimmed[..maxLength];

        trimmed = ScriptTagPattern().Replace(trimmed, string.Empty);
        trimmed = EventHandlerPattern().Replace(trimmed, string.Empty);
        trimmed = JavascriptProtocolPattern().Replace(trimmed, string.Empty);
        return trimmed;
    }

    [GeneratedRegex(@"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptTagPattern();

    [GeneratedRegex(@"\bon\w+\s*=", RegexOptions.IgnoreCase)]
    private static partial Regex EventHandlerPattern();

    [GeneratedRegex(@"javascript\s*:", RegexOptions.IgnoreCase)]
    private static partial Regex JavascriptProtocolPattern();
}
