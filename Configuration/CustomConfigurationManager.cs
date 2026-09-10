using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using TaskManagementServiceBusApi.Configuration.ConfigurationFactory;
using TaskManagementServiceBusApi.Configuration.SecretManager;

namespace TaskManagementServiceBusApi.Configuration
{
    public class CustomConfigurationManager(WebApplicationBuilder builder)
    {
        private readonly WebApplicationBuilder _builder = builder;

        private ISecretManager _secretManager = new LocalSecretManager(builder);

        private IConfigurationFactory _configurationFactory = new LocalConfigurationFactory(builder);

        private TokenCredential _azureCredential = new DefaultAzureCredential();

        private readonly ServiceBusConfiguration _serviceBusConfiguration = new();

        private readonly ApiManagementConfiguration _apiManagementConfiguration = new();

        private readonly AzureAdSettings _azureAdSettings = new();

        private readonly ApplicationInsightsConfigurtion _applicationInsightsConfigurtion = new();

        private readonly StorageAccountConfiguration _storageAccountConfiguration = new();

        public async Task Initialize()
        {
            Console.WriteLine("Initialize environment: " + _builder.Environment.EnvironmentName);
            // Azure AD
            _azureAdSettings.Instance = await _configurationFactory.GetConfigurationAsync("AzureAd:Instance");
            _azureAdSettings.TenantId = await _configurationFactory.GetConfigurationAsync("AzureAd:TenantId");
            _azureAdSettings.ClientId = await _configurationFactory.GetConfigurationAsync("AzureAd:ClientId");
            _azureAdSettings.ClientSecret = await _configurationFactory.GetConfigurationAsync("AzureAd:ClientSecret");

            _serviceBusConfiguration.QueueName = await _configurationFactory.GetConfigurationAsync("ServiceBus:QueueName");

            // Azure Storage Account
            _storageAccountConfiguration.ContainerName = await _configurationFactory.GetConfigurationAsync("StorageAccount:ContainerName");
            try
            {
                if (!_builder.Environment.IsDevelopment())
                {
                    // Azure Key Vault
                    var clientSecret = new SecretClient(
                        new Uri("https://thangkeyvault.vault.azure.net/"),
                        _azureCredential
                    );
                    _secretManager = new KeyVaultSecretManager(clientSecret);

                    // Service Bus
                    _serviceBusConfiguration.ConnectionString = await _secretManager.GetSecretAsync(ServiceBusConfiguration.KeyVaultSecretKey);
                    _serviceBusConfiguration.AdminConnectionString = await _secretManager.GetSecretAsync(ServiceBusConfiguration.KeyVaultSecretKey);

                    // Storage Account
                    _storageAccountConfiguration.ConnectionString = await _secretManager.GetSecretAsync(StorageAccountConfiguration.KeyVaultSecretKey);

                    // Application Insights
                    _applicationInsightsConfigurtion.ConnectionString = await _secretManager.GetSecretAsync(ApplicationInsightsConfigurtion.KeyVaultSecretKey);

                }
                else
                {
                    _azureCredential = new ClientSecretCredential(
                        _azureAdSettings.TenantId,
                        _azureAdSettings.ClientId,
                        _azureAdSettings.ClientSecret
                    );
                    // Service Bus
                    _serviceBusConfiguration.ConnectionString = await _secretManager.GetSecretAsync("ServiceBus:ConnectionString");
                    _serviceBusConfiguration.AdminConnectionString = await _secretManager.GetSecretAsync("ServiceBus:AdminConnectionString");

                    // Storage Account
                    _storageAccountConfiguration.ConnectionString = await _secretManager.GetSecretAsync("StorageAccount:ConnectionString");

                    // Application Insights
                    _applicationInsightsConfigurtion.ConnectionString = await _secretManager.GetSecretAsync("ApplicationInsights:ConnectionString");
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Initialize error: " + ex.Message);
                throw;
            }

            // API Management
            _apiManagementConfiguration.BaseUrl = await _configurationFactory.GetConfigurationAsync("ApiManagement:BaseUrl");
            _apiManagementConfiguration.SubscriptionKey = await _configurationFactory.GetConfigurationAsync("ApiManagement:SubscriptionKey");

        }

        public ServiceBusConfiguration GetServiceBusConfiguration()
        {
            return _serviceBusConfiguration;
        }

        public ApiManagementConfiguration GetApiManagementConfiguration()
        {
            return _apiManagementConfiguration;
        }

        public StorageAccountConfiguration GetStorageAccountConfiguration()
        {
            return _storageAccountConfiguration;
        }

        public ApplicationInsightsConfigurtion GetApplicationInsightsConfigurtion()
        {
            return _applicationInsightsConfigurtion;
        }

        public AzureAdSettings GetAzureAdSettings()
        {
            return _azureAdSettings;
        }

        public TokenCredential GetAzureCredential()
        {
            return _azureCredential;
        }
    }
}