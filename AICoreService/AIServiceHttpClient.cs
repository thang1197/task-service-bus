using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedCommon.Cache.Service;
using SharedCommon.Configuration.Entity;
using SharedCommon.CoreHttp;

namespace TaskManagementServiceBusApi.AICoreService
{
    public class AICoreServiceHttpClient(
        ICredentialCache credentialCache,
        HttpClient httpClient,
        AzureAdConfiguration azureAdConfiguration
    ) : CoreHttpClient(credentialCache, httpClient, azureAdConfiguration)
    {
        
    }
}