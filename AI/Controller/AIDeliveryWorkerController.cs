using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskManagementServiceBusApi.AI.Worker;
using TaskManagementServiceBusApi.Helper;

namespace TaskManagementServiceBusApi.AI.Controller
{
    [ApiController]
    [Route("api/ai-delivery-worker")]
    public class AIDeliveryWorkerController(AIDeliveryWorker aiDeliveryWorker) : ControllerBase
    {
        private readonly AIDeliveryWorker _aiDeliveryWorker = aiDeliveryWorker;
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe()
        {
            // await _aiDeliveryWorker.StartAsync(CancellationToken.None);
            try
            {
                await _aiDeliveryWorker.SubscribeAsync(CancellationToken.None);
                return HttpHelper.GenerateResponse("Subscribed successfully", StatusCodes.Status200OK);
            }
            catch (System.Exception ex)
            {               
                return HttpHelper.GenerateErrorResponse($"An error occurred while subscribing: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start()
        {
            try{
                await _aiDeliveryWorker.StartAsync(CancellationToken.None);
                return HttpHelper.GenerateResponse("Worker started successfully", StatusCodes.Status200OK);
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse($"An error occurred while starting the worker: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("stop")]
        public async Task<IActionResult> Stop()
        {
            try
            {
                await _aiDeliveryWorker.StopAsync(CancellationToken.None);
                return HttpHelper.GenerateResponse("Worker stopped successfully", StatusCodes.Status200OK);
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse($"An error occurred while stopping the worker: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages(int maxMessageCount = 10)
        {
            try
            {
                var messages = await _aiDeliveryWorker.GetMessagesAsync(maxMessageCount, CancellationToken.None);
                return HttpHelper.GenerateResponse(messages, StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                return HttpHelper.GenerateErrorResponse($"An error occurred while retrieving messages: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
            
        }
    }
}