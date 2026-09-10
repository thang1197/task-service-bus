using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.Configuration.ConfigurationFactory
{
    public interface IConfigurationFactory
    {
        public Task<string> GetConfigurationAsync(string key);
    }
}