using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.File.DTO
{
    public class FileHandleResponse
    {
        public Stream? FileStream { get; set; }
        public required string FileName { get; set; }
        public required string FileType { get; set; }
    }
}