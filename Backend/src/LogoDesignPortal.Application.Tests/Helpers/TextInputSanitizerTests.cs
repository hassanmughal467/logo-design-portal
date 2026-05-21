using LogoDesignPortal.Application.Helpers;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Helpers;

public class TextInputSanitizerTests
{
    [Fact]
    public void SanitizePlainText_RemovesScriptTags()
    {
        var result = TextInputSanitizer.SanitizePlainText("Hello <script>alert(1)</script> world");
        Assert.DoesNotContain("<script", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Hello", result);
        Assert.Contains("world", result);
    }

    [Fact]
    public void SanitizePlainText_RemovesEventHandlers()
    {
        var result = TextInputSanitizer.SanitizePlainText(@"<img src=x onerror=alert(1)>");
        Assert.DoesNotContain("onerror=", result, StringComparison.OrdinalIgnoreCase);
    }
}
