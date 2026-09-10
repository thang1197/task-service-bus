using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.Configuration.SecretManager
{
    public interface ISecretManager
    {
        public Task<string> GetSecretAsync(string secretName);
    }
}