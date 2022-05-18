/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Extensions.AspNetCore.Configuration.Secrets; // v1.2.1
using System.Net.Http;
using System.Security.Authentication;
using System.Net;
using Azure.Core.Pipeline;

namespace ChummerHub.Services
{
    public class KeyVault
    {
        private readonly ILogger _logger;
        private const string keyVaultUrl = "https://sinnersvault.vault.azure.net/";
        private static SecretClient client;

        public KeyVault(ILogger logger)
        {
            _logger = logger;
        }

        public string GetSecret(string secretName)
        {
            try
            {
                if (client == null)
                {
                    //client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
                    var httpClient = new HttpClient(new HttpClientHandler
                    {
                        DefaultProxyCredentials = CredentialCache.DefaultCredentials,
                        SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls
                    });
                    var secretClientOptions = new SecretClientOptions
                    {
                        Transport = new HttpClientTransport(httpClient)
                    };
                    client = new SecretClient(
                                new Uri(keyVaultUrl),
                                new DefaultAzureCredential(/*defaultAzureCredentialOptions*/),
                                secretClientOptions);
                }
                KeyVaultSecret keySecret = client.GetSecret(secretName);
                return keySecret.Value;

            }
#if DEBUG
            catch (AuthenticationFailedException afe)
            {
                if (!afe.Message.Contains("(Azure Key Vault) is configured for use by Azure Active Directory users only."))
                {
                    throw;
                }
                return null;
            }
#endif
            catch(Azure.RequestFailedException e)
            {
                _logger?.LogWarning(e, e.Message);
                return null;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, e.Message);
                throw;
            }
        }
    }
}
