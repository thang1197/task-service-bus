using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.ServiceBus.DTO
{
    public class SubscriptionRequest
    {
        public required string TopicName { get; set; }
        public required string SubscriptionName { get; set; }
    }
}