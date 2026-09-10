using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;

namespace TaskManagementServiceBusApi.Configuration.SecretManager
{
    public class KeyVaultSecretManager(SecretClient secretClient) : ISecretManager
    {
        private readonly SecretClient _secretClient = secretClient;
        public async Task<string> GetSecretAsync(string secretName)
        {
            try
            {
                var secret = await _secretClient.GetSecretAsync(secretName);
                if(string.IsNullOrEmpty(secret.Value.Value)) throw new KeyNotFoundException($"Configuration for key '{secretName}' not found.");
                return secret.Value.Value;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Error getting secret: " + ex.Message);
                throw;
            }
        }
    }
}