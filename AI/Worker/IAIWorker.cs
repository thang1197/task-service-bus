using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.Worker.Service;

namespace TaskManagementServiceBusApi.AI.Worker
{
    public interface IAIWorker : ISubscriberWorkerService
    {       
        public Task StartAsync(CancellationToken cancellationToken);
        public Task StopAsync(CancellationToken cancellationToken);
    }
}