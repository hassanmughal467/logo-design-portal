using System.Text.RegularExpressions;

namespace LogoDesignPortal.API.Middleware;

public class InputSanitizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InputSanitizationMiddleware> _logger;

    public InputSanitizationMiddleware(RequestDelegate next, ILogger<InputSanitizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip sanitization for CORS preflight and non-body requests
        if (context.Request.Method == "OPTIONS" ||
            context.Request.Method == "GET" ||
            context.Request.Method == "DELETE" ||
            !context.Request.ContentType?.Contains("application/json") == true)
        {
            await _next(context);
            return;
        }

        // Only sanitize JSON content for POST/PUT/PATCH
        if (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "PATCH")
        {
            // Enable buffering to allow reading body multiple times
            context.Request.EnableBuffering();

            // Read the original body
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var originalBody = await reader.ReadToEndAsync();

            // Reset the stream position for the next middleware/controller
            context.Request.Body.Position = 0;

            // Only sanitize if body contains potentially dangerous content
            if (!string.IsNullOrEmpty(originalBody) && ContainsDangerousContent(originalBody))
            {
                var sanitizedBody = SanitizeInput(originalBody);

                // Replace the body stream with sanitized content
                var sanitizedBytes = System.Text.Encoding.UTF8.GetBytes(sanitizedBody);
                context.Request.Body = new MemoryStream(sanitizedBytes);
            }
        }

        await _next(context);
    }

    private static bool ContainsDangerousContent(string input)
    {
        // Quick check for potentially dangerous patterns
        return input.Contains("<script", StringComparison.OrdinalIgnoreCase) ||
               input.Contains("javascript:", StringComparison.OrdinalIgnoreCase) ||
               input.Contains("onerror=", StringComparison.OrdinalIgnoreCase) ||
               input.Contains("onclick=", StringComparison.OrdinalIgnoreCase);
    }

    private static string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Remove script tags
        input = Regex.Replace(input, @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", "", RegexOptions.IgnoreCase);

        // Remove javascript: protocol
        input = Regex.Replace(input, @"javascript:", "", RegexOptions.IgnoreCase);

        // Remove on* event handlers
        input = Regex.Replace(input, @"on\w+\s*=", "", RegexOptions.IgnoreCase);

        // Remove data: URLs that could be dangerous
        input = Regex.Replace(input, @"data:text/html", "", RegexOptions.IgnoreCase);

        return input;
    }
}
