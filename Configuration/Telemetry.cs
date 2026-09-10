using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.Configuration
{
    public class Telemetry
    {
         public const string ServiceName = "task-management-servicebus-api";

        public static readonly ActivitySource ActivitySource =
            new(ServiceName);

        public static readonly Meter Meter =
            new(ServiceName);
    }
}