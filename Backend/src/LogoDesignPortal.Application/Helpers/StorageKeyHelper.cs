namespace LogoDesignPortal.Application.Helpers;

/// <summary>
/// Normalizes storage keys and resolves legacy absolute paths stored in the database.
/// </summary>
public static class StorageKeyHelper
{
    public static string ValidateAndNormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Storage key is required.", nameof(key));
        }

        var normalized = key.Replace('\\', '/').TrimStart('/');
        if (normalized.Contains("..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Path traversal is not allowed in storage keys.");
        }

        return normalized;
    }

    /// <summary>
    /// Converts a stored path (absolute local path or relative key) to a storage key.
    /// </summary>
    public static string ResolveKey(string storedPathOrKey, string localBasePath)
    {
        if (string.IsNullOrWhiteSpace(storedPathOrKey))
        {
            throw new ArgumentException("Stored path or key is required.", nameof(storedPathOrKey));
        }

        if (!Path.IsPathRooted(storedPathOrKey))
        {
            return ValidateAndNormalizeKey(storedPathOrKey);
        }

        var fullBase = Path.GetFullPath(localBasePath);
        var fullPath = Path.GetFullPath(storedPathOrKey);
        if (fullPath.StartsWith(fullBase, StringComparison.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(fullBase, fullPath);
            return ValidateAndNormalizeKey(relative);
        }

        return ValidateAndNormalizeKey(storedPathOrKey.Replace('\\', '/').TrimStart('/'));
    }

    public static string CombineKey(string prefix, string fileName)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return ValidateAndNormalizeKey(fileName);
        }

        return ValidateAndNormalizeKey($"{prefix.Trim('/')}/{fileName}");
    }
}
