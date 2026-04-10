using Shine.Media.Entities;
using Shine.Media.WebApi.Client;
using Shine.Media.WebApi.Dtos;
using UsersIconUploader.Abstractions;
using UsersIconUploader.Models;

namespace UsersIconUploader.Infrastructure;

/// <summary>
/// Creates one media record per local file via <see cref="IMediaClient.CreateMedia"/>, returning job ids and upload URLs from <see cref="CreateMediaResponse"/>.
/// </summary>
public sealed class ShineMediaImageProcessingJobsClient(
    IMediaClient mediaClient,
    string category,
    MediaType mediaType = MediaType.Image) : IImageProcessingJobsClient
{
    public async Task<IReadOnlyList<PreparedUploadSlot>> CreateUploadSlotsAsync(
        IReadOnlyList<LocalAsset> assets,
        CancellationToken cancellationToken = default)
    {
        if (assets.Count == 0)
            return [];

        var slots = new List<PreparedUploadSlot>(assets.Count);
        foreach (var asset in assets)
        {
            var request = new CreateMediaRequestDto
            {
                Body = new CreateMediaRequestBodyDto
                {
                    Name = asset.FileName,
                    Category = category,
                    MediaType = mediaType
                }
            };

            var response = await mediaClient.CreateMedia(request, cancellationToken).ConfigureAwait(false);
            if (response is null
                || string.IsNullOrWhiteSpace(response.Id)
                || string.IsNullOrWhiteSpace(response.UploadUrl))
            {
                throw new InvalidOperationException(
                    $"CreateMedia returned an empty response for '{asset.FileName}'.");
            }

            if (!Uri.TryCreate(response.UploadUrl, UriKind.Absolute, out var uploadUri))
            {
                throw new InvalidOperationException(
                    $"CreateMedia returned a non-absolute UploadUrl for '{asset.FileName}': {response.UploadUrl}");
            }

            slots.Add(new PreparedUploadSlot(response.Id, uploadUri));
        }

        return slots;
    }
}
