using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.File.DTO;

namespace TaskManagementServiceBusApi.File.Service
{
    public interface IFileProcessor
    {
        public Task<FileHandleResponse> HandleFile(Stream fileStream, string fileName, string fileType);
    }
}