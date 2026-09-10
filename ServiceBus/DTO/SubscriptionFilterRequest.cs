using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.ServiceBus.DTO
{
    public class SubscriptionFilterRequest
    {
        public required string TopicName { get; set; }
        public required string SubscriptionName { get; set; }

        public required string Filter { get; set; }

        public required string Expression { get; set; }
    }
}