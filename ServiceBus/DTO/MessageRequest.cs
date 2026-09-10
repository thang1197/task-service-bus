using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.ServiceBus.DTO
{
    public class MessageRequest
    {
        public required string QueueName { get; set; } = string.Empty;
        public JsonElement Payload { get; set; } = JsonElement.Parse("{}");
        public required string Subject { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
    }
}