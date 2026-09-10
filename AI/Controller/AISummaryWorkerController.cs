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
    [Route("api/ai-summary-worker")]
    public class AISummaryWorkerController(AISummaryWorker aiSummaryWorker) : ControllerBase
    {
        private readonly AISummaryWorker _aiSummaryWorker = aiSummaryWorker;
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe()
        {
            // await _aiSummaryWorker.StartAsync(CancellationToken.None);
            try
            {
                await _aiSummaryWorker.SubscribeAsync(CancellationToken.None);
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
                await _aiSummaryWorker.StartAsync(CancellationToken.None);
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
                await _aiSummaryWorker.StopAsync(CancellationToken.None);
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
                var messages = await _aiSummaryWorker.GetMessagesAsync(maxMessageCount, CancellationToken.None);
                return HttpHelper.GenerateResponse(messages, StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                return HttpHelper.GenerateErrorResponse($"An error occurred while retrieving messages: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
            
        }
    }
}