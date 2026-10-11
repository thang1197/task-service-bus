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
        public int Id { get; set; }
        public string EventId { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Description { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Username { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public required string UserCreated { get; set; } = string.Empty;
        public required string UserUpdated { get; set; } = string.Empty;

        public ActivityType Type { get; set; } = ActivityType.Unknown;
    }
}