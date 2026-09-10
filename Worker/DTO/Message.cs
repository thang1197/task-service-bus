using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.Worker.DTO
{
    public class Message
    {
        public string Id { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public int DeliveryCount { get; set; } = 0;

        public string Subject { get; set; } = string.Empty;
    }
}