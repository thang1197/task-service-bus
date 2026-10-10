using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.File.DTO;
using TaskManagementServiceBusApi.FunctionAppCore.Storage.DTO;
using TaskManagementServiceBusApi.FunctionAppCore.Storage.Service;
using TaskManagementServiceBusApi.Worker.DTO;

namespace TaskManagementServiceBusApi.File.Service
{
    public class FileStorageService(ILogger<FileStorageService> logger, IFunctionAppCoreStorageService functionAppCoreStorageService) : IFileStorageService
    {
        private readonly ILogger<FileStorageService> _logger = logger;
        private readonly IFunctionAppCoreStorageService _functionAppCoreStorageService = functionAppCoreStorageService;
        
        public async Task<Stream> DownloadFileAsync(string fileName, string containerName, CancellationToken cancellationToken = default)
        {
            var response = await _functionAppCoreStorageService.DownloadFileAsync(fileName, containerName, cancellationToken) ?? throw new Exception("File not found.");
            return response.Stream;
        }

        public async Task<FileUploadResponse> UploadFileAsync(FileUploadRequest fileUploadRequest, CancellationToken cancellationToken = default)
        {
            var response = await _functionAppCoreStorageService.UploadFileAsync(fileUploadRequest, cancellationToken) ?? throw new Exception("File not found.");
            if(response.Data == null)
            {
                throw new Exception("File upload response data is null.");
            }
            return new FileUploadResponse
            {
                FileName = response.Data.Name,
                FileType = response.Data.ContentType,
                Url = response.Data.Url,
                Size = response.Data.Size,
                UserCreated = response.Data.UserCreated,
                UserUpdated = response.Data.UserUpdated,
                Version = response.Data.Version
            };
        }
    }
}