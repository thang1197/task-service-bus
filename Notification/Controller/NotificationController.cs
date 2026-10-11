using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskManagementServiceBusApi.Notification.Service;

namespace TaskManagementServiceBusApi.Notification.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController(INotificationService notificationService) : ControllerBase
    {
        private readonly INotificationService _notificationService = notificationService;
        [HttpGet("start")]
        public async Task<IActionResult> StartAsync()
        {
            try
            {
                var result = await _notificationService.StartAsync();
                return Ok(result);
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        [HttpGet("stop")]
        public async Task<IActionResult> StopAsync()
        {
            try
            {
                var result = await _notificationService.StopAsync();
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}