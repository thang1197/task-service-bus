using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.File.DTO;
using TaskManagementServiceBusApi.FunctionAppCore.Storage.DTO;
using TaskManagementServiceBusApi.Worker.Service;

namespace TaskManagementServiceBusApi.File.Service
{
    public interface IFileStorageService
    {
        public Task<FileUploadResponse> UploadFileAsync(FileUploadRequest fileUploadRequest, CancellationToken cancellationToken = default);
        public Task<Stream> DownloadFileAsync(string fileName, string containerName, CancellationToken cancellationToken = default);
    }
}