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
        public int ProjectId { get; set; }

        public ActivityType Type { get; set; } = ActivityType.Unknown;

        public string Content { get; set; } = string.Empty;

        public int TaskId { get; set; }

        public Dictionary<string, string> Metadata { get; set; } = [];
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ActivityType { TaskCreated, StatusChanged, Commented, Assigned, DueDateChanged, Unknown }
}