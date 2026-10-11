using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCommon.CoreApi;
using SharedCommon.CoreHttp;
using TaskManagementServiceBusApi.Configuration;
using TaskManagementServiceBusApi.FunctionAppCore.Storage.DTO;

namespace TaskManagementServiceBusApi.FunctionAppCore.Storage.Service
{
    public interface IFunctionAppCoreStorageService
    {
        public Task<StreamResponse> DownloadFileAsync(string fileName, string containerName, CancellationToken cancellationToken = default);

        public Task<Response<FileResponse>> UploadFileAsync(FileUploadRequest fileUploadRequest, CancellationToken cancellationToken = default);
    }
}