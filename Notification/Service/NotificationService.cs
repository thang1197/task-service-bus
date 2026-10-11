using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using TaskManagementServiceBusApi.Worker.Service;

namespace TaskManagementServiceBusApi.Notification.Service
{
    public class NotificationService(ILogger<WorkerService> logger, ServiceBusClient serviceBusClient) : WorkerService(logger,
        serviceBusClient.CreateReceiver("notification", new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        })
    ), INotificationService
    {
        protected override async Task HandleMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken)
        {
            // Implement your notification handling logic here
            _logger.LogInformation("Processing notification message: {MessageId}", message.MessageId);
            _logger.LogInformation("Notification message: {MessageBody}", message.Body);
            // Simulate some processing delay
            await Task.Delay(1000, cancellationToken);
            _logger.LogInformation("Notification message processed: {MessageId}", message.MessageId);
        }
    }
}