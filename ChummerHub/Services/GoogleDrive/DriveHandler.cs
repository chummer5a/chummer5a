using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using ChummerHub.API;
using ChummerHub.Models.V1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static Google.Apis.Drive.v3.DriveService;

namespace ChummerHub.Services.GoogleDrive
{
    public class DriveHandler 

    {
        GoogleCredential Credential = null;
        public static string[] Scopes = {
            DriveService.Scope.DriveFile,
            DriveService.Scope.Drive,
            DriveService.Scope.DriveAppdata,
            DriveService.Scope.DriveMetadata

        };
        static string ApplicationName = "SINners";
        private readonly ILogger _logger;

        private static string _contentType = "application/octet-stream";
        private static string _folderId = "1DfCHQnJbV077VDKNACbLRWP7OvG-NHfM";
        IConfiguration Configuration;



        https://stackoverflow.com/questions/26098009/how-google-api-v-3-0-net-library-and-google-oauth2-handling-refresh-token/31843907#31843907




        private static readonly IAuthorizationCodeFlow flow =
            new ForceOfflineGoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = "779360551859-i817g72s0ork3bffvnhtvpl0q2gi8sub.apps.googleusercontent.com",
                ClientSecret = "Q2yMsXBtdd-zxp6vPXcjkGFz"
            },
            Scopes = DriveHandler.Scopes,
            DataStore = new GoogleIDataStore(),
        });

        public DriveHandler(ILogger<Startup> Logger, IConfiguration configuration)
        {
            Configuration = configuration;
            _logger = Logger;

            //GoogleCredential tempcredential;
            //var json = EmbeddedResource.GetResource("ChummerHub.Services.GoogleDrive.SINners.json");
            //tempcredential = GoogleCredential.FromJson(json);

            //if (tempcredential.IsCreateScopedRequired)
            //    Credential = tempcredential.CreateScoped(Scopes);

            
            if (String.IsNullOrEmpty(_folderId))
                _folderId = Configuration["Authentication:Google:ChummerFolderId"];


        }

        internal string StoreXmlInCloud(SINner chummerFile, IFormFile uploadedFile)
        {
            string url = "default";
            try
            {
                _logger.LogTrace("Storing " + uploadedFile.FileName + " to GDrive...");

                // Create Drive API service.
                BaseClientService.Initializer initializer = new BaseClientService.Initializer()
                {
                    HttpClientInitializer = (IConfigurableHttpClientInitializer)Credential,
                    ApplicationName = "SINners",
                    GZipEnabled = true,
                };
                CancellationToken cancellationToken = new CancellationToken();

                

                var result = new AuthorizationCodeInstalledApp(flow).AuthorizeAsync("user", cancellationToken);

                if (result.Result.Credential != null)
                {
                    // Create Drive API service.
                    var service = new DriveService(initializer);
                }

                 

                if (String.IsNullOrEmpty(_folderId))
                { 
                    Google.Apis.Drive.v3.Data.File fileMetadata = new Google.Apis.Drive.v3.Data.File();
                    fileMetadata.Name = "Chummer";
                    fileMetadata.MimeType = "application/vnd.google-apps.folder";
                    var folderid = service.Files.Create(fileMetadata).Execute();
                    string msg = "ChummerFolderId: " + folderid.Id;
                    _logger.LogCritical(msg);
                    throw new HubException("HubException: " + msg);
                }

                UploadFileToDrive(service, uploadedFile, _contentType, chummerFile);
                
                // Define parameters of request.
                FilesResource.ListRequest listRequest = service.Files.List();
                listRequest.PageSize = 10;
                listRequest.Q = "'"+ _folderId + "' in parents";
                listRequest.Fields = "nextPageToken, files(id, name, webContentLink)";
                
                // List files.
                IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
                    .Files;
                url = "Folder " + _folderId + ":" + Environment.NewLine;
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        url += String.Format("{0} ({1}): {2}", file.Name, file.Id, file.WebContentLink) + Environment.NewLine;
                    }
                }
                else
                {
                    url += " No files found.";
                }

                _logger.LogError("FolderUrl: " + url);

                // Define parameters of request.
                listRequest = service.Files.List();
                listRequest.PageSize = 10;
                listRequest.Fields = "nextPageToken, files(id, name, webContentLink)";

                // List files.
                files = listRequest.Execute()
                    .Files;
                url = "ParentFolder: " + Environment.NewLine;
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        url += String.Format("{0} ({1}): {2}", file.Name, file.Id, file.WebContentLink) + Environment.NewLine;
                    }
                }
                else
                {
                    url += "No files found.";
                }

                _logger.LogError("ParentUrl: " + url);
            }
            catch(Exception e)
            {
                _logger.LogError("Could not store file on GDrive: " + e.ToString());
                throw;

            }

            return url;
        }

        private string UploadFileToDrive(DriveService service, IFormFile uploadFile, string conentType, SINner chummerFile)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            { Permissions = new List<Permission>()
                { new Permission()
                {
                    Kind = "drive#permission",
                    Role = "reader",
                    Type = "anyone"
                }}
            };
            fileMetadata.Properties = new Dictionary<string, string>();
            //foreach(var tag in chummerFile.SINnerMetaData.Tags)
            //{
            //    fileMetadata.Properties.Add(tag.Display, tag.TagValue);
            //}
            fileMetadata.Name = chummerFile.SINnerId.ToString();
            fileMetadata.Parents = new List<string> { _folderId };
            _logger.LogError("Uploading " + uploadFile.FileName + " as " + fileMetadata.Name);// + " to folder: " + _folderId);
            fileMetadata.MimeType = _contentType;
            fileMetadata.Shared = true;
            fileMetadata.OriginalFilename = uploadFile.FileName;
            
            FilesResource.CreateMediaUpload request;

            try
            {
                request = service.Files.Create(fileMetadata, uploadFile.OpenReadStream(), conentType);
                request.Fields = "id, webContentLink";
                var uploadprogress = request.Upload();
                
                while ((uploadprogress.Status != Google.Apis.Upload.UploadStatus.Completed)
                    && (uploadprogress.Status != Google.Apis.Upload.UploadStatus.Failed))
                {   
                    if (uploadprogress.Exception != null)
                        throw uploadprogress.Exception;
                    uploadprogress = request.Resume();
                }
                
            }
            catch(Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
            
            _logger.LogError("Chummer \"" + chummerFile.SINnerId.ToString() + "\" uploaded: " + request.ResponseBody?.ToString());

            chummerFile.GoogleDriveFileId = request.ResponseBody?.Id;
            chummerFile.DownloadUrl = request.ResponseBody?.WebContentLink;

            return chummerFile.GoogleDriveFileId;


        }
    }
}
