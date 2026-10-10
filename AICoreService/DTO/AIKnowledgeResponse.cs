using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.AICoreService.DTO
{
    public class AIKnowledgeResponse
    {
         public string Id { get; set; } = string.Empty;

        public int ProjectId { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int TaskId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
    
}