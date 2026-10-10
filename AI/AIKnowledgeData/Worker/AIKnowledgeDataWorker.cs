using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using TaskManagementServiceBusApi.AI.AIKnowledgeData.DTO;
using TaskManagementServiceBusApi.AI.AIKnowledgeData.Service;
using TaskManagementServiceBusApi.AICoreService.Service;
using TaskManagementServiceBusApi.Helper;
using TaskManagementServiceBusApi.Worker.Service;

namespace TaskManagementServiceBusApi.AI.AIKnowledgeData.Worker
{
    public class AIKnowledgeDataWorker(ILogger<AIKnowledgeDataWorker> logger, ServiceBusClient serviceBusClient, IAIKnowledgeService aiKnowledgeService) :
    SubscriberWorkerService(logger, serviceBusClient, new SubscriberWorkerServiceOptions
    {
        TopicName = "tasks",
        SubscriptionName = "ai-knowledge-data",
        ReceiverOptions = new ServiceBusReceiverOptions { 
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        }
    }),
    IAIKnowledgeDataWorker
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
        private readonly IAIKnowledgeService _aiKnowledgeService = aiKnowledgeService;
        public override async Task ProcessMessagesAsync(CancellationToken cancellationToken)
        {
            if (_serviceBusReceiver == null)
            {
                throw new InvalidOperationException("ServiceBusReceiver is not initialized. Please subscribe to a topic and subscription first.");
            }
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Waiting for messages...");
                var message = await _serviceBusReceiver.ReceiveMessageAsync(cancellationToken: cancellationToken);

                if (message == null)
                {
                    continue;
                }

                try
                {
                    _logger.LogInformation("Processing message: {MessageId}", message.MessageId);
                    _logger.LogInformation("Message: {MessageBody}", message.Body);
                    var task = message.Body.ToObjectFromJson<AIKnowledgeMessage>(_jsonSerializerOptions);

                    if (task == null)
                    {
                        _logger.LogError("Failed to deserialize message body.");
                        continue;
                    }

                    // Process the message
                    _logger.LogInformation("Processing message: {MessageId}", task);

                    // Assign the EventId from the message to the task object
                    task.EventId = message.MessageId;
                    var result = await _aiKnowledgeService.UpsertAIKnowledgeAsync(task);

                    await _serviceBusReceiver.CompleteMessageAsync(message, cancellationToken);
                    _logger.LogInformation("Message processed: {MessageId}", message.MessageId);
                }
                catch (System.Exception ex)
                {
                    var kind = FailureClassifier.Classify(ex);
                    _logger.LogError(ex, "Error occurred while processing messages. Kind: {Kind}", kind);
                    if (kind == FailureKind.Permanent)
                    {
                        await _serviceBusReceiver.DeadLetterMessageAsync(message, cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await _serviceBusReceiver.AbandonMessageAsync(message, cancellationToken: cancellationToken);
                    }
                }
            }

        }
    }
}