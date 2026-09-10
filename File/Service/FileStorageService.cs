using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using TaskManagementServiceBusApi.Configuration;
using TaskManagementServiceBusApi.Credential.Service;
using TaskManagementServiceBusApi.File.DTO;
using TaskManagementServiceBusApi.Worker.Service;

namespace TaskManagementServiceBusApi.File.Service
{
    public class FileStorageService(
    ILogger<FileStorageService> logger,
    ServiceBusClient serviceBusClient,
    BlobServiceClient blobServiceClient,
    IOptions<StorageAccountConfiguration> storageAccountConfiguration,
    ICredentialManager credentialManager,
    HttpClient httpClient
    ) : WorkerService(logger, serviceBusClient.CreateReceiver(
        "file-queue",
        new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        }
    )), IFileService
    {
        private readonly BlobServiceClient _blobServiceClient = blobServiceClient;

        private readonly ICredentialManager _credentialManager = credentialManager;
        private readonly HttpClient _httpClient = httpClient;

        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        // private readonly BlobContainerClient _blobContainerClient = blobServiceClient.GetBlobContainerClient(storageAccountConfiguration.Value.ContainerName);
        public async Task<Stream> DownloadFileAsync(string fileName)
        {
             using var activity = Telemetry.ActivitySource.StartActivity("DownloadFileAsync", ActivityKind.Consumer);
            activity?.SetTag("fileName", fileName);
            activity?.SetTag("containerName", storageAccountConfiguration.Value.ContainerName);
            try
            {
                var container = _blobServiceClient.GetBlobContainerClient(storageAccountConfiguration.Value.ContainerName);
                var blob = container.GetBlobClient(fileName) ?? throw new Exception("File not found.");
                var content = await blob.DownloadAsync();
                // return blob.DownloadAsync();
                return content.Value.Content;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while downloading file.");
                throw;
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName)
        {
            using var activity = Telemetry.ActivitySource.StartActivity("UploadFileAsync", ActivityKind.Consumer);
            activity?.SetTag("fileName", fileName);
            activity?.SetTag("containerName", containerName);

            try
            {
                var container = _blobServiceClient.GetBlobContainerClient(containerName);
                var blob = container.GetBlobClient(fileName);
                var result = await blob.UploadAsync(fileStream);
                return fileName;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading file.");
                throw;
            }
        }

        private IFileProcessor? GetFileProcessor(string contentType)
        {
            // Normalize content type to prevent casing or spacing issues
            string normalizedType = contentType?.Trim().ToLowerInvariant() ?? string.Empty;

            return normalizedType switch
            {
                // Image Group (handles image/jpeg, image/png, image/webp, etc.)
                string ct when ct.StartsWith("image/") => new ImageProcessor(_logger),

                // PDF Group
                "application/pdf" => new PdfProcessor(_logger),

                // Word Group (handles legacy .doc, modern .docx, and template types)
                // "application/msword" or
                // "application/vnd.openxmlformats-officedocument.wordprocessingml.document" or
                // "application/vnd.ms-word.document.macroenabled.12" => new WordProcessor(),

                // Fallback for unsupported formats
                _ => new GeneralFileProcessor(_logger),
            };
        }

        private static string GetContainerName(string contentType)
        {
            string normalizedType = contentType?.Trim().ToLowerInvariant() ?? string.Empty;
            return normalizedType switch
            {
                string ct when ct.StartsWith("image/") => "images",
                "application/pdf" => "pdf",
                _ => "general",

            };
        }

        protected override async Task HandleMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken)
        {
            try
            {
                using var activity = Telemetry.ActivitySource.StartActivity("Handle File Message", ActivityKind.Consumer);
                activity?.SetTag("messageId", message.MessageId);

                _logger.LogInformation("Received message: {MessageBody}", message.Body.ToString());
                string stringBody = message.Body.ToString();
                if (string.IsNullOrEmpty(stringBody)) throw new Exception("Message body is null or empty.");

                FileMessage fileMessage = JsonSerializer.Deserialize<FileMessage>(stringBody, _jsonSerializerOptions) ?? throw new Exception("Message body is null or empty.");
                IFileProcessor? fileProcessor = GetFileProcessor(fileMessage.FileType) ?? throw new Exception("File type is not correct.");

                activity?.SetTag("fileId", fileMessage.FileId);
                activity?.SetTag("fileName", fileMessage.FileName);

                var streamDownload = await DownloadFileAsync(fileMessage.FileName) ?? throw new Exception("File not found.");
                var fileHandleResponse = await fileProcessor.HandleFile(streamDownload, fileMessage.FileName, fileMessage.FileType);
                var containerName = GetContainerName(fileMessage.FileType);

                if (fileHandleResponse.FileStream != null)
                {
                    await UploadFileAsync(fileHandleResponse.FileStream, fileHandleResponse.FileName, containerName);
                    _logger.LogInformation("File upload successfully: {FileName}", fileHandleResponse.FileName);
                }

                string credential = await _credentialManager.GetCredential();
                _logger.LogInformation("Credential: {Credential}", credential);

                var createFileRequest = new CreateFileRequest
                {
                    Folder = containerName,
                    FileId = fileHandleResponse.FileName,
                    Status = "DONE",
                    ContentType = fileMessage.FileType,
                    Name = fileMessage.FileName,
                    UserCreated = fileMessage.UserCreated,
                    UserUpdated = fileMessage.UserUpdated,
                    ProjectId = fileMessage.ProjectId,
                    TaskId = fileMessage.TaskId,
                    Url = fileMessage.Url
                };

                activity?.SetTag("createRequest", createFileRequest);

                HttpRequestMessage requestMessage = new()
                {
                    Headers = {
                        { "Authorization", $"Bearer {credential}" }
                    },
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("/task-api/v1/file", UriKind.Relative),
                    Content = new StringContent(JsonSerializer.Serialize(createFileRequest, _jsonSerializerOptions), System.Text.Encoding.UTF8, "application/json")
                };

                var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

                // 3. Inspect response details if it fails
                if (!response.IsSuccessStatusCode)
                {
                    // Read the server's error explanation body (e.g., model validation errors)
                    string responseErrorBody = await response.Content.ReadAsStringAsync(cancellationToken);

                    Console.WriteLine($"[DEBUG] HTTP Status Code: {(int)response.StatusCode} {response.StatusCode}");
                    Console.WriteLine($"[DEBUG] Server Error Body:\n{responseErrorBody}");
                }

                response.EnsureSuccessStatusCode();
                _logger.LogInformation("File updated successfully: {FileName}", fileHandleResponse.FileName);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HttpRequestException occurred while handling file storage");
                if (ExceptionHandler.IsHttpTransientError(ex))
                {

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    throw new TransientException($"HttpRequestException occurred while handling file storage: {ex.Message}", ex);
                }
                throw new BusinessException($"HttpRequestException occurred while handling file storage: {ex.Message}", ex);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing file.");
                throw;
            }
        }
    }
}