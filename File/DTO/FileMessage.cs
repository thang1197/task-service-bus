using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.File.DTO
{
    public class FileMessage
    {
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public int TaskId { get; set; } = 0;
        public int ProjectId { get; set; } = 0;
        public string UserCreated { get; set; } = string.Empty;

        public string UserUpdated { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;
    }
}