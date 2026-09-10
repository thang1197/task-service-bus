using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.ServiceBus.DTO
{
    public class QueueResponse
    {
        public required string QueueName { get; set; }

        public required string Status { get; set; }
    }
}