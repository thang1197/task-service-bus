using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.Configuration.TaskManagementApi.DTO;

namespace TaskManagementServiceBusApi.Configuration.TaskManagementApi
{
    public class TaskManagementApiService(HttpClient httpClient) : ITaskManagementApiService
    {
        private readonly HttpClient _httpClient = httpClient;

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<List<UserTasksResponse>> GetAllUserTasks(string projectId, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/tasks/project/{projectId}", cancellationToken);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                // Assuming you have a method to deserialize the JSON content into a list of UserTasksResponse
                return JsonSerializer.Deserialize<List<UserTasksResponse>>(content, _jsonSerializerOptions) ?? [];
            }
            catch (System.Exception ex)
            {
                // Log the exception or handle it as needed
                throw new ApplicationException($"Error occurred while fetching tasks for project {projectId}: {ex.Message}", ex);
            }
        }
    }
}