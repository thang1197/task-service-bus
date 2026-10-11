using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.AICoreService.DTO
{
    public class UpdateAIKnowledgeRequest
    {
        public string Id { get; set; } = string.Empty;

        public int ProjectId { get; set; }

        public ActivityType? Type { get; set; }
        public string? Content { get; set; }

        public int TaskId { get; set; }

        public DateTime CreatedAt { get; set; }

        public Dictionary<string, string>? Metadata { get; set; } = [];
    }
}