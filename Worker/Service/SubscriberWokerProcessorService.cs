using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using TaskManagementServiceBusApi.Worker.DTO;

namespace TaskManagementServiceBusApi.Worker.Service
{
    public class SubscriberWokerProcessorService(
         ILogger<SubscriberWorkerService> logger,
        ServiceBusClient serviceBusClient,
        string topicName,
        string subscriptionName,
        string mode = "PeekLock",
        int maxConcurrentCalls = 1
    ) : ISubscriberWorkerService
    {
        protected readonly ILogger<SubscriberWorkerService> _logger = logger;
        private readonly string _mode = mode;

        protected readonly int _maxConcurrentCalls = maxConcurrentCalls;
        protected readonly string _topicName = topicName;
        protected readonly string _subscriptionName = subscriptionName;
        protected ServiceBusProcessor? _serviceBusProcessor;
        protected ServiceBusReceiver? _serviceBusReceiver;

        private readonly ServiceBusClient _serviceBusClient = serviceBusClient;
        public async Task<List<Message>> GetMessagesAsync(int maxMessageCount, CancellationToken cancellationToken)
        {
            try
            {
                if (_serviceBusReceiver == null)
                {
                    throw new InvalidOperationException("ServiceBusReceiver is not initialized. Please subscribe to a topic and subscription first.");
                }

                var messages = await _serviceBusReceiver.PeekMessagesAsync(maxMessages: maxMessageCount, fromSequenceNumber: 1, cancellationToken: cancellationToken);

                _logger.LogInformation("Peeked {MessageCount} messages from topic: {FullyQualifiedNamespace}, subscription: {EntityPath}", messages.Count, _serviceBusReceiver.FullyQualifiedNamespace, _serviceBusReceiver.EntityPath);
                return [.. messages.Select(m => new Message
                {
                    Id = m.MessageId,
                    Payload = m.Body.ToString(),
                    State = m.State.ToString(),
                    DeliveryCount = m.DeliveryCount,
                    Subject = m.Subject
                })];
            }
            catch (ServiceBusException sbEx)
            {
                _logger.LogError(sbEx, "Service Bus error occurred while receiving messages");
                // Handle Service Bus specific exceptions
                throw;
            }
            catch (System.Exception)
            {
                
                throw;
            }
        }

        public async Task SubscribeAsync(CancellationToken cancellationToken)
        {
            try
            {
                _serviceBusReceiver ??= _serviceBusClient.CreateReceiver(_topicName, _subscriptionName);
                _serviceBusProcessor ??= _serviceBusClient.CreateProcessor(
                    _topicName, 
                    _subscriptionName,
                    new ServiceBusProcessorOptions
                    {
                        AutoCompleteMessages = false,
                        MaxConcurrentCalls = _maxConcurrentCalls,
                        ReceiveMode = _mode.Equals("ReceiveAndDelete", StringComparison.OrdinalIgnoreCase) ? ServiceBusReceiveMode.ReceiveAndDelete : ServiceBusReceiveMode.PeekLock
                    }
                );
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while subscribing to topic: {TopicName}, subscription: {SubscriptionName}", _topicName, _subscriptionName);
                throw;
            }
        }

        public async Task UnsubscribeAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_serviceBusProcessor != null)
                {
                    await _serviceBusProcessor.CloseAsync(cancellationToken);
                    _serviceBusProcessor = null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while unsubscribing from topic: {TopicName}, subscription: {SubscriptionName}", _topicName, _subscriptionName);
                throw;
            }
        }
    }
}