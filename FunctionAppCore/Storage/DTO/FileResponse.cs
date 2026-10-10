using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.FunctionAppCore.Storage.DTO
{
    public class FileResponse
    {
        public int Id { get; init; }
        public required string Name { get; init; }
        public required string Url { get; init; }
        public long Size { get; init; }
        public required string ContentType { get; init; }
        public string Version { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string UserCreated { get; init; } = string.Empty;
        public string UserUpdated { get; init; } = string.Empty;
    }
}