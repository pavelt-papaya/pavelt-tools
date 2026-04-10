using UsersIconUploader.Models;

namespace UsersIconUploader.Abstractions;

/// <summary>
/// Uploads raw file bytes to a pre-signed or job-specific URL returned by <see cref="IImageProcessingJobsClient"/>.
/// </summary>
public interface IAssetUploader
{
    Task UploadAsync(LocalAsset asset, Uri uploadUrl, CancellationToken cancellationToken = default);
}
