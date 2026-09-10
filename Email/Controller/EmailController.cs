using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskManagementServiceBusApi.Email.Service;

namespace TaskManagementServiceBusApi.Email.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController(IEmailService emailService) : ControllerBase
    {
        private readonly IEmailService _emailService = emailService;
        [HttpGet("start")]
        public async Task<IActionResult> StartSendMailAsync()
        {
            try
            {
                var result = await _emailService.StartAsync();
                return Ok(result);
            }
            catch (System.Exception)
            {
                
                throw;
            }
        }

        [HttpGet("stop")]
        public async Task<IActionResult> StopSendMailAsync()
        {
            try
            {
                var result = await _emailService.StopAsync();
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}