using System.Text.Json;
using Azure.Messaging.ServiceBus;
using TaskManagementServiceBusApi.AI.DTO;
using TaskManagementServiceBusApi.Configuration;
using TaskManagementServiceBusApi.Worker.Service;

namespace TaskManagementServiceBusApi.AI.Worker
{
    public class AISummaryWorker(ILogger<AISummaryWorker> logger, ServiceBusClient serviceBusClient) : SubscriberWorkerService(
        logger,
        serviceBusClient,
        "tasks",
        "task-summary-worker"
        ), IAIWorker
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _workerTask;
        public async Task HandleCommandAsync(string messageId, string command, string payload, CancellationToken cancellationToken)
        {
            using var activity = Telemetry.ActivitySource.StartActivity("HandleCommandAsync", System.Diagnostics.ActivityKind.Internal);
            activity?.SetTag("messageId", messageId);
            activity?.SetTag("command", command);
            activity?.SetTag("payload", payload);

            _logger.LogInformation("Received command: {Command}", command);
            _logger.LogInformation("Received payload: {Payload}", payload);

            // Simulate some asynchronous work
            await Task.Delay(2000, cancellationToken);

            // You can add more logic here based on the command and payload
        }

        public async Task ProcessMessagesAsync(CancellationToken cancellationToken)
        {
            if (_serviceBusReceiver == null)
            {
                await SubscribeAsync(cancellationToken);
            }

            // Simulate receiving messages
            while (!cancellationToken.IsCancellationRequested && _serviceBusReceiver != null)
            {
                // Handle the received command and payload
                using var activity = Telemetry.ActivitySource.StartActivity("ReceiveMessageAsync", System.Diagnostics.ActivityKind.Consumer);
                var message = await _serviceBusReceiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
                if (message != null)
                {

                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var payload = message.Body.ToObjectFromJson<AISummaryWorkerMessage>(jsonOptions);
                    activity?.SetTag("messageId", message.MessageId);
                    activity?.SetTag("command", payload?.Command);
                    activity?.SetTag("payload", payload?.Data);

                    if (payload != null)
                    {
                        await HandleCommandAsync(message.MessageId, payload.Command, payload.Data, cancellationToken);
                    }

                    // Complete the message after processing
                    await _serviceBusReceiver.CompleteMessageAsync(message, cancellationToken);
                }
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _workerTask = Task.Run(() => ProcessMessagesAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while starting the AI summary worker.");
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
                await UnsubscribeAsync(cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while stopping the AI summary worker.");
                throw;
            }
        }
    }
}