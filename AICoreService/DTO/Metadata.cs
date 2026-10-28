using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.AICoreService.DTO
{
    public class Metadata
    {
        public required string Actor { get; set; }
        public Changes TaskName { get; set; } = new Changes { From = "", To = "" };
        public Changes? Assignee { get; set; }
        public Changes? ExpirationDate { get; set; }
        public Changes? Status { get; set; }
    }

    public class Changes
    {
        public required string From { get; set; }
        public required string To { get; set; }
    }
}