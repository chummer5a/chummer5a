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
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChummerHub.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger _logger;
        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor, ILogger<EmailSender> logger)
       {
            Options = optionsAccessor.Value;
            
            _logger = logger;
            var keys = new KeyVault(_logger);
            Options.SendGridKey = keys.GetSecret($"SendGridKey");
            Options.SendGridUser = keys.GetSecret($"SendGridUser");
            
        }

        public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

        public Task SendEmailAsync(string email, string subject, string message)
        {
            _logger.LogInformation("SendMailAsync\tTo: " + email
                    + Environment.NewLine + "\tSubject: " + subject
                    + Environment.NewLine + "\tMessage: " + message);
            return Execute(Options.SendGridKey, subject, message, email);
        }

        public Task Execute(string apiKey, string subject, string message, string email)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentNullException(nameof(apiKey), "EmailSender.cs: apiKey is null!");
            }
            var httpClientHandler = new HttpClientHandler()
            {
                Proxy = WebRequest.GetSystemWebProxy(),// new WebProxy("http://localhost:8888"),
                UseProxy = true,
                DefaultProxyCredentials = CredentialCache.DefaultCredentials,
                Credentials = CredentialCache.DefaultCredentials
            };
            httpClientHandler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;
            httpClientHandler.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            httpClientHandler.PreAuthenticate = true;
            httpClientHandler.SslProtocols = System.Security.Authentication.SslProtocols.Ssl3 | System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls;
            httpClientHandler.ServerCertificateCustomValidationCallback = (message2, cert, chain, errors) => true;
            var httpClient = new HttpClient(httpClientHandler);

            var client = new SendGridClient(httpClient, apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("archon.megalon@gmail.com", "Archon Megalon"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.TrackingSettings = new TrackingSettings
            {
                ClickTracking = new ClickTracking { Enable = false }
            };
            var task = client.SendEmailAsync(msg);
#if DEBUG
            try
            {
                task.Wait();
            }
            catch (Exception e)
            {
                _logger.LogError("Exception sending mail: " + Environment.NewLine + e);
                throw;
            }
#endif
            return task;
        }
    }
}
