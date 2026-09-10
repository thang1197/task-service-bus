using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.Worker.Service;

namespace TaskManagementServiceBusApi.Audit.Service
{
    public interface IAuditService : IWorkerService
    {
        public Task CreateAuditLogAsync(string action, string entity, string userId, string details);
    }
}