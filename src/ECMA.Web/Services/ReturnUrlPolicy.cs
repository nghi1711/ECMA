namespace ECMA.Web.Services;

public static class ReturnUrlPolicy
{
    private static readonly HashSet<string> AllowedGetPaths = new(StringComparer.Ordinal)
    {
        "/", "/privacy", "/account/profile", "/account/change-password"
    };

    public static string? Canonicalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsControl) || value.Contains('\\')) return null;

        string decoded = value;
        for (var i = 0; i < 4; i++)
        {
            string next;
            try { next = Uri.UnescapeDataString(decoded); }
            catch (UriFormatException) { return null; }
            if (next == decoded) break;
            decoded = next;
        }

        if (!decoded.StartsWith('/') || decoded.StartsWith("//") || decoded.Contains('\\') || decoded.Any(char.IsControl) ||
            !Uri.TryCreate("https://local" + decoded, UriKind.Absolute, out var uri) || uri.Host != "local" ||
            !AllowedGetPaths.Contains(uri.AbsolutePath)) return null;

        return uri.PathAndQuery + uri.Fragment;
    }
}
