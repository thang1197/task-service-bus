using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.ServiceBus.DTO
{
    public class QueueRequest
    {
        public required string QueueName { get; set; }
    }
}