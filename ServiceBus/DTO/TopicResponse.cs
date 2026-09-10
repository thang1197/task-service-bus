using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.ServiceBus.DTO
{
    public class TopicResponse
    {
        public required string TopicName { get; set; }

        public required string Status { get; set; }
    }
}