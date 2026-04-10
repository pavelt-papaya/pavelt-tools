using Flurl.Http;
using Shine.Media.WebApi.Client;

namespace UsersIconUploader.Infrastructure;

public static class MediaClientFactory
{
    /// <summary>
    /// Base URL of the port-forwarded (or reachable) Shine Media HTTP API root.
    /// </summary>
    public static IMediaClient Create(string apiBaseUrl)
    {
        var normalized = apiBaseUrl.Trim().TrimEnd('/');
        if (normalized.Length == 0)
            throw new ArgumentException("API base URL is empty.", nameof(apiBaseUrl));

        var flurl = new FlurlClient(normalized);
        return new MediaClient(flurl);
    }
}
