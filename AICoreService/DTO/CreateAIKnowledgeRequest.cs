using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.AICoreService.DTO
{
    public class CreateAIKnowledgeRequest
    {
        public required string EventId { get; set; } = string.Empty;

        public required string TaskTitle { get; set; }
        public int ProjectId { get; set; }

        public int TaskId { get; set; }

        public required Metadata Metadata { get; set; }
    }
}