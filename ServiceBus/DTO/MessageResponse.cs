using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.ServiceBus.DTO
{
    public class MessageResponse
    {
        public required string MessageId { get; set; }

        public required string Body { get; set; }

        public required string ContentType { get; set; }

        public required string CorrelationId { get; set; }
    }
}