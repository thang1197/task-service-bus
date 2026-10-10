using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.ServiceBus;

namespace TaskManagementServiceBusApi.Helper
{
    public enum FailureKind { Transient, Permanent }

    // Optional markers you can throw from your own code
    public class PermanentException(string message, Exception? inner = null) : Exception(message, inner)
    {
    }
    public class TransientException(string message, Exception? inner = null) : Exception(message, inner)
    {
    }

    public class FailureClassifier
    {
        // Unknown errors: retry (bounded by max retries) or fail fast?
        private const FailureKind DefaultKind = FailureKind.Transient;

        public static FailureKind Classify(Exception ex)
        {
            foreach (var e in Flatten(ex))
            {
                var kind = ClassifySingle(e);
                if (kind.HasValue) return kind.Value;
            }
            return DefaultKind;
        }

        public static bool IsTransient(Exception ex) => Classify(ex) == FailureKind.Transient;

        private static FailureKind? ClassifySingle(Exception e) => e switch
        {
            // Explicit markers always win
            PermanentException => FailureKind.Permanent,
            TransientException => FailureKind.Transient,

            // Bad data: retrying will never help
            JsonException or FormatException or ArgumentException
                or InvalidCastException or NotSupportedException
                or UnauthorizedAccessException => FailureKind.Permanent,

            // HTTP
            HttpRequestException h when h.StatusCode is not null
                => ClassifyHttp((int)h.StatusCode.Value),
            HttpRequestException => FailureKind.Transient,      // DNS, connection reset, etc.

            // Azure SDK (Storage, Key Vault, etc.)
            RequestFailedException r when r.Status != 0
                => ClassifyHttp(r.Status),

            // Service Bus client itself
            ServiceBusException sb => sb.IsTransient ? FailureKind.Transient : FailureKind.Permanent,

            // Cosmos DB
            // CosmosException c => ClassifyHttp((int)c.StatusCode),

            // SQL Server
            // SqlException s => s.IsTransient || s.Number is 1205 or -2   // deadlock, timeout
            //     ? FailureKind.Transient : FailureKind.Permanent,

            // Network and timeouts
            TimeoutException or SocketException or IOException => FailureKind.Transient,
            TaskCanceledException => FailureKind.Transient,      // usually an HTTP timeout

            _ => null   // unknown, keep looking at inner exceptions
        };

        private static FailureKind ClassifyHttp(int status) => status switch
        {
            408 or 429 => FailureKind.Transient,                 // timeout, throttled
            >= 500 and <= 599 => FailureKind.Transient,          // server-side
            _ => FailureKind.Permanent                           // 400, 401, 403, 404, 409, 422...
        };

        // Yields ex, then its inner exceptions (unwrapping AggregateException)
        private static IEnumerable<Exception> Flatten(Exception? ex)
        {
            while (ex is not null)
            {
                yield return ex;

                if (ex is AggregateException agg)
                {
                    foreach (var inner in agg.InnerExceptions)
                        foreach (var nested in Flatten(inner))
                            yield return nested;
                    yield break;
                }
                ex = ex.InnerException;
            }
        }
    }
}