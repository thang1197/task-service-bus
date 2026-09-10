using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using TaskManagementServiceBusApi.Configuration;
using TaskManagementServiceBusApi.Credential.Service;
using TaskManagementServiceBusApi.Worker.Service;

namespace TaskManagementServiceBusApi.Audit.Service
{
    public class AuditService(ILogger<AuditService> logger, ServiceBusClient serviceBusClient, HttpClient httpClient, ICredentialManager credentialManager) : WorkerService(logger, serviceBusClient.CreateReceiver("create-audit-log", new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        })), IAuditService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ICredentialManager _credentialManager = credentialManager;
        public Task CreateAuditLogAsync(string action, string entity, string userId, string details)
        {
            throw new NotImplementedException();
        }

        protected override async Task HandleMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var messageBody = message.Body.ToString();
                _logger.LogInformation("Received message: {MessageBody}", messageBody);
                var credential = await _credentialManager.GetCredential();
                if(string.IsNullOrEmpty(credential)) throw new Exception("Credential is null or empty.");
                
                var request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(messageBody, System.Text.Encoding.UTF8, "application/json"),
                    RequestUri = new Uri("/task-api/v1/auditlog", UriKind.Relative)
                };
                request.Headers.Add("Authorization", $"Bearer {credential}");

                var result = await _httpClient.SendAsync(request, cancellationToken);

                result.EnsureSuccessStatusCode();
                _logger.LogInformation("Audit log created successfully for message: {MessageBody}", messageBody);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HttpRequestException occurred while creating audit log");
                if (ExceptionHandler.IsHttpTransientError(ex))
                {

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    throw new TransientException($"HttpRequestException occurred while creating audit log: {ex.Message}", ex);
                }
                throw new BusinessException($"HttpRequestException occurred while creating audit log: {ex.Message}", ex);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "SystemException occurred while creating audit log");
                throw new Exception($"SystemException occurred while creating audit log: {ex.Message}", ex);
            }
        }
    }
}