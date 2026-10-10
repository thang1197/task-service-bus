using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SharedCommon.CoreApi;
using SharedCommon.CoreHttp;
using TaskManagementServiceBusApi.Configuration;
using TaskManagementServiceBusApi.FunctionAppCore.Storage.DTO;

namespace TaskManagementServiceBusApi.FunctionAppCore.Storage.Service
{
    public class FunctionAppCoreStorageService(FunctionAppCoreHttpClient functionAppCoreHttpClient) : IFunctionAppCoreStorageService
    {
        private readonly FunctionAppCoreHttpClient _functionAppCoreHttpClient = functionAppCoreHttpClient;
        public async Task<StreamResponse> DownloadFileAsync(string fileName, string containerName, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("DownloadFileFunction?fileName=" + fileName + "&containerName=" + containerName, UriKind.Relative)
                };
                var streamResponse = await _functionAppCoreHttpClient.SendStreamAsync(request, cancellationToken);
                return streamResponse;
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }

        public async Task<Response<FileResponse>> UploadFileAsync(FileUploadRequest fileUploadRequest, CancellationToken cancellationToken = default)
        {
            var formData = new MultipartFormDataContent
                {
                    { new StreamContent(fileUploadRequest.Content), "file", fileUploadRequest.FileName },
                    { new StringContent(fileUploadRequest.ContainerName), "containerName" },
                    { new StringContent(fileUploadRequest.TaskId.ToString(CultureInfo.InvariantCulture)), "taskId" },
                    { new StringContent(fileUploadRequest.ProjectId.ToString(CultureInfo.InvariantCulture)), "projectId" }
                };
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("UploadFileFunction", UriKind.Relative),
                Content = formData
            };
            var response = await _functionAppCoreHttpClient.SendAsync<Response<FileResponse>>(request, cancellationToken);
            return response;
        }
    }
}