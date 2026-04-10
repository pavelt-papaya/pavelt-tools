using UsersIconUploader.Models;

namespace UsersIconUploader.Services;

public static class LocalAssetScanner
{
    private static readonly HashSet<string> SupportedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".gif", ".webp", ".bmp", ".svg", ".ico"
        };

    /// <summary>
    /// Returns image files directly under <paramref name="folderPath"/> (not recursive), sorted by file name.
    /// </summary>
    public static IReadOnlyList<LocalAsset> Scan(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"Folder not found: {folderPath}");

        var dir = new DirectoryInfo(folderPath);
        var files = dir
            .EnumerateFiles()
            .Where(f => SupportedExtensions.Contains(f.Extension))
            .OrderBy(f => f.Name, StringComparer.Ordinal)
            .Select(f => new LocalAsset(f.FullName, f.Name))
            .ToList();

        return files;
    }
}
