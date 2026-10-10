using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using SharedCommon.Cache.Service;
using SharedCommon.Configuration;
using SharedCommon.CoreApi;
using SharedCommon.CoreApi.Files.Service;
using StackExchange.Redis;
using TaskManagementServiceBusApi.AI.AIKnowledgeData.Service;
using TaskManagementServiceBusApi.AI.AIKnowledgeData.Worker;
using TaskManagementServiceBusApi.AI.Worker;
using TaskManagementServiceBusApi.AICoreService;
using TaskManagementServiceBusApi.AICoreService.Service;
using TaskManagementServiceBusApi.Audit.Service;
using TaskManagementServiceBusApi.Cache.Service;
using TaskManagementServiceBusApi.Configuration;
using TaskManagementServiceBusApi.Credential.Service;
using TaskManagementServiceBusApi.Email.Service;
using TaskManagementServiceBusApi.File.Service;
using TaskManagementServiceBusApi.File.Worker;
using TaskManagementServiceBusApi.FunctionAppCore;
using TaskManagementServiceBusApi.FunctionAppCore.Storage.Service;
using TaskManagementServiceBusApi.Helper;
using TaskManagementServiceBusApi.ServiceBus.Service;
using TaskManagementServiceBusApi.Worker.Service;

var builder = WebApplication.CreateBuilder(args);

var configurationManager = new CustomConfigurationManager();
await configurationManager.InitializeConfigurationAsync(builder);

// CredentialCache
builder.Services.AddSingleton<ICredentialCache, CredentialCache>();

// builder.Services.AddSingleton(x =>
// {
//     return configurationManager.GetAzureCredential();
// });


// Handle configuration
builder.Services.Configure<ServiceBusConfiguration>(x =>
{
    var serviceBusConfiguration = configurationManager.GetServiceBusConfiguration();
    x.ConnectionString = serviceBusConfiguration.ConnectionString;
    x.AdminConnectionString = serviceBusConfiguration.AdminConnectionString;
    x.QueueName = serviceBusConfiguration.QueueName;
});

builder.Services.Configure<StorageAccountConfiguration>(x =>
{
    var storageAccountConfiguration = configurationManager.GetStorageAccountConfiguration();
    x.ConnectionString = storageAccountConfiguration.ConnectionString;
    x.ContainerName = storageAccountConfiguration.ContainerName;
});

builder.Services.AddHttpClient();

// Credential Manager
builder.Services.AddSingleton<ICredentialManager>(sp =>
{
    var azureAdSettings = sp.GetRequiredService<IOptions<AzureAdSettings>>().Value;
    var logger = sp.GetRequiredService<ILogger<DefaultAzureCredential>>();
    return new CredentialManger(logger, azureAdSettings);
});

// Service Bus Client
builder.Services.AddSingleton(sp =>
{
    var serviceBusConfiguration = configurationManager.GetServiceBusConfiguration();
    Console.WriteLine("ServiceBus ConnectionString: " + serviceBusConfiguration.ConnectionString);
    return new ServiceBusClient(serviceBusConfiguration.ConnectionString);
});

// Storage Account Client
builder.Services.AddSingleton(sp =>
{
    var storageAccountConfiguration = configurationManager.GetStorageAccountConfiguration();
    Console.WriteLine("StorageAccount ConnectionString: " + storageAccountConfiguration.ConnectionString);
    return new BlobServiceClient(storageAccountConfiguration.ConnectionString);
});

builder.Services.AddControllers();

// Function App Core
builder.Services.AddHttpClient<FunctionAppCoreHttpClient>(opts =>
{
    opts.Timeout = TimeSpan.FromSeconds(20);
    opts.BaseAddress = new Uri(configurationManager.GetFunctionAppCoreConfiguration().BaseUrl);
});
builder.Services.AddSingleton<IFunctionAppCoreStorageService, FunctionAppCoreStorageService>();


// Add services to the container.
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IServiceBusService, ServiceBusService>();
builder.Services.AddSingleton<IAuditService, AuditService>();

// Core Api Serivce
builder.Services.AddHttpClient<CoreApiHttpClient>(opts =>
{
    var coreApiBaseUrl = configurationManager.GetApiManagementConfiguration().BaseUrl;
    Console.WriteLine("Core API BaseUrl: " + coreApiBaseUrl);
    opts.BaseAddress = new Uri(coreApiBaseUrl);
    opts.Timeout = TimeSpan.FromSeconds(20);
});
builder.Services.AddSingleton<ICoreApiFileService, CoreApiFileService>();

// File
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddSingleton<IFileWorker, FileWorker>();

// AI Service
var aiServiceBaseUrl = configurationManager.GetAICoreServiceConfiguration().BaseUrl;
builder.Services.AddHttpClient<AICoreServiceHttpClient>(opts =>
{
    Console.WriteLine("AI Service BaseUrl: " + aiServiceBaseUrl);
    opts.BaseAddress = new Uri(aiServiceBaseUrl);
    opts.Timeout = TimeSpan.FromSeconds(20);
});
builder.Services.AddSingleton<IAIService, AIService>();


// AI Knowledge Data Worker
builder.Services.AddSingleton<IAIKnowledgeService, AIKnowledgeService>();
builder.Services.AddSingleton<IAIKnowledgeDataWorker, AIKnowledgeDataWorker>();


builder.Services.AddSingleton(sp =>
{
    var serviceBusConfiguration = sp.GetRequiredService<IOptions<ServiceBusConfiguration>>().Value;
    // var connectionString = "Endpoint=sb://localhost:5300;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
    var adminClient = new ServiceBusAdministrationClient(serviceBusConfiguration.AdminConnectionString, new ServiceBusAdministrationClientOptions
    {
        Retry = { Mode = RetryMode.Exponential, MaxRetries = 3, Delay = TimeSpan.FromSeconds(2), MaxDelay = TimeSpan.FromSeconds(30) }
    });
    Console.WriteLine("ServiceBusAdministrationClient created with connection string: " + serviceBusConfiguration.AdminConnectionString);
    return adminClient;
});

// Application Insights
builder.Services
.AddOpenTelemetry()
.UseAzureMonitor(options =>
{
    var applicationInsightsConfigurtion = configurationManager.GetApplicationInsightConfiguration();
    Console.WriteLine("ApplicationInsights ConnectionString: " + applicationInsightsConfigurtion.ConnectionString);
    options.ConnectionString = applicationInsightsConfigurtion.ConnectionString;
})
.WithTracing(builder =>
{
    builder.AddSource(Telemetry.ActivitySource.Name);
})
.ConfigureResource(resource =>
{
    resource.AddService(
        serviceName: Telemetry.ServiceName,
        serviceVersion: "1.0.0",
        serviceNamespace: "task-management"
    );
});

var app = builder.Build();

app.MapControllers();
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.Run();
