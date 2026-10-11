using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.FunctionAppCore.Storage.DTO
{
    public class FileUploadRequest
    {
        public required string FileName { get; set; }

        public required string ContainerName { get; set; }

        public int TaskId { get; set; }

        public int ProjectId { get; set; }

        public required Stream Content { get; set; }
    }
}