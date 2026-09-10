using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using TaskManagementServiceBusApi.Configuration;

namespace TaskManagementServiceBusApi.Credential.Service
{
    public class CredentialManger(ILogger<TokenCredential> logger, AzureAdSettings azureAdSettings) : ICredentialManager
    {
        private readonly ILogger<TokenCredential> _logger = logger;
        private readonly TokenCredential _clientCredential = new ClientSecretCredential(
            azureAdSettings.TenantId,
            azureAdSettings.ClientId,
            azureAdSettings.ClientSecret
        );
        // private readonly TokenCredential _clientCredential = clientCredential;
        private string _credential = string.Empty;
        private DateTime? _expirationTime;

        public async Task<string> GetCredential()
        {
            if (string.IsNullOrEmpty(_credential) || (_expirationTime.HasValue && DateTime.UtcNow >= _expirationTime.Value.Subtract(TimeSpan.FromMinutes(5))))
            {
                // Fetch the credential from Key Vault or any other secure source
                // For demonstration, let's assume we fetch it from Key Vault
                _logger.LogInformation("Fetching new credential from Key Vault...");
                var credential = await _clientCredential.GetTokenAsync(new TokenRequestContext(["api://18b03cb2-0463-44dc-9ce6-50e641df38c5/.default"]), CancellationToken.None);
                _logger.LogInformation("New credential fetched successfully.");
                _logger.LogInformation("Credential expires at: {ExpirationTime}", credential.ExpiresOn);
                _credential = credential.Token;
                _expirationTime = DateTime.UtcNow.AddSeconds(credential.ExpiresOn.Subtract(DateTime.UtcNow).TotalSeconds);
            }
            return _credential;
        }

        public void SetCredential(string credential)
        {
            _credential = credential;
        }

        public void ClearCredential()
        {
            _credential = string.Empty;
        }
    }
}