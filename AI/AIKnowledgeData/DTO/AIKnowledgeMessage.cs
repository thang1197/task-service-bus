using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.AICoreService.DTO;

namespace TaskManagementServiceBusApi.AI.AIKnowledgeData.DTO
{
    public class AIKnowledgeMessage
    {
        public required string Id { get; set; }

        public required string EventId { get; set; } 
        public required string TaskTitle { get; set; }

        public int ProjectId { get; set; }

        public int TaskId { get; set; }

        [JsonPropertyName("action")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Action Action { get; set; }

        [JsonPropertyName("actor")]
        public required string Actor { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; } =
            DateTimeOffset.UtcNow;

        [JsonPropertyName("changes")]
        public List<Changes> Changes { get; set; } = [];
    }

     public sealed class Changes
    {
        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        [JsonPropertyName("oldValue")]
        public object? OldValue { get; set; }

        [JsonPropertyName("newValue")]
        public object? NewValue { get; set; }
    }

    public enum Action
    {
        TaskCreated,
        TaskUpdated,
        TaskStatusChanged,
        TaskAssigned,
        TaskUnassigned,
        TaskDeleted,
        TaskRestored,
        TaskCommentAdded,
        TaskAttachmentAdded,
        TaskDueDateChanged
    }
}