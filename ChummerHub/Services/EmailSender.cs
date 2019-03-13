using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
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
            httpClientHandler.ServerCertificateCustomValidationCallback = (message2, cert, chain, errors) => { return true; };
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
            catch(Exception e)
            {
                _logger.LogError("Exception sending mail: " + Environment.NewLine + e.ToString());
                throw;
            }
#endif
            return task;
        }
    }
}
