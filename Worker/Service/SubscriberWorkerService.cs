using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using TaskManagementServiceBusApi.Worker.DTO;

namespace TaskManagementServiceBusApi.Worker.Service
{
    public abstract class SubscriberWorkerService(
        ILogger<SubscriberWorkerService> logger, 
        ServiceBusClient serviceBusClient,
        SubscriberWorkerServiceOptions options
        ) : ISubscriberWorkerService
    {
        protected readonly ILogger<SubscriberWorkerService> _logger = logger;
        // private readonly string _mode = mode;
        // protected readonly string _topicName = topicName;
        // protected readonly string _subscriptionName = subscriptionName;

        protected readonly SubscriberWorkerServiceOptions _options = options;
        protected readonly ServiceBusClient _serviceBusClient = serviceBusClient;
        protected ServiceBusReceiver? _serviceBusReceiver;

        protected SubscriberState _state = SubscriberState.Unsubscribed;

        private Task? _workerTask;
        private CancellationTokenSource? _cancellationTokenSource;

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
                _serviceBusReceiver = _serviceBusClient.CreateReceiver(_options.TopicName, _options.SubscriptionName, _options.ReceiverOptions);
                _state = SubscriberState.Subscribed;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while subscribing to topic: {TopicName}, subscription: {SubscriptionName}", _options.TopicName, _options.SubscriptionName);
                throw ;
            }
        }

        public async Task UnsubscribeAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_serviceBusReceiver != null)
                {
                    await _serviceBusReceiver.CloseAsync(cancellationToken);
                    _serviceBusReceiver = null;
                }
                _state = SubscriberState.Unsubscribed;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while unsubscribing from topic: {TopicName}, subscription: {SubscriptionName}", _options.TopicName, _options.SubscriptionName);
                throw ;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if(_state == SubscriberState.Started) return;

                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                if(_state != SubscriberState.Unsubscribed) await SubscribeAsync(_cancellationTokenSource.Token);
                _workerTask = Task.Run(() => ProcessMessagesAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
                _state = SubscriberState.Started;
            }
            catch (System.Exception)
            {
                
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                if (_workerTask != null)
                {
                    await _workerTask;
                }
                _state = SubscriberState.Stopped;
            }
            catch (System.Exception)
            {
                
                throw;
            }
        }

        public abstract Task ProcessMessagesAsync(CancellationToken cancellationToken);
    }

    public enum SubscriberState
    {
        Started,
        Stopped,
        Subscribed,
        Unsubscribed
    }

    public class SubscriberWorkerServiceOptions
    {
        public string TopicName { get; set; } = string.Empty;
        public string SubscriptionName { get; set; } = string.Empty;
        
        public ServiceBusReceiverOptions ReceiverOptions { get; set; } = new ServiceBusReceiverOptions();
    }
}