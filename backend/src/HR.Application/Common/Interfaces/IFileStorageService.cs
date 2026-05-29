namespace HR.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(string fileName, Stream content, string contentType, CancellationToken ct = default);
    Task<Stream?> DownloadAsync(string fileKey, CancellationToken ct = default);
    Task DeleteAsync(string fileKey, CancellationToken ct = default);
    Task<string> GetPresignedUrlAsync(string fileKey, TimeSpan expiry, CancellationToken ct = default);
}
