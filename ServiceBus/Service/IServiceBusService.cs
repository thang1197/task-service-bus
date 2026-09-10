using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.ServiceBus.DTO;

namespace TaskManagementServiceBusApi.ServiceBus.Service
{
    public interface IServiceBusService
    {
        public Task SendMessageAsync(MessageRequest messageRequest);

        public Task CreateTopicAsync(string topicName, CancellationToken cancellationToken);

        public Task<List<TopicResponse>> GetTopicsAsync(CancellationToken cancellationToken);

        public Task CreateQueueAsync(string queueName, CancellationToken cancellationToken);

        public Task<List<QueueResponse>> GetQueuesAsync(CancellationToken cancellationToken);

        public Task DeleteQueueAsync(string queueName, CancellationToken cancellationToken);

        public Task CreateSubscriptionAsync(string topicName, string subscriptionName, CancellationToken cancellationToken);

        public Task DeleteTopicAsync(string topicName, CancellationToken cancellationToken);

        public Task DeleteSubscriptionAsync(string topicName, string subscriptionName, CancellationToken cancellationToken);

        public Task AddSubscriptionFilterAsync(string topicName, string subscriptionName, string filter, string expression, CancellationToken cancellationToken);

        public Task<List<RuleResponse>> GetRulesAsync(string topicName, string subscriptionName, CancellationToken cancellationToken);

        public Task<List<SubscriptionResponse>> GetSubscriptionsAsync(string topicName, CancellationToken cancellationToken);

        public Task RemoveSubscriptionFilterAsync(string topicName, string subscriptionName, string ruleName, CancellationToken cancellationToken);

        public Task<List<MessageResponse>> GetMessagesFromQueueAsync(string queueName, int maxMessageCount, CancellationToken cancellationToken);

        public Task<List<MessageResponse>> GetMessageFromQueuesDLQAsync(string queueName, int maxMessageCount, CancellationToken cancellationToken);
    }
}