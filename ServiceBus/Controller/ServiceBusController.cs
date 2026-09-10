using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TaskManagementServiceBusApi.Helper;
using TaskManagementServiceBusApi.ServiceBus.DTO;
using TaskManagementServiceBusApi.ServiceBus.Service;

namespace TaskManagementServiceBusApi.ServiceBus.Controller
{
    [ApiController]
    [Route("api/servicebus")]
    public class ServiceBusController(IServiceBusService serviceBusService) : ControllerBase
    {
        private readonly IServiceBusService _serviceBusService = serviceBusService;
        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequest messageRequest)
        {
            try
            {
                await _serviceBusService.SendMessageAsync(messageRequest);
                return HttpHelper.GenerateResponse(new { message = "Message sent successfully" });
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse(ex.Message);
            }
        }

        [HttpPost("create-topic")]
        public async Task<IActionResult> CreateTopic([FromBody] TopicRequest topicRequest)
        {
            try
            {
                await _serviceBusService.CreateTopicAsync(topicRequest.TopicName, CancellationToken.None);
                return HttpHelper.GenerateResponse(new { message = "Topic created successfully" });
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse(ex.Message);
            }
        }

        [HttpPost("create-subscription")]
        public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionRequest subscriptionRequest)
        {
            try
            {
                await _serviceBusService.CreateSubscriptionAsync(subscriptionRequest.TopicName, subscriptionRequest.SubscriptionName, CancellationToken.None);
                return HttpHelper.GenerateResponse(new { message = "Subscription created successfully" });
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse(ex.Message);
            }
        }

        [HttpDelete("delete-topic/{topicName}")]
        public async Task<IActionResult> DeleteTopic(string topicName)
        {
            try
            {
                await _serviceBusService.DeleteTopicAsync(topicName, CancellationToken.None);
                return HttpHelper.GenerateResponse(new { message = "Topic deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse(ex.Message);
            }
        }

        [HttpDelete("delete-subscription/{topicName}/{subscriptionName}")]
        public async Task<IActionResult> DeleteSubscription(string topicName, string subscriptionName)
        {
            try
            {
                await _serviceBusService.DeleteSubscriptionAsync(topicName, subscriptionName, CancellationToken.None);
                return HttpHelper.GenerateResponse(new { message = "Subscription deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse(ex.Message);
            }
        }

        [HttpGet("get-subscriptions/{topicName}")]
        public async Task<IActionResult> GetSubscriptions(string topicName)
        {
            try
            {
                var subscriptions = await _serviceBusService.GetSubscriptionsAsync(topicName, CancellationToken.None);
                return HttpHelper.GenerateResponse(subscriptions);
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse(ex.Message);
            }
        }

        [HttpPost("add-subscription-filter")]
        public async Task<IActionResult> AddSubscriptionFilter([FromBody] SubscriptionFilterRequest filterRequest)
        {
            try
            {
                await _serviceBusService.AddSubscriptionFilterAsync(filterRequest.TopicName, filterRequest.SubscriptionName,filterRequest.Filter, filterRequest.Expression, CancellationToken.None);
                return HttpHelper.GenerateResponse(new { message = "Subscription filter added successfully" });
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse(ex.Message);
            }
        }

        [HttpGet("get-rules/{topicName}/{subscriptionName}")]
        public async Task<IActionResult> GetRules(string topicName, string subscriptionName)
        {
            try
            {
                var rules = await _serviceBusService.GetRulesAsync(topicName, subscriptionName, CancellationToken.None);
                return HttpHelper.GenerateResponse(rules);
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse(ex.Message);
            }
        }

        [HttpDelete("remove-subscription-filter/{topicName}/{subscriptionName}/{ruleName}")]
        public async Task<IActionResult> RemoveSubscriptionFilter(string topicName, string subscriptionName, string ruleName)
        {
            try
            {
                await _serviceBusService.RemoveSubscriptionFilterAsync(topicName, subscriptionName, ruleName, CancellationToken.None);
                return HttpHelper.GenerateResponse(new { message = "Subscription filter removed successfully" });
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse(ex.Message);
            }
        }

        // Queue related endpoints
        [HttpPost("queues")]
        public async Task<IActionResult> CreateQueue([FromBody] QueueRequest queueRequest)
        {
            try
            {
                await _serviceBusService.CreateQueueAsync(queueRequest.QueueName, CancellationToken.None);
                return HttpHelper.GenerateResponse(new { message = "Queue created successfully" });
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse(ex.Message);
            }
        }

        [HttpGet("queues")]
        public async Task<IActionResult> GetQueues()
        {
            try
            {
                var queues = await _serviceBusService.GetQueuesAsync(CancellationToken.None);
                return HttpHelper.GenerateResponse(queues);
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse(ex.Message);
            }
        }

        [HttpDelete("queues/{queueName}")]
        public async Task<IActionResult> DeleteQueue(string queueName)
        {
            try
            {
                await _serviceBusService.DeleteQueueAsync(queueName, CancellationToken.None);
                return HttpHelper.GenerateResponse(new { message = "Queue deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse(ex.Message);
            }
        }

        [HttpGet("queues-messages")]
        public async Task<IActionResult> GetMessagesFromQueue([FromQuery] string queueName, [FromQuery] int maxMessageCount)
        {
            try
            {
                if (string.IsNullOrEmpty(queueName)) return HttpHelper.GenerateErrorResponse("Queue name is required", StatusCodes.Status400BadRequest);
                var messages = await _serviceBusService.GetMessagesFromQueueAsync(queueName, maxMessageCount, CancellationToken.None);
                return HttpHelper.GenerateResponse(messages);
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse(ex.Message);
            }
        }

        [HttpGet("queues-messages-dlq")] // Get messages from DLQ
        public async Task<IActionResult> GetMessageFromQueuesDLQ([FromQuery] string queueName, [FromQuery] int maxMessageCount)
        {
            try
            {
                if (string.IsNullOrEmpty(queueName)) return HttpHelper.GenerateErrorResponse("Queue name is required", StatusCodes.Status400BadRequest);
                var messages = await _serviceBusService.GetMessageFromQueuesDLQAsync(queueName, maxMessageCount, CancellationToken.None);
                return HttpHelper.GenerateResponse(messages);
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse(ex.Message);
            }
        }
    }
}