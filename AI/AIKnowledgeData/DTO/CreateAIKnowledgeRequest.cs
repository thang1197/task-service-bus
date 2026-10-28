using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.AICoreService.DTO;

namespace TaskManagementServiceBusApi.AI.AIKnowledgeData.DTO
{
    public class CreateAIKnowledgeRequest
    {
        public required string EventId { get; set; } = string.Empty;

        public required string TaskTitle { get; set; }
        public int ProjectId { get; set; }

        public string Content { get; set; } = string.Empty;

        public int TaskId { get; set; }

        public Dictionary<string, string> Metadata { get; set; } = [];
    }
}