using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.Worker.DTO;

namespace TaskManagementServiceBusApi.Worker.Service
{
    public interface IWorkerService
    {
        public Task<WorkerResult> StartAsync();
        public Task<WorkerResult> StopAsync();
    }
}