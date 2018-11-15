using System;
using System.Collections.Generic;
using System.Configuration;
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
using Google.Apis.Auth.OAuth2.Responses;
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
        private static string _folderId = "";
        IConfiguration Configuration;



        //https://stackoverflow.com/questions/26098009/how-google-api-v-3-0-net-library-and-google-oauth2-handling-refresh-token/31843907#31843907



        public async Task<UserCredential> GetUserCredential(IConfiguration configuration)
        {
            try
            {
                UserCredential credential;
                string refreshToken = Configuration["Authentication:Google:RefreshToken"];
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                           new ClientSecrets
                           {
                               ClientId = Configuration["Authentication:Google:GoogleChummerSINersId"],
                               ClientSecret = Configuration["Authentication:Google:GoogleChummerSINersSecret"]
                           }, Scopes, "user", CancellationToken.None, new GoogleIDataStore("me", refreshToken, _logger));

                return credential;
            }
            catch (Exception e)
            {
                _logger.LogError("Could not get UserCredentials: " + e.ToString());
                return null;
            }
        }

        private UserCredential AuthorizeGoogleUser()
        {
            try
            {
                string refreshToken = Configuration["Authentication:Google:RefreshToken"];

                if (String.IsNullOrEmpty(refreshToken))
                    throw new ArgumentException("Configuration[\"Authentication:Google:RefreshToken\"] == null! ");

                var token = new TokenResponse
                {
                    AccessToken = "",
                    RefreshToken = refreshToken
                };

                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = Configuration["Authentication:Google:GoogleChummerSINersId"],
                        ClientSecret = Configuration["Authentication:Google:GoogleChummerSINersSecret"]
                    },
                    Scopes = Scopes,
                    DataStore = new GoogleIDataStore("me", refreshToken, _logger)
                });

                
                UserCredential credential = new UserCredential(flow, "me", token);
                return credential;
            }
            catch (Exception e)
            {
                _logger.LogError("Could Authorize Google User: " + e.ToString());
                throw;
            }
            return null;
        }

        private static IAuthorizationCodeFlow flow = null;
       

        public DriveHandler(ILogger<Startup> Logger, IConfiguration configuration)
        {
            Configuration = configuration;
            _logger = Logger;
            string refreshToken = Configuration["Authentication:Google:RefreshToken"];

            if (String.IsNullOrEmpty(_folderId))
                _folderId = Configuration["Authentication:Google:ChummerFolderId"];
        }

        internal string StoreXmlInCloud(SINner chummerFile, IFormFile uploadedFile)
        {
            string url = "default";
            try
            {
                _logger.LogTrace("Storing " + uploadedFile.FileName + " to GDrive...");

                UserCredential creds = AuthorizeGoogleUser();
                if (creds == null)
                    throw new Exception("Invalid Google User");

                // Create Drive API service.
                BaseClientService.Initializer initializer = new BaseClientService.Initializer()
                {
                    HttpClientInitializer = (IConfigurableHttpClientInitializer)creds,
                    ApplicationName = "SINners",
                    GZipEnabled = true,
                };
                CancellationToken cancellationToken = new CancellationToken();


                // Create Drive API service.
                var service = new DriveService(initializer);


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

            return chummerFile.DownloadUrl;
        }

        private string UploadFileToDrive(DriveService service, IFormFile uploadFile, string conentType, SINner chummerFile)
        {
            FilesResource.CreateMediaUpload request;
            try
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File();
                fileMetadata.Properties = new Dictionary<string, string>();
                foreach(var tag in chummerFile.SINnerMetaData.Tags)
                {
                    fileMetadata.Properties.Add(tag.Display, tag.TagValue);
                }
                fileMetadata.Name = chummerFile.SINnerId.ToString() + ".chum5z";
                fileMetadata.Parents = new List<string> { _folderId };
                _logger.LogError("Uploading " + uploadFile.FileName + " as " + fileMetadata.Name);// + " to folder: " + _folderId);
                //fileMetadata.MimeType = _contentType;
                fileMetadata.OriginalFilename = uploadFile.FileName;
            
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
                if (uploadprogress.Status == Google.Apis.Upload.UploadStatus.Failed)
                {
                    _logger.LogError("Chummer \"" + chummerFile.SINnerId.ToString() + "\" upload failed: " + uploadprogress.Exception?.ToString());
                    throw uploadprogress.Exception;
                }

            }
            catch(Exception e)
            {
                Exception innere = e;
                while(innere.InnerException != null)
                {
                    innere = innere.InnerException;
                }
                _logger.LogError(innere.ToString());
                throw;
            }
            
            _logger.LogError("Chummer \"" + chummerFile.SINnerId.ToString() + "\" uploaded: " + request.ResponseBody?.ToString());

            chummerFile.GoogleDriveFileId = request.ResponseBody?.Id;
            chummerFile.DownloadUrl = request.ResponseBody?.WebContentLink;

            UploadFilePermission(service, chummerFile);

            return chummerFile.GoogleDriveFileId;


        }

        private void UploadFilePermission(DriveService service, SINner chummerFile)
        {
            try
            {
                Google.Apis.Drive.v3.Data.Permission permission = new Google.Apis.Drive.v3.Data.Permission();
                permission.Type = "anyone";
                permission.Role = "reader";
                permission.AllowFileDiscovery = true;

                PermissionsResource.CreateRequest request = service.Permissions.Create(permission, chummerFile.GoogleDriveFileId);
                request.Fields = "id";
                request.Execute();
            }
            catch(Exception e)
            {
                string msg = "Error while setting permissions for " + chummerFile.SINnerId + ": " + Environment.NewLine;
                msg += e.ToString();
                _logger.LogError(msg);
                throw;
            }
        }
    }
}
