using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.Configuration.TaskManagementApi.DTO;

namespace TaskManagementServiceBusApi.Configuration.TaskManagementApi
{
    public interface ITaskManagementApiService
    {
        public Task<List<UserTasksResponse>> GetAllUserTasks(string projectId, CancellationToken cancellationToken);
    }
}