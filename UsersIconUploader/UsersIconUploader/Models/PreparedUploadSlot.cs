namespace UsersIconUploader.Models;

/// <summary>
/// Server-issued destination for one asset: where to PUT/POST the bytes and which job id to record after upload.
/// </summary>
public sealed record PreparedUploadSlot(string JobId, Uri UploadUrl);
