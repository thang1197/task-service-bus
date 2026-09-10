using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.Configuration
{
    public class ExceptionHandler
    {
        public static bool IsHttpTransientError(HttpRequestException ex)
        {
            // 1. Check StatusCode (if response was received)
            if (ex.StatusCode.HasValue)
            {
                int code = (int)ex.StatusCode.Value;
                return code == 408 // Request Timeout
                    || code == 429 // Too Many Requests (Rate Limit)
                    || code >= 500; // 500 Internal Server, 502 Bad Gateway, 503 Service Unavailable, 504 Gateway Timeout
            }

            // 2. Check HttpRequestError (.NET 7+) for network-level failures without HTTP response (e.g., DNS, connection dropped)
            return ex.HttpRequestError == HttpRequestError.NameResolutionError
                || ex.HttpRequestError == HttpRequestError.ConnectionError
                || ex.HttpRequestError == HttpRequestError.HttpProtocolError;
        }
    }

    public class TransientException(string? message, Exception? innerException) : Exception(message, innerException)
    {
    }

    public class BusinessException(string? message, Exception? innerException) : Exception(message, innerException)
    {
    }
}