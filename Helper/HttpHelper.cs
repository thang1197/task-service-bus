using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TaskManagementServiceBusApi.Configuration;

namespace TaskManagementServiceBusApi.Helper
{
    public class HttpHelper(IOptions<ApiManagementConfiguration> apiManagementConfiguration, IOptions<AzureAdSettings> azureAdSettings, ClientSecretCredential clientSecretCredential)
    {
        private readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(30),
            BaseAddress = new Uri(apiManagementConfiguration.Value.BaseUrl)
        };

        private readonly ClientSecretCredential _clientSecretCredential= clientSecretCredential;
        private readonly AzureAdSettings _azureAdSettings = azureAdSettings.Value;

        public static IActionResult GenerateResponse<T>(T data, int statusCode = StatusCodes.Status200OK)
        {
            try
            {
                var response = new
                {
                    status = "success",
                    code = statusCode,
                    data = data
                };
                var result = new ObjectResult(response)
                {
                    StatusCode = statusCode
                };

                result.ContentTypes.Add("application/json");
                return result;
            }
            catch (Exception ex)
            {
                var response = new
                {
                    status = "error",
                    code = StatusCodes.Status500InternalServerError,
                    error = ex.Message
                };
                var result = new ObjectResult(response)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return result;
            }
        }

        public static IActionResult GenerateErrorResponse(string errorMessage, int statusCode = StatusCodes.Status400BadRequest)
        {
            var response = new
            {
                status = "error",
                code = statusCode,
                error = errorMessage
            };
            var result = new ObjectResult(response)
            {
                StatusCode = statusCode
            };
            return result;
        }

        public HttpClient GetHttpClient()
        {
            return _httpClient;
        }

        public async Task<string> GetAdToken()
        {
            try
            {
                var token = await _clientSecretCredential.GetTokenAsync(new TokenRequestContext([_azureAdSettings.Scope]));
                return token.Token;
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }
}