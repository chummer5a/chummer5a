using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub.Services
{
    public class KeyVault
    {
        private readonly ILogger _logger;
        private static string keyVaultUrl = "https://sinnersvault.vault.azure.net/";
        private static SecretClient client = null;

        public KeyVault(ILogger logger)
        {
            _logger = logger;
        }

        public string GetSecret(string secretName)
        {
            try
            {
                if (client == null)
                    client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
                KeyVaultSecret keySecret = client.GetSecret(secretName);
                return keySecret.Value;
                
            }
            catch (Exception e)
            {
                _logger?.LogError(e, e.Message);
                throw;
            }
        }
    }
}
