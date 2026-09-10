using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using TaskManagementServiceBusApi.Email.DTO;
using TaskManagementServiceBusApi.Helper;
using TaskManagementServiceBusApi.Worker.Service;

namespace TaskManagementServiceBusApi.Email.Service
{
    public class EmailService(ILogger<EmailService> logger, ServiceBusClient serviceBusClient) : WorkerService(logger, serviceBusClient.CreateReceiver("send-email", new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        })), IEmailService
    {
        // private readonly ILogger<EmailService> _logger = logger;

        protected override async Task HandleMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken)
        {
            // Simulate sending an email
            try
            {
                _logger.LogInformation("Sending email...");
                await StimulateHelper.StimulateAsyncWorkRandom(cancellationToken);
                _logger.LogInformation("Email sent successfully.");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email.");
                throw new Exception("Error occurred while sending email.", ex);
            }
        }
    }

    enum State
    {
        Started,
        Stopped,
        Idle
    }
}