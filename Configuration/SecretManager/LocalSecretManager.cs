using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.Configuration.SecretManager
{
    public class LocalSecretManager(WebApplicationBuilder builder) : ISecretManager
    {
        private readonly WebApplicationBuilder _builder = builder;
        public async Task<string> GetSecretAsync(string secretName)
        {
            try
            {
                var secret =_builder.Configuration.GetSection(secretName).Value;
                if(string.IsNullOrEmpty(secret)) throw new KeyNotFoundException($"Configuration for key '{secretName}' not found.");
                return secret;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Error getting secret: " + ex.Message);
                throw;
            }
        }
    }
}