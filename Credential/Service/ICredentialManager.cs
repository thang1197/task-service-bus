using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementServiceBusApi.Credential.Service
{
    public interface ICredentialManager
    {
        public Task<string> GetCredential();
        public void SetCredential(string credential);
        public void ClearCredential();
    }
}