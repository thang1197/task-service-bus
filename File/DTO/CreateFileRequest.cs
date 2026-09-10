using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.File.DTO
{
    public class CreateFileRequest
    {
        public string? Name { get; set; }

        public string? Url { get; set; }

        public string? FileId { get; set; }

        public string? Folder { get; set; }

        public string? Status { get; set; }

        public long? Size { get; set; }

        public int? ProjectId { get; set; }

        public int? TaskId { get; set; }

        public string? ContentType { get; set; }

        public string? Version { get; set; } = string.Empty;

        public string? UserCreated { get; set; }

        public string? UserUpdated { get; set; }
    }
}