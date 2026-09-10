using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.Configuration.ConfigurationFactory
{
    public class LocalConfigurationFactory(WebApplicationBuilder builder) : IConfigurationFactory
    {
        private readonly WebApplicationBuilder _builder = builder;
        public async Task<string> GetConfigurationAsync(string key)
        {
            try
            {
                var configuration =_builder.Configuration.GetSection(key).Value;
                if(string.IsNullOrEmpty(configuration)) throw new KeyNotFoundException($"Configuration for key '{key}' not found.");
                return configuration;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Error getting secret: " + ex.Message);
                throw;
            }
        }
    }
}