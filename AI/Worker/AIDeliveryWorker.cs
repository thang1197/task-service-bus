using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using TaskManagementServiceBusApi.AI.DTO;
using TaskManagementServiceBusApi.Configuration;
using TaskManagementServiceBusApi.Configuration.TaskManagementApi;
using TaskManagementServiceBusApi.Worker.DTO;
using TaskManagementServiceBusApi.Worker.Service;

namespace TaskManagementServiceBusApi.AI.Worker
{
    public class AIDeliveryWorker(ILogger<SubscriberWorkerService> logger, ITaskManagementApiService taskManagementApiService, ServiceBusClient serviceBusClient) : SubscriberWokerProcessorService(
        logger, 
        serviceBusClient, 
        "tasks", 
        "task-delivery-worker"), IAIWorker
    {
        private readonly ITaskManagementApiService _taskManagementApiService = taskManagementApiService;
        public async Task GetUserTasksAsync(string messageId, string command, string payload, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handled command: {Command} with payload: {Payload}", command, payload);
                var userTasks = await _taskManagementApiService.GetAllUserTasks(payload, cancellationToken);
                _logger.LogInformation("Retrieved {TaskCount} tasks for project {ProjectId}", userTasks.Count, payload);
                await Task.Delay(1000, cancellationToken); // Simulate some async work
            }
            catch (System.Exception)
            {
                _logger.LogError("Error occurred while handling command: {Command} with payload: {Payload}", command, payload);
                throw;
            }
        }

        private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                if(_serviceBusProcessor != null)
                {
                    _serviceBusProcessor.ProcessMessageAsync += async args =>
                    {
                        var message = args.Message;
                        var jsonOptions = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        var payload = message.Body.ToObjectFromJson<AISummaryWorkerMessage>(jsonOptions);
                        _logger.LogInformation("Received message: {MessageId} with command: {Command} and payload: {Payload}", message.MessageId, payload?.Command, payload?.Data);
                        if (payload != null)
                        {
                            await GetUserTasksAsync(message.MessageId, payload.Command, payload.Data, cancellationToken);
                        }
                        await args.CompleteMessageAsync(message, cancellationToken);
                    };

                    _serviceBusProcessor.ProcessErrorAsync += async args =>
                    {
                        _logger.LogError(args.Exception, "Error occurred while processing messages");
                    };
                }
            }
            catch (System.Exception)
            {
                _logger.LogError("Error occurred while processing messages");
                throw;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await SubscribeAsync(cancellationToken);
                if(_serviceBusProcessor != null)
                {
                    await ProcessMessagesAsync(cancellationToken);
                    await _serviceBusProcessor.StartProcessingAsync(cancellationToken);
                }
            }
            catch (System.Exception)
            {
                _logger.LogError("Error occurred while starting AIDeliveryWorker");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if(_serviceBusProcessor != null)
                {
                    await _serviceBusProcessor.StopProcessingAsync(cancellationToken);
                    await UnsubscribeAsync(cancellationToken);
                }
            }
            catch (System.Exception)
            {
                _logger.LogError("Error occurred while stopping AIDeliveryWorker");
                throw;
            }
        }
    }
}