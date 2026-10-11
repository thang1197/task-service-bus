using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskManagementServiceBusApi.AI.AIKnowledgeData.Worker;
using TaskManagementServiceBusApi.Helper;

namespace TaskManagementServiceBusApi.AI.AIKnowledgeData.Controller
{
    [ApiController]
    [Route("api/ai-knowledge-data-worker")]
    public class AIKnowledgeDataController(IAIKnowledgeDataWorker aiKnowledgeDataWorker)
    {
        private readonly IAIKnowledgeDataWorker _aiKnowledgeDataWorker = aiKnowledgeDataWorker;
        [HttpPost("start")]
        public async Task<IActionResult> Start()
        {
            try{
                await _aiKnowledgeDataWorker.StartAsync(CancellationToken.None);
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
                await _aiKnowledgeDataWorker.StopAsync(CancellationToken.None);
                return HttpHelper.GenerateResponse("Worker stopped successfully", StatusCodes.Status200OK);
            }
            catch (System.Exception ex)
            {
                return HttpHelper.GenerateErrorResponse($"An error occurred while stopping the worker: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe()
        {
            // await _aiDeliveryWorker.StartAsync(CancellationToken.None);
            try
            {
                await _aiKnowledgeDataWorker.SubscribeAsync(CancellationToken.None);
                return HttpHelper.GenerateResponse("Subscribed successfully", StatusCodes.Status200OK);
            }
            catch (System.Exception ex)
            {               
                return HttpHelper.GenerateErrorResponse($"An error occurred while subscribing: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("unsubscribe")]
        public async Task<IActionResult> Unsubscribe()
        {
            try
            {
                await _aiKnowledgeDataWorker.UnsubscribeAsync(CancellationToken.None);
                return HttpHelper.GenerateResponse("Unsubscribed successfully", StatusCodes.Status200OK);
            }
            catch (System.Exception ex)
            {               
                return HttpHelper.GenerateErrorResponse($"An error occurred while unsubscribing: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }
    }
}