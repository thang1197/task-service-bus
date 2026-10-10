using TaskManagementServiceBusApi.AICoreService.DTO;

namespace TaskManagementServiceBusApi.AICoreService.Service
{
    public class AIService(ILogger<AIService> logger, AICoreServiceHttpClient aiCoreServiceHttpClient) : IAIService
    {
        private readonly ILogger<AIService> _logger = logger;
        private readonly AICoreServiceHttpClient _aiCoreServiceHttpClient = aiCoreServiceHttpClient;
        public async Task<AIKnowledgeResponse> CreateAIKnowledge(CreateAIKnowledgeRequest data)
        {
            try
            {
                var request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, "application/json"),
                    RequestUri = new Uri("aisummary/knowledge", UriKind.Relative)
                };
                var result = await _aiCoreServiceHttpClient.SendAsync<AIKnowledgeResponse>(request) ?? throw new Exception("AIKnowledgeResponse is null.");
                return result;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating AI knowledge.");
                throw;
            }
        }

        public async Task<AIKnowledgeResponse> UpdateAIKnowledge(UpdateAIKnowledgeRequest data)
        {
            try
            {
                var request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Patch,
                    Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, "application/json"),
                    RequestUri = new Uri("aisummary/knowledge/" + data.Id, UriKind.Relative)
                };
                var result = await _aiCoreServiceHttpClient.SendAsync<AIKnowledgeResponse>(request) ?? throw new Exception("AIKnowledgeResponse is null.");
                return result;
            }
            catch (System.Exception)
            {
                
                throw;
            }
        }
    }
}