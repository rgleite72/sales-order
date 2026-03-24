using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using SalesOrder.Application.Contracts.Storage;
using SalesOrder.Application.DTOs.Storage;

namespace SalesOrder.Infrastructure.Storage;

public class OrderDocumentStorageService : IOrderDocumentStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public OrderDocumentStorageService(
        BlobServiceClient blobServiceClient,
        IConfiguration configuration)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = configuration["AzureBlobStorage:ContainerName"]!;
    }

    public async Task<UploadOrderPdfResultDto> UploadPdfAsync(
        UploadOrderPdfRequestDto request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.FilePath))
            throw new ArgumentException("FilePath is required.");

        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new ArgumentException("FileName is required.");

        if (!File.Exists(request.FilePath))
            throw new FileNotFoundException("PDF file not found.", request.FilePath);

        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

        await containerClient.CreateIfNotExistsAsync(cancellationToken: ct);

        var blobName = $"{Guid.NewGuid():N}-{request.FileName}";
        var blobClient = containerClient.GetBlobClient(blobName);

        await using var fileStream = File.OpenRead(request.FilePath);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = request.ContentType
            }
        };

        await blobClient.UploadAsync(fileStream, uploadOptions, ct);

        return new UploadOrderPdfResultDto
        {
            BlobName = blobName,
            BlobUrl = blobClient.Uri.ToString(),
            ContentType = request.ContentType
        };
    }
}
