using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.Configuration
{
    public class AppConfiguration
    {
        
    }

    public class ServiceBusConfiguration
    {
        public const string SectionName = "ServiceBus";
        public const string KeyVaultSecretKey = "ServiceBusStandardConnectionString";

        public const string KeyVaultAdminSecretKey = "ServiceBusStandardAdminConnectionString";
        public string ConnectionString { get; set; } = string.Empty;
        public string QueueName { get; set; } = string.Empty;

        public string AdminConnectionString { get; set; } = string.Empty;
        
    }

    public class ApiManagementConfiguration
    {
        public const string SectionName = "ApiManagement";
        public string BaseUrl { get; set; } = string.Empty;
        public string SubscriptionKey { get; set; } = string.Empty;
    }

    public class CreateLogAuditConfiguration : ServiceBusConfiguration
    {
        public new const string SectionName = "CreateLogAudit";

        public new const string KeyVaultSecretKey = "ServiceBusCreateAuditLogConnectionString";
    }

    public class StorageAccountConfiguration
    {
        public const string SectionName = "StorageAccount";
        public const string KeyVaultSecretKey = "StorageAccountConnectionString";
        public string ConnectionString { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
    }

    public class AzureAdSettings
    {
        public const string SectionName = "AzureAd";

        public string Instance { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;

        public string ClientSecret { get; set; } = string.Empty;

        public string Scope { get; set; } = string.Empty;
    }

    public class ApplicationInsightsConfigurtion
    {
        public const string SectionName = "ApplicationInsights";

        public const string KeyVaultSecretKey = "ApplicationInsightConnectionString";

        public string ConnectionString { get; set; } = string.Empty;
    }

    public class RedisSettings
    {
       public const string SectionName = "Redis";

        public string Url { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public int Port { get; set; }

        public string Username { get; set; } = string.Empty;
    }

    public class AICoreServiceConfiguration
    {
        public const string SectionName = "AICoreService";

        public string BaseUrl { get; set; } = string.Empty;
    }

    public class FunctionAppCoreConfiguration
    {
        public const string SectionName = "FunctionAppCore";

        public string BaseUrl { get; set; } = string.Empty;
    }

    public class CoreApiConfiguration
    {
        public const string SectionName = "CoreApi";

        public string BaseUrl { get; set; } = string.Empty;
    }
}