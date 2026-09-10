using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.Configuration.TaskManagementApi.DTO
{
    public class UserTasksResponse
    {
        public required string UserId { get; set; } = string.Empty;

        public required List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }

    public class TaskItem
    {
        public required string TaskId { get; set; } = string.Empty;

        public required string Title { get; set; } = string.Empty;

        public required string Description { get; set; } = string.Empty;

        public required DateTime DueDate { get; set; }

        public required bool IsCompleted { get; set; }
    }
}