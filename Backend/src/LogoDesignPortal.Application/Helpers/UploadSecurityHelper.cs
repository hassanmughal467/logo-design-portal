using System.Text;

using System.Text.RegularExpressions;



namespace LogoDesignPortal.Application.Helpers;



/// <summary>

/// Centralized upload filename sanitization, extension policy, and content-type validation.

/// </summary>

public static partial class UploadSecurityHelper

{

    /// <summary>Extensions that must never be accepted regardless of magic bytes.</summary>

    public static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)

    {

        ".exe", ".bat", ".cmd", ".com", ".msi", ".scr", ".ps1", ".vbs", ".js", ".jse",

        ".wsf", ".wsh", ".hta", ".dll", ".aspx", ".asp", ".php", ".jsp", ".html", ".htm",

        ".svgz", ".jar", ".app", ".deb", ".rpm", ".dmg", ".iso", ".lnk", ".reg", ".inf"

    };



    private static readonly Dictionary<string, string> AllowedMimeByExtension = new(StringComparer.OrdinalIgnoreCase)

    {

        [".jpg"] = "image/jpeg",

        [".jpeg"] = "image/jpeg",

        [".png"] = "image/png",

        [".gif"] = "image/gif",

        [".webp"] = "image/webp",

        [".pdf"] = "application/pdf",

        [".svg"] = "image/svg+xml",

    };



    /// <summary>

    /// Validates the upload filename for blocked extensions, double extensions, and path traversal.

    /// </summary>

    public static void ValidateUploadFileName(string? fileName)

    {

        if (string.IsNullOrWhiteSpace(fileName))

            throw new InvalidOperationException("File name is required.");



        var normalized = fileName.Replace('\\', '/').Trim();

        if (normalized.Contains("..", StringComparison.Ordinal))

            throw new InvalidOperationException("Path traversal in file name is not allowed.");



        var nameOnly = Path.GetFileName(normalized);

        if (string.IsNullOrWhiteSpace(nameOnly))

            throw new InvalidOperationException("Invalid file name.");



        var effectiveExt = GetEffectiveExtension(nameOnly);

        if (string.IsNullOrWhiteSpace(effectiveExt))

            throw new InvalidOperationException("File must have an extension.");



        if (BlockedExtensions.Contains(effectiveExt))

            throw new InvalidOperationException($"File type '{effectiveExt}' is not allowed.");



        if (HasDoubleExtension(nameOnly))

            throw new InvalidOperationException("Double file extensions are not allowed.");

    }



    /// <summary>Returns the last extension segment (e.g. <c>file.png.exe</c> → <c>.exe</c>).</summary>

    public static string GetEffectiveExtension(string fileName)

    {

        var nameOnly = Path.GetFileName(fileName.Replace('\\', '/'));

        var ext = Path.GetExtension(nameOnly);

        return string.IsNullOrWhiteSpace(ext) ? string.Empty : ext.ToLowerInvariant();

    }



    private static readonly HashSet<string> DecoyExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".webp", ".pdf", ".txt", ".doc", ".docx", ".zip"
    };

    /// <summary>Detects decoy+payload patterns such as <c>logo.png.exe</c> (not <c>my.company.png</c>).</summary>
    public static bool HasDoubleExtension(string fileName)
    {
        var nameOnly = Path.GetFileName(fileName.Replace('\\', '/'));
        var last = GetEffectiveExtension(nameOnly);
        var withoutLast = Path.GetFileNameWithoutExtension(nameOnly);
        var penultimate = Path.GetExtension(withoutLast);
        if (string.IsNullOrEmpty(penultimate))
            return false;
        return DecoyExtensions.Contains(penultimate)
               && !string.Equals(penultimate, last, StringComparison.OrdinalIgnoreCase);
    }



    /// <summary>

    /// Strips path segments and dangerous characters from an original upload filename for safe storage and download headers.

    /// </summary>

    public static string SanitizeOriginalFileName(string? fileName, string fallbackExtension = ".bin")

    {

        if (string.IsNullOrWhiteSpace(fileName))

            return $"upload{fallbackExtension}";



        var nameOnly = Path.GetFileName(fileName.Replace('\\', '/').Trim());

        if (string.IsNullOrWhiteSpace(nameOnly))

            return $"upload{fallbackExtension}";



        var ext = GetEffectiveExtension(nameOnly);

        if (string.IsNullOrWhiteSpace(ext))

            ext = fallbackExtension.StartsWith('.') ? fallbackExtension : "." + fallbackExtension;



        var baseName = Path.GetFileNameWithoutExtension(nameOnly);

        if (HasDoubleExtension(nameOnly))

            baseName = baseName.Contains('.') ? baseName[..baseName.LastIndexOf('.')] : baseName;



        if (string.IsNullOrWhiteSpace(baseName))

            baseName = "upload";



        baseName = InvalidFileNameChars().Replace(baseName, "_");

        baseName = baseName.Trim('.', ' ');

        if (baseName.Length > 120)

            baseName = baseName[..120];



        if (string.IsNullOrWhiteSpace(baseName))

            baseName = "upload";



        return $"{baseName}{ext}";

    }



    /// <summary>

    /// When a MIME type is declared, ensure it is compatible with the file extension (best-effort; extension + magic bytes remain primary).

    /// </summary>

    public static void ValidateDeclaredContentType(string extension, string? contentType)

    {

        if (string.IsNullOrWhiteSpace(contentType))

            return;



        var ext = extension.StartsWith('.') ? extension : "." + extension;

        if (!AllowedMimeByExtension.TryGetValue(ext, out var expected))

            return;



        if (!contentType.StartsWith(expected, StringComparison.OrdinalIgnoreCase)

            && !(ext is ".jpg" or ".jpeg" && contentType.StartsWith("image/jpg", StringComparison.OrdinalIgnoreCase)))

        {

            throw new InvalidOperationException(

                $"Content type '{contentType}' does not match file extension '{ext}'.");

        }

    }



    /// <summary>Validates magic bytes for image/PDF types that support signature checks.</summary>

    public static void ValidateMagicBytes(string extension, Stream stream)

    {

        if (!MagicBytes.TryGetValue(extension.ToLowerInvariant(), out var signatures))

            return;



        var header = new byte[Math.Max(12, signatures.Max(s => s.Length))];

        var read = stream.Read(header, 0, header.Length);

        stream.Position = 0;



        if (read < signatures.Min(s => s.Length))

            throw new InvalidOperationException($"File content does not match extension '{extension}'.");



        var matches = signatures.Any(sig => header.Take(sig.Length).SequenceEqual(sig));

        if (!matches && extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))

        {

            var riff = header.Take(4).SequenceEqual(new byte[] { 0x52, 0x49, 0x46, 0x46 });

            var webp = read >= 12 && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;

            matches = riff && webp;

        }



        if (!matches)

            throw new InvalidOperationException($"File content does not match extension '{extension}'. Possible file type spoofing.");

    }



    private static readonly Dictionary<string, byte[][]> MagicBytes = new(StringComparer.OrdinalIgnoreCase)

    {

        [".jpg"] = [new byte[] { 0xFF, 0xD8, 0xFF }],

        [".jpeg"] = [new byte[] { 0xFF, 0xD8, 0xFF }],

        [".png"] = [new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }],

        [".gif"] = [new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }],

        [".webp"] = [new byte[] { 0x52, 0x49, 0x46, 0x46 }],

        [".pdf"] = [new byte[] { 0x25, 0x50, 0x44, 0x46 }],

        [".svg"] = [new byte[] { 0x3C, 0x3F, 0x78, 0x6D, 0x6C }, new byte[] { 0x3C, 0x73, 0x76, 0x67 }],

    };



    [GeneratedRegex(@"[<>:""/\\|?*\x00-\x1F]")]

    private static partial Regex InvalidFileNameChars();

}


