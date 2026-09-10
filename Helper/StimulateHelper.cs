using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.Helper
{
    public class StimulateHelper()
    {
        private static readonly Random _random = new() {};
        private static ILogger<StimulateHelper> _logger = new LoggerFactory().CreateLogger<StimulateHelper>();
        public static async Task StimulateAsyncWork(int delayInSeconds, CancellationToken cancellationToken)
        {
            try
            {
                // Simulate some asynchronous work with a delay
                await Task.Delay(TimeSpan.FromSeconds(delayInSeconds), cancellationToken);
                _logger.LogInformation("Stimulated work completed after {DelayInSeconds} seconds.", delayInSeconds);
            }
            catch (TaskCanceledException)
            {
                // Handle the cancellation if needed
                _logger.LogWarning("Stimulated work was canceled.");
            }
        }

        public static async Task StimulateAsyncWorkRandom(CancellationToken cancellationToken)
        {
            var delayInSeconds = _random.Next(3, 20);
            try
            {
                // Simulate some asynchronous work with a delay
                await Task.Delay(TimeSpan.FromSeconds(delayInSeconds), cancellationToken);
                _logger.LogInformation("Stimulated work completed after {DelayInSeconds} seconds.", delayInSeconds);
            }
            catch (TaskCanceledException)
            {
                // Handle the cancellation if needed
                _logger.LogWarning("Stimulated work was canceled.");
            }
        }
    }
}