using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCommon.Configuration;

namespace TaskManagementServiceBusApi.Configuration
{
    public class CustomConfigurationManager : AppConfigurationManager
    {
        private readonly ServiceBusConfiguration _serviceBusConfiguration = new();

        private readonly StorageAccountConfiguration _storageAccountConfiguration = new();

        private readonly AICoreServiceConfiguration _aiCoreServiceConfiguration = new();

        private readonly FunctionAppCoreConfiguration _functionAppCoreConfiguration = new();

        public override async Task InitializeConfigurationAsync(IHostApplicationBuilder builder)
        {
            try
            {
                await base.InitializeConfigurationAsync(builder);
                _serviceBusConfiguration.ConnectionString = await GetSecretAsync(builder.Environment.IsDevelopment() ? "ServiceBus:ConnectionString" : ServiceBusConfiguration.KeyVaultSecretKey);
                _serviceBusConfiguration.AdminConnectionString = await GetSecretAsync(builder.Environment.IsDevelopment() ? "ServiceBus:AdminConnectionString" : ServiceBusConfiguration.KeyVaultSecretKey);
                _serviceBusConfiguration.QueueName = await GetConfigurationAsync("ServiceBus:QueueName");

                _storageAccountConfiguration.ConnectionString = await GetSecretAsync(builder.Environment.IsDevelopment() ? "StorageAccount:ConnectionString" : StorageAccountConfiguration.KeyVaultSecretKey);
                _storageAccountConfiguration.ContainerName = await GetConfigurationAsync("StorageAccount:ContainerName");

                _aiCoreServiceConfiguration.BaseUrl = await GetConfigurationAsync("AICoreService:BaseUrl");

                _functionAppCoreConfiguration.BaseUrl = await GetConfigurationAsync("FunctionAppCore:BaseUrl");
                Console.WriteLine("Initialize configuration successfully");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Error getting configuration: " + ex.Message);
            }
        }

        public ServiceBusConfiguration GetServiceBusConfiguration()
        {
            return _serviceBusConfiguration;
        }

        public StorageAccountConfiguration GetStorageAccountConfiguration()
        {
            return _storageAccountConfiguration;
        }

        public AICoreServiceConfiguration GetAICoreServiceConfiguration()
        {
            return _aiCoreServiceConfiguration;
        }

        public FunctionAppCoreConfiguration GetFunctionAppCoreConfiguration()
        {
            return _functionAppCoreConfiguration;
        }
    }
}