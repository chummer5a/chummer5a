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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailSender'
    public class EmailSender : IEmailSender
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailSender'
    {
        private readonly ILogger _logger;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailSender.EmailSender(IOptions<AuthMessageSenderOptions>, ILogger<EmailSender>)'
        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor, ILogger<EmailSender> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailSender.EmailSender(IOptions<AuthMessageSenderOptions>, ILogger<EmailSender>)'
        {
            Options = optionsAccessor.Value;
            _logger = logger;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailSender.Options'
        public AuthMessageSenderOptions Options { get; } //set only via Secret Manager
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailSender.Options'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailSender.SendEmailAsync(string, string, string)'
        public Task SendEmailAsync(string email, string subject, string message)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailSender.SendEmailAsync(string, string, string)'
        {
            _logger.LogInformation("SendMailAsync\tTo: " + email
                    + Environment.NewLine + "\tSubject: " + subject
                    + Environment.NewLine + "\tMessage: " + message);
            return Execute(Options.SendGridKey, subject, message, email);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailSender.Execute(string, string, string, string)'
        public Task Execute(string apiKey, string subject, string message, string email)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailSender.Execute(string, string, string, string)'
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
#pragma warning disable CS0618 // 'SslProtocols.Ssl3' is obsolete: 'This value has been deprecated.  It is no longer supported. http://go.microsoft.com/fwlink/?linkid=14202'
            httpClientHandler.SslProtocols = System.Security.Authentication.SslProtocols.Ssl3 | System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls;
#pragma warning restore CS0618 // 'SslProtocols.Ssl3' is obsolete: 'This value has been deprecated.  It is no longer supported. http://go.microsoft.com/fwlink/?linkid=14202'
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
