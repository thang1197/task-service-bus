using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCommon.Cache.Service;

namespace TaskManagementServiceBusApi.Configuration
{
    public class LocalDistributedCache : IDistributedCache
    {
        public async Task<T?> GetAsync<T>(string key)
        {
            return default;
        }

        public Task RemoveAsync(string key)
        {
            throw new NotImplementedException();
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry)
        {
        }
    }
}