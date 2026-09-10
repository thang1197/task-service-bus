using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.File.DTO;

namespace TaskManagementServiceBusApi.File.Service
{
    public class GeneralFileProcessor(ILogger logger) : IFileProcessor
    {
        private readonly ILogger _logger = logger;
        public async Task<FileHandleResponse> HandleFile(Stream fileStream, string fileName, string fileType)
        {
            try
            {
                var fileHandleResponse = new FileHandleResponse
                {
                    FileName = fileName,
                    FileType = fileType,
                    FileStream = fileStream
                };
                return fileHandleResponse;
            }
            catch (System.Exception ex)
            {
                
                throw;
            }
        }
    }
}