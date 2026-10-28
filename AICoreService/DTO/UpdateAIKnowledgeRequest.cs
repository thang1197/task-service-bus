using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.AICoreService.DTO
{
    public class UpdateAIKnowledgeRequest
    {
        public required string Id { get; set; }

        public required string TaskTitle { get; set; }

        public int ProjectId { get; set; }

        public int TaskId { get; set; }

        public Metadata? Metadata { get; set; }
    }
}