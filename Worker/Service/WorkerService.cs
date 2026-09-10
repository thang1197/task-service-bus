using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using TaskManagementServiceBusApi.Configuration;
using TaskManagementServiceBusApi.Worker.DTO;

namespace TaskManagementServiceBusApi.Worker.Service
{
    public abstract class WorkerService(ILogger<WorkerService> logger, ServiceBusReceiver receiver) : IWorkerService
    {
        protected readonly ILogger<WorkerService> _logger = logger;
        private readonly ServiceBusReceiver _receiver = receiver;
        private static State _state = State.Idle;
        private CancellationTokenSource _cts = new();
        private Task? _processMessageTask;
        private readonly Guid _id = Guid.NewGuid();

        protected abstract Task HandleMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken);

        public async Task<WorkerResult> StartAsync()
        {
            _logger.LogInformation(
                "[{Id}] StartAsync",
                _id);
            var workerResult = new WorkerResult
            {
                IsSuccess = true,
                Message = "Worker started successfully."
            };
            try
            {

                if (_state == State.Started)
                {
                    workerResult.IsSuccess = false;
                    workerResult.Message = "Worker is already started.";
                    return workerResult;
                }

                _cts = new CancellationTokenSource();

                _state = State.Started;

                // Start processing messages in a background task
                _processMessageTask = Task.Run(ProcessMessageAsync);

                return workerResult;
            }
            catch (Exception ex)
            {
                workerResult.IsSuccess = false;
                workerResult.Message = $"Error starting worker: {ex.Message}";
                return workerResult;
            }
        }

        public async Task<WorkerResult> StopAsync()
        {
            _logger.LogInformation(
                "[{Id}] StopAsync",
                _id);
            if (_state != State.Started)
            {
                _logger.LogWarning("Worker service is not started. Current state: {State}", _state);
                return new WorkerResult
                {
                    IsSuccess = false,
                    Message = "Worker service is not started."
                };
            }
            _state = State.Stopped;
            if (_processMessageTask == null)
            {
                _logger.LogWarning("Processing task is null. No task to wait for.");
                return new WorkerResult
                {
                    IsSuccess = true,
                    Message = "Worker service stopped."
                };
            }
            await _processMessageTask;
            _cts.Cancel();
            _logger.LogInformation("Token cancellation requested: {IsCancellationRequested}", _cts.Token.IsCancellationRequested);

            return new WorkerResult
            {
                IsSuccess = true,
                Message = "Worker service stopped."
            };
        }
        private async Task RenewalLockAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken)
        {
            try
            {
                while(!cancellationToken.IsCancellationRequested)
                {
                    var timeToLockExpiration = message.LockedUntil - DateTimeOffset.UtcNow;
                    if (timeToLockExpiration <= TimeSpan.FromSeconds(10))
                    {
                        _logger.LogInformation("Lock for message {MessageId} is about to expire. Renewing lock...", message.MessageId);
                        await _receiver.RenewMessageLockAsync(message, cancellationToken);
                        _logger.LogInformation("Lock renewed for message: {MessageId}", message.MessageId);
                    }
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to renew lock for message: {MessageId}", message.MessageId);
            }
        }

        private async Task ProcessMessageAsync()
        {
            if (_cts == null)
            {
                _logger.LogError("CancellationTokenSource is null. Cannot process messages.");
                return;
            }
            while (!_cts.Token.IsCancellationRequested)
            {
                _logger.LogInformation("Token cancellation requested: {IsCancellationRequested}", _cts.Token.IsCancellationRequested);
                // ServiceBusReceivedMessage? message = null;

                _logger.LogInformation("Waiting for messages...");

                ServiceBusReceivedMessage message = await _receiver.ReceiveMessageAsync(
                    // maxWaitTime: TimeSpan.FromSeconds(30),
                    cancellationToken: _cts.Token);
                
                if (_state == State.Stopped)
                {
                    _logger.LogInformation("Worker service is stopping. Exiting message processing loop.");
                    break;
                }
                var renewalLockCancellationTokenSource = new CancellationTokenSource();
                try
                {

                    _logger.LogInformation(
                    "Receive returned. Null = {Null}, MessageId = {MessageId}, LockDuration = {LockDuration}, ExpiresAt = {ExpiresAt}",
                    message == null,
                    message?.MessageId,
                    message?.LockedUntil,
                    message?.ExpiresAt);

                    if (message == null)
                    {
                        _logger.LogInformation("No message received. Continuing to wait...");
                        continue;
                    }

                    _logger.LogInformation("Processing message: {MessageId}", message.MessageId);
                    _logger.LogInformation("Message body: {MessageBody}", message.Body);
                    var renewalLockTask = Task.Run(() => RenewalLockAsync(message, renewalLockCancellationTokenSource.Token));

                    await HandleMessageAsync(message, _cts.Token);
                    // Stop the lock renewal task after processing the message
                    renewalLockCancellationTokenSource.Cancel();
                    await _receiver.CompleteMessageAsync(message);
                }
                catch (OperationCanceledException ex)
                {
                    // Expected when stopping the worker
                    _logger.LogInformation(ex, "Worker is stopping.");
                    break;
                }
                catch (TransientException ex)
                {
                    _logger.LogWarning(ex, "TransientException occurred while processing message.");
                    await _receiver.AbandonMessageAsync(message);
                }
                catch (ServiceBusException ex)
                {
                    switch (ex.Reason)
                    {
                        case ServiceBusFailureReason.ServiceTimeout:
                            _logger.LogWarning(ex, "Service timeout occurred while processing message.");
                            break;
                        case ServiceBusFailureReason.MessageLockLost:
                            _logger.LogWarning(ex, "Message lock lost while processing message.");
                            await _receiver.AbandonMessageAsync(message);
                            break;
                        default:
                            _logger.LogError(ex, "ServiceBusException occurred while processing message.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing message.");

                    if (message != null)
                    {
                        try
                        {
                            // await _receiver.AbandonMessageAsync(message);
                            renewalLockCancellationTokenSource.Cancel();
                            await _receiver.DeadLetterMessageAsync(message, "ProcessingError", ex.Message);
                        }
                        catch (Exception abandonEx)
                        {
                            _logger.LogError(abandonEx, "Failed to abandon message.");
                        }
                    }
                }
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