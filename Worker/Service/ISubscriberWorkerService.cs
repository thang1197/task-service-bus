using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.Worker.DTO;

namespace TaskManagementServiceBusApi.Worker.Service
{
    public interface ISubscriberWorkerService
    {
        public Task SubscribeAsync(CancellationToken cancellationToken);
        public Task UnsubscribeAsync(CancellationToken cancellationToken);

        public Task<List<Message>> GetMessagesAsync(int maxMessageCount, CancellationToken cancellationToken);
        
    }
}