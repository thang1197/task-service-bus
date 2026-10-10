using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.AI.AIKnowledgeData.DTO;
using TaskManagementServiceBusApi.AICoreService.DTO;

namespace TaskManagementServiceBusApi.AI.AIKnowledgeData.Service
{
    public interface IAIKnowledgeService
    {
        public Task<AIKnowledgeResponse> UpsertAIKnowledgeAsync(AIKnowledgeMessage request);
    }
}