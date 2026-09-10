using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.Email.DTO;
using TaskManagementServiceBusApi.Worker.Service;

namespace TaskManagementServiceBusApi.Email.Service
{
    public interface IEmailService : IWorkerService
    {
    }
}