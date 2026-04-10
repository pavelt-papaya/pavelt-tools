using System.Net.Http.Headers;
using UsersIconUploader.Abstractions;
using UsersIconUploader.Models;

namespace UsersIconUploader.Infrastructure;

/// <summary>
/// Streams the local file to the upload URL with HTTP PUT (common for pre-signed object storage URLs).
/// </summary>
public sealed class PlaceholderAssetUploader : IAssetUploader
{
    private static readonly HttpClient Http = new();

    public async Task UploadAsync(LocalAsset asset, Uri uploadUrl, CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(
            asset.FullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            FileOptions.Asynchronous);

        using var content = new StreamContent(stream);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse(MimeTypeForFileName(asset.FileName));

        using var response = await Http.PutAsync(uploadUrl, content, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    private static string MimeTypeForFileName(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return ext.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            _ => "application/octet-stream"
        };
    }
}
