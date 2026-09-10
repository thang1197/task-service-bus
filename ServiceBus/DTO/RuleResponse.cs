using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.ServiceBus.DTO
{
    public class RuleResponse
    {
        public string RuleName { get; set; } = string.Empty;
        public string Filter { get; set; } = string.Empty;
    }
}