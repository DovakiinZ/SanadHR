using Amazon.S3;
using Amazon.S3.Model;
using HR.Application.Common.Interfaces;

namespace HR.Infrastructure.Services;

public class R2FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public R2FileStorageService(IAmazonS3 s3Client, string bucketName)
    {
        _s3Client = s3Client;
        _bucketName = bucketName;
    }

    public async Task<string> UploadAsync(string fileName, Stream content, string contentType, CancellationToken ct = default)
    {
        var key = $"{Guid.NewGuid()}/{fileName}";
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType
        };
        await _s3Client.PutObjectAsync(request, ct);
        return key;
    }

    public async Task<Stream?> DownloadAsync(string fileKey, CancellationToken ct = default)
    {
        var response = await _s3Client.GetObjectAsync(_bucketName, fileKey, ct);
        return response.ResponseStream;
    }

    public async Task DeleteAsync(string fileKey, CancellationToken ct = default)
    {
        await _s3Client.DeleteObjectAsync(_bucketName, fileKey, ct);
    }

    public Task<string> GetPresignedUrlAsync(string fileKey, TimeSpan expiry, CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = fileKey,
            Expires = DateTime.UtcNow.Add(expiry)
        };
        return Task.FromResult(_s3Client.GetPreSignedURL(request));
    }
}
