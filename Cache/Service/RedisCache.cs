using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace TaskManagementServiceBusApi.Cache.Service
{
    public class RedisCache(IConnectionMultiplexer connection) : IDistributedCache
    {
        private readonly StackExchange.Redis.IDatabase _redis = connection.GetDatabase();

        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _redis.StringGetAsync(key);

            if (value.IsNullOrEmpty)
            {
                return default;
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)value.ToString();
            }
            return JsonSerializer.Deserialize<T>((string)value!, _serializerOptions);
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _redis.KeyDeleteAsync(key);
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                string serializedValue = value is string str
                    ? str
                    : JsonSerializer.Serialize(value, _serializerOptions);

                Expiration expiration =  expiry is not null && expiry.HasValue ? expiry.Value : default;

                await _redis.StringSetAsync(key, serializedValue, expiration);
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }
}