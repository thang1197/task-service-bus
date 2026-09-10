using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.AI.DTO
{
    public class AISummaryWorkerMessage
    {
        public string Command { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }
}