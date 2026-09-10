using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.Configuration.ConfigurationFactory
{
    public class AppConfigurationFactory : IConfigurationFactory
    {
        public async Task<string> GetConfigurationAsync(string key)
        {
            throw new NotImplementedException();
        }
    }
}