namespace UsersIconUploader.Models;

/// <summary>
/// A file on disk that should be sent through the image-processing pipeline.
/// </summary>
public sealed record LocalAsset(string FullPath, string FileName);
