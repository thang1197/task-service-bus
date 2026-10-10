using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.AICoreService.DTO;

namespace TaskManagementServiceBusApi.AICoreService.Service
{
    public interface IAIService
    {
        public Task<AIKnowledgeResponse> CreateAIKnowledge(CreateAIKnowledgeRequest request);

        public Task<AIKnowledgeResponse> UpdateAIKnowledge(UpdateAIKnowledgeRequest request);
    }
}