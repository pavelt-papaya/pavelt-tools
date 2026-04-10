using UsersIconUploader.Abstractions;
using UsersIconUploader.Models;

namespace UsersIconUploader.Services;

/// <summary>
/// Coordinates job creation, per-file upload, and ordered job id collection for JSON output.
/// </summary>
public sealed class AssetUploadOrchestrator(
    IImageProcessingJobsClient jobsClient,
    IAssetUploader assetUploader)
{
    public async Task<IReadOnlyList<string>> UploadFolderAndCollectJobIdsAsync(
        string folderPath,
        CancellationToken cancellationToken = default)
    {
        var assets = LocalAssetScanner.Scan(folderPath);
        if (assets.Count == 0)
            return Array.Empty<string>();

        var slots = await jobsClient.CreateUploadSlotsAsync(assets, cancellationToken).ConfigureAwait(false);
        if (slots.Count != assets.Count)
            throw new InvalidOperationException(
                $"API returned {slots.Count} slot(s) but folder has {assets.Count} asset(s); counts must match.");

        for (var i = 0; i < assets.Count; i++)
        {
            await assetUploader
                .UploadAsync(assets[i], slots[i].UploadUrl, cancellationToken)
                .ConfigureAwait(false);
        }

        return slots.Select(s => s.JobId).ToList();
    }
}
