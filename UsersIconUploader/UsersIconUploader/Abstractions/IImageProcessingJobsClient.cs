using UsersIconUploader.Models;

namespace UsersIconUploader.Abstractions;

/// <summary>
/// Talks to the image-processing API (e.g. port-forwarded service) to reserve upload URLs and job ids.
/// </summary>
public interface IImageProcessingJobsClient
{
    /// <summary>
    /// Creates one processing job (or batch) per asset. The returned list must align with <paramref name="assets"/> by index.
    /// </summary>
    Task<IReadOnlyList<PreparedUploadSlot>> CreateUploadSlotsAsync(
        IReadOnlyList<LocalAsset> assets,
        CancellationToken cancellationToken = default);
}
