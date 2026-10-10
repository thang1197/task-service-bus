using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.File.DTO
{
    public class FileUploadResponse
    {
        public required string FileName { get; set; }
        public required string FileType { get; set; }

        public required string Url { get; set; }

        public string Version { get; set; } = string.Empty;

        public long Size { get; set; }

        public string UserCreated { get; set; } = string.Empty;

        public string UserUpdated { get; set; } = string.Empty;
    }
}