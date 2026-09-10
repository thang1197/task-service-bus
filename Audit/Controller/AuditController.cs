
using Microsoft.AspNetCore.Mvc;
using TaskManagementServiceBusApi.Audit.Service;
using TaskManagementServiceBusApi.Helper;

namespace TaskManagementServiceBusApi.Audit.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController(ILogger<AuditController> logger, IAuditService auditService) : ControllerBase
    {
        private readonly ILogger<AuditController> _logger = logger;
        private readonly IAuditService _auditService = auditService;

        [HttpPost("start")]
        public async Task<IActionResult> StartAuditAsync()
        {
            try
            {
                // Implement the logic to start the audit process here
                var result = await _auditService.StartAsync();
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
                var result = await _auditService.StopAsync();
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