using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using TaskManagementServiceBusApi.ServiceBus.DTO;

namespace TaskManagementServiceBusApi.ServiceBus.Service
{
    public class ServiceBusService(ServiceBusClient serviceBusClient, ServiceBusAdministrationClient adminClient, ILogger<ServiceBusService> logger) : IServiceBusService
    {
        // create-audit-log, file-queue
        private readonly ServiceBusSender _sender = serviceBusClient.CreateSender("tasks");
        private readonly ServiceBusClient _serviceBusClient = serviceBusClient;
        private readonly ServiceBusAdministrationClient _adminClient = adminClient;

        private readonly ILogger<ServiceBusService> _logger = logger;

        public async Task CreateSubscriptionAsync(string topicName, string subscriptionName, CancellationToken cancellationToken)
        {
            try
            {
                await _adminClient.CreateSubscriptionAsync(topicName, subscriptionName, cancellationToken: cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating subscription {SubscriptionName} for topic {TopicName}", subscriptionName, topicName);
                throw;
            }
        }

        public async Task CreateTopicAsync(string topicName, CancellationToken cancellationToken)
        {
            try
            {
                await _adminClient.CreateTopicAsync(topicName, cancellationToken: cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating topic {TopicName}", topicName);
                throw;
            }
        }

        public async Task DeleteSubscriptionAsync(string topicName, string subscriptionName, CancellationToken cancellationToken)
        {
            try
            {
                await _adminClient.DeleteSubscriptionAsync(topicName, subscriptionName, cancellationToken: cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting subscription {SubscriptionName} for topic {TopicName}", subscriptionName, topicName);
                throw;
            }
        }

        public async Task DeleteTopicAsync(string topicName, CancellationToken cancellationToken)
        {
            try
            {
                await _adminClient.DeleteTopicAsync(topicName, cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting topic {TopicName}", topicName);
                throw;
            }
        }

        public async Task AddSubscriptionFilterAsync(string topicName, string subscriptionName, string filter, string expression, CancellationToken cancellationToken)
        {
            try
            {
                await _adminClient.CreateRuleAsync(topicName, subscriptionName, new CreateRuleOptions(filter, new SqlRuleFilter(expression)), cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding subscription filter for topic {TopicName} and subscription {SubscriptionName}", topicName, subscriptionName);
                throw;
            }
        }

        public async Task SendMessageAsync(MessageRequest messageRequest)
        {
            try
            {
                CancellationTokenSource cts = new();
                var message = new ServiceBusMessage
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    MessageId = Guid.NewGuid().ToString(),
                    Body = new BinaryData(messageRequest.Payload.GetRawText()),
                    ContentType = "application/json",
                    Subject = messageRequest.Subject,
                    SessionId = messageRequest.SessionId,
                };
                await _sender.SendMessageAsync(message, cts.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending message to Service Bus");
                // Handle the exception as needed
                throw;
            }
        }

        public async Task<List<RuleResponse>> GetRulesAsync(string topicName, string subscriptionName, CancellationToken cancellationToken)
        {
            var ruleResponses = new List<RuleResponse>();
            try
            {
                var rules = _adminClient.GetRulesAsync(topicName, subscriptionName, cancellationToken);
                await foreach (var rule in rules)
                {
                    ruleResponses.Add(new RuleResponse
                    {
                        RuleName = rule.Name,
                        Filter = rule.Filter.ToString() ?? string.Empty
                    });
                }
                return ruleResponses;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving rules for topic {TopicName} and subscription {SubscriptionName}", topicName, subscriptionName);
                throw;
            }
        }

        public async Task<List<SubscriptionResponse>> GetSubscriptionsAsync(string topicName, CancellationToken cancellationToken)
        {
            var subscriptionResponses = new List<SubscriptionResponse>();
            try
            {
                var subscriptions = _adminClient.GetSubscriptionsAsync(topicName, cancellationToken);
                await foreach (var subscription in subscriptions)
                {
                    subscriptionResponses.Add(new SubscriptionResponse
                    {
                        SubscriptionName = subscription.SubscriptionName,
                        TopicName = subscription.TopicName,
                        Status = subscription.Status.ToString()
                    });
                }
                return subscriptionResponses;
            }
            catch (System.Exception)
            {
                _logger.LogError("Error occurred while retrieving subscriptions for topic {TopicName}", topicName);
                throw;
            }
        }

        public async Task RemoveSubscriptionFilterAsync(string topicName, string subscriptionName, string ruleName, CancellationToken cancellationToken)
        {
            try
            {
                await _adminClient.DeleteRuleAsync(topicName, subscriptionName, ruleName, cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing subscription filter {RuleName} for topic {TopicName} and subscription {SubscriptionName}", ruleName, topicName, subscriptionName);
                throw;
            }
        }

        public async Task<List<TopicResponse>> GetTopicsAsync(CancellationToken cancellationToken)
        {
            var topicResponses = new List<TopicResponse>();
            try
            {
                var topics = _adminClient.GetTopicsAsync(cancellationToken);
                await foreach (var topic in topics)
                {
                    topicResponses.Add(new TopicResponse
                    {
                        TopicName = topic.Name,
                        Status = topic.Status.ToString()
                    });
                }
                return topicResponses;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving topics");
                throw;
            }
        }

        public async Task CreateQueueAsync(string queueName, CancellationToken cancellationToken)
        {
            try
            {
                CreateQueueOptions queueOptions = new(queueName)
                {
                    MaxDeliveryCount = 3,
                    LockDuration = TimeSpan.FromMinutes(1)
                };
                await _adminClient.CreateQueueAsync(queueOptions, cancellationToken: cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating queue {QueueName}", queueName);
                throw;
            }
        }

        public async Task<List<QueueResponse>> GetQueuesAsync(CancellationToken cancellationToken)
        {
            var queueResponses = new List<QueueResponse>();
            try
            {
                var queues = _adminClient.GetQueuesAsync(cancellationToken);
                await foreach (var queue in queues)
                {
                    queueResponses.Add(new QueueResponse
                    {
                        QueueName = queue.Name,
                        Status = queue.Status.ToString()
                    });
                }
                return queueResponses;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving queues");
                throw;
            }
        }

        public async Task DeleteQueueAsync(string queueName, CancellationToken cancellationToken)
        {
            try
            {
                await _adminClient.DeleteQueueAsync(queueName, cancellationToken: cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting queue {QueueName}", queueName);
                throw;
            }
        }

        public async Task<List<MessageResponse>> GetMessagesFromQueueAsync(string queueName, int maxMessageCount, CancellationToken cancellationToken)
        {
            try
            {
                var receiver = _serviceBusClient.CreateReceiver(queueName);
                var messages = await receiver.PeekMessagesAsync(maxMessageCount, cancellationToken: cancellationToken, fromSequenceNumber: 0);

                var messageResponses = messages.Select(message => new MessageResponse
                {
                    MessageId = message.MessageId,
                    Body = message.Body.ToString(),
                    ContentType = message.ContentType,
                    CorrelationId = message.CorrelationId
                }).ToList();

                return messageResponses;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving messages from queue {QueueName}", queueName);
                throw;
            }
        }

        public async Task<List<MessageResponse>> GetMessageFromQueuesDLQAsync(string queueName, int maxMessageCount, CancellationToken cancellationToken)
        {
            try
            {
                var receiver = _serviceBusClient.CreateReceiver(queueName, new ServiceBusReceiverOptions
                {
                    SubQueue = SubQueue.DeadLetter
                });
                var messages = await receiver.PeekMessagesAsync(maxMessageCount, cancellationToken: cancellationToken, fromSequenceNumber: 0);

                var messageResponses = messages.Select(message => new MessageResponse
                {
                    MessageId = message.MessageId,
                    Body = message.Body.ToString(),
                    ContentType = message.ContentType,
                    CorrelationId = message.CorrelationId
                }).ToList();

                return messageResponses;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving messages from queue {QueueName}", queueName);
                throw;
            }
        }
    }
}