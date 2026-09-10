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
using TaskManagementServiceBusApi.AI.Worker;
using TaskManagementServiceBusApi.Audit.Service;
using TaskManagementServiceBusApi.Configuration;
using TaskManagementServiceBusApi.Configuration.TaskManagementApi;
using TaskManagementServiceBusApi.Credential.Service;
using TaskManagementServiceBusApi.Email.Service;
using TaskManagementServiceBusApi.File.Service;
using TaskManagementServiceBusApi.Helper;
using TaskManagementServiceBusApi.ServiceBus.Service;
using TaskManagementServiceBusApi.Worker.Service;

var builder = WebApplication.CreateBuilder(args);

var configurationManager = new CustomConfigurationManager(builder);
await configurationManager.Initialize();

builder.Services.AddSingleton(x =>
{
    return configurationManager.GetAzureCredential();
});


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

builder.Services.Configure<AzureAdSettings>(x =>
{
    var azureAdSettings = configurationManager.GetAzureAdSettings();
    x.Instance = azureAdSettings.Instance;
    x.TenantId = azureAdSettings.TenantId;
    x.ClientId = azureAdSettings.ClientId;
});

builder.Services.Configure<ApiManagementConfiguration>(x =>
{
    var apiManagementConfiguration = configurationManager.GetApiManagementConfiguration();
    x.BaseUrl = apiManagementConfiguration.BaseUrl;
});

builder.Services.ConfigureHttpClientDefaults(builder =>
{
    builder.ConfigureHttpClient((sp, client) =>
    {
        var config = sp.GetRequiredService<IOptions<ApiManagementConfiguration>>().Value;
        client.BaseAddress = new Uri(config.BaseUrl);
    });
});

builder.Services.AddHttpClient();

// Task Management API Service Http Client
builder.Services.AddHttpClient<ITaskManagementApiService, TaskManagementApiService>(client =>
{
    var config = builder.Configuration.GetSection(ApiManagementConfiguration.SectionName).Get<ApiManagementConfiguration>();
    client.BaseAddress = new Uri(config?.BaseUrl ?? string.Empty);
});

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
    var serviceBusConfiguration = sp.GetRequiredService<IOptions<ServiceBusConfiguration>>().Value;
    return new ServiceBusClient(serviceBusConfiguration.ConnectionString);
});

// Storage Account Client
builder.Services.AddSingleton(sp =>
{
    var storageAccountConfiguration = sp.GetRequiredService<IOptions<StorageAccountConfiguration>>().Value;
    Console.WriteLine("StorageAccount ConnectionString: " + storageAccountConfiguration.ConnectionString);
    return new BlobServiceClient(storageAccountConfiguration.ConnectionString);
});

builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IServiceBusService, ServiceBusService>();
builder.Services.AddSingleton<IAuditService, AuditService>();
builder.Services.AddSingleton<IFileService, FileStorageService>();

// Worker
builder.Services.AddSingleton<AISummaryWorker>();
builder.Services.AddSingleton<AIDeliveryWorker>();

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
    var applicationInsightsConfigurtion = configurationManager.GetApplicationInsightsConfigurtion();
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
