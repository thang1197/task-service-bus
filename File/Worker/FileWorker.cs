using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using SharedCommon.CoreApi.Files.DTO;
using SharedCommon.CoreApi.Files.Service;
using TaskManagementServiceBusApi.Configuration;
using TaskManagementServiceBusApi.File.DTO;
using TaskManagementServiceBusApi.File.Service;
using TaskManagementServiceBusApi.FunctionAppCore.Storage.DTO;
using TaskManagementServiceBusApi.FunctionAppCore.Storage.Service;
using TaskManagementServiceBusApi.Worker.Service;

namespace TaskManagementServiceBusApi.File.Worker
{
    public class FileWorker(
        ILogger<FileWorker> logger,
        ServiceBusClient serviceBusClient,
        IFileStorageService fileStorageService,
        ICoreApiFileService coreApiFileService
    ) : WorkerService(
        logger,
        serviceBusClient.CreateReceiver(
        "files",
        new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        })
    ), IFileWorker
    {
        private readonly IFileStorageService _fileStorageService = fileStorageService;
        private readonly ICoreApiFileService _coreApiFileService = coreApiFileService;

        private readonly JsonSerializerOptions _jsonSerializerOptions = new ()
        {
            PropertyNameCaseInsensitive = true
        };
        
        private IFileProcessor GetFileProcessor(string contentType)
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

                string stringBody = message.Body.ToString();
                if (string.IsNullOrEmpty(stringBody)) throw new Exception("Message body is null or empty.");

                FileMessage fileMessage = JsonSerializer.Deserialize<FileMessage>(stringBody, _jsonSerializerOptions) ?? throw new Exception("Message body is null or empty.");
                IFileProcessor fileProcessor = GetFileProcessor(fileMessage.FileType);

                activity?.SetTag("fileId", fileMessage.FileId);
                activity?.SetTag("fileName", fileMessage.FileName);

                var streamDownload = await _fileStorageService.DownloadFileAsync(fileMessage.FileName, "uploads", cancellationToken) ?? throw new Exception("File not found.");
                var fileHandleResponse = await fileProcessor.HandleFile(streamDownload, fileMessage.FileName, fileMessage.FileType);

                var fileUploadRequest = new FileUploadRequest()
                {
                    ContainerName = GetContainerName(fileMessage.FileType),
                    FileName = fileHandleResponse.FileName,
                    Content = fileHandleResponse.FileStream ?? throw new Exception("File stream is null."),
                    TaskId = fileMessage.TaskId,
                    ProjectId = fileMessage.ProjectId
                };

                var fileUploadResponse = await _fileStorageService.UploadFileAsync(fileUploadRequest, cancellationToken);

                var createFileStorageRequest = new CreateFileStorageRequest()
                {
                    ContentType = fileUploadResponse.FileType,
                    Name = fileUploadResponse.FileName,
                    Url = fileUploadResponse.Url,
                    TaskId = fileMessage.TaskId,
                    ProjectId = fileMessage.ProjectId,
                    FileId = fileUploadResponse.FileName,
                    Folder = GetContainerName(fileMessage.FileType),
                    Size = fileUploadResponse.Size,
                    Status = "DONE",
                    UserCreated = fileUploadResponse.UserCreated,
                    UserUpdated = fileUploadResponse.UserUpdated
                };
                var createFileStorageResponse = await _coreApiFileService.CreateFileStorageAsync(createFileStorageRequest);

                _logger.LogInformation("File processed successfully.");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing file.");
                throw;
            }
        }
    }
}