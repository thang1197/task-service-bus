using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskManagementServiceBusApi.File.Service;
using TaskManagementServiceBusApi.Helper;

namespace TaskManagementServiceBusApi.File.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController(ILogger<FileController> logger, IFileService fileService) : ControllerBase
    {
        private readonly IFileService _fileService = fileService;
        private readonly ILogger<FileController> _logger = logger;
        [HttpPost("start")]
        public async Task<IActionResult> StartAuditAsync()
        {
            try
            {
                // Implement the logic to start the audit process here
                var result = await _fileService.StartAsync();
                return HttpHelper.GenerateResponse(result, StatusCodes.Status202Accepted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while starting the audit process.");
                return HttpHelper.GenerateErrorResponse(ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("stop")]
        public async Task<IActionResult> StopAuditAsync()
        {
            try
            {
                // Implement the logic to stop the audit process here
                var result = await _fileService.StopAsync();
                return HttpHelper.GenerateResponse(result, StatusCodes.Status202Accepted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while stopping the audit process.");
                return HttpHelper.GenerateErrorResponse(ex.Message, StatusCodes.Status500InternalServerError);
            }
        }
    }
}