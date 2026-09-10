using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.Worker.Service;

namespace TaskManagementServiceBusApi.File.Service
{
    public interface IFileService : IWorkerService
    {
        public Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName);
        public Task<Stream> DownloadFileAsync(string fileName);
    }
}