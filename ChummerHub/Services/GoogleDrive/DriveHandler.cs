using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChummerHub.API;
using ChummerHub.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ChummerHub.Services.GoogleDrive
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DriveHandler'
    public class DriveHandler
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DriveHandler'

    {
#pragma warning disable CS0414 // The field 'DriveHandler.Credential' is assigned but its value is never used
        GoogleCredential Credential = null;
#pragma warning restore CS0414 // The field 'DriveHandler.Credential' is assigned but its value is never used
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DriveHandler.Scopes'
        public static string[] Scopes = {
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DriveHandler.Scopes'
            DriveService.Scope.DriveFile,
            DriveService.Scope.Drive,
            DriveService.Scope.DriveAppdata,
            DriveService.Scope.DriveMetadata

        };
#pragma warning disable CS0414 // The field 'DriveHandler.ApplicationName' is assigned but its value is never used
        static string ApplicationName = "SINners";
#pragma warning restore CS0414 // The field 'DriveHandler.ApplicationName' is assigned but its value is never used
        private readonly ILogger _logger;

        private static string _contentType = "application/octet-stream";
        private static string _folderId = string.Empty;
        readonly IConfiguration Configuration;



        //https://stackoverflow.com/questions/26098009/how-google-api-v-3-0-net-library-and-google-oauth2-handling-refresh-token/31843907#31843907



#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DriveHandler.GetUserCredential(IConfiguration)'
        public async Task<UserCredential> GetUserCredential(IConfiguration configuration)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DriveHandler.GetUserCredential(IConfiguration)'
        {
            try
            {
                string refreshToken = Configuration["Authentication:Google:RefreshToken"];
                UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                           new ClientSecrets
                           {
                               ClientId = Configuration["Authentication:Google:GoogleChummerSINersId"],
                               ClientSecret = Configuration["Authentication:Google:GoogleChummerSINersSecret"]
                           }, Scopes, "user", CancellationToken.None, new GoogleIDataStore("me", refreshToken, _logger));

                return credential;
            }
            catch (Exception e)
            {
                _logger.LogError("Could not get UserCredentials: " + e);
                return null;
            }
        }

        private UserCredential AuthorizeGoogleUser()
        {
            try
            {
                string refreshToken = Configuration["Authentication:Google:RefreshToken"];

                if (string.IsNullOrEmpty(refreshToken))
                    throw new ArgumentException("Configuration[\"Authentication:Google:RefreshToken\"] == null! ");

                var token = new TokenResponse
                {
                    AccessToken = "",
                    RefreshToken = refreshToken
                };

                var flow2 = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = Configuration["Authentication:Google:GoogleChummerSINersId"],
                        ClientSecret = Configuration["Authentication:Google:GoogleChummerSINersSecret"]
                    },
                    Scopes = Scopes,
                    DataStore = new GoogleIDataStore("me", refreshToken, _logger)
                });


                UserCredential credential = new UserCredential(flow2, "me", token);
                return credential;
            }
            catch (Exception e)
            {
                _logger.LogError("Could Authorize Google User: " + e);
                throw;
            }
        }

#pragma warning disable CS0414 // The field 'DriveHandler.flow' is assigned but its value is never used
        private static IAuthorizationCodeFlow flow = null;
#pragma warning restore CS0414 // The field 'DriveHandler.flow' is assigned but its value is never used


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DriveHandler.DriveHandler(ILogger<Startup>, IConfiguration)'
        public DriveHandler(ILogger<Startup> Logger, IConfiguration configuration)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DriveHandler.DriveHandler(ILogger<Startup>, IConfiguration)'
        {
            Configuration = configuration;
            _logger = Logger;
            string refreshToken = Configuration["Authentication:Google:RefreshToken"];

            if (string.IsNullOrEmpty(_folderId))
                _folderId = Configuration["Authentication:Google:ChummerFolderId"];
        }

        internal string StoreXmlInCloud(SINnerUploadAble uploadFile, IFormFile uploadedFile)
        {
            string url = "default";
            try
            {
                _logger.LogTrace("Storing " + uploadedFile.FileName + " to GDrive...");

                UserCredential creds = AuthorizeGoogleUser();
                if (creds == null)
                    throw new Exception("Invalid Google User");

                // Create Drive API service.
                BaseClientService.Initializer initializer = new BaseClientService.Initializer
                {
                    HttpClientInitializer = creds,
                    ApplicationName = "SINners",
                    GZipEnabled = true,
                };
#pragma warning disable CS0219 // The variable 'cancellationToken' is assigned but its value is never used
                CancellationToken cancellationToken = new CancellationToken();
#pragma warning restore CS0219 // The variable 'cancellationToken' is assigned but its value is never used


                // Create Drive API service.
                var service = new DriveService(initializer);

                if (string.IsNullOrEmpty(_folderId))
                {
                    File fileMetadata = new File
                    {
                        Name = "Chummer",
                        MimeType = "application/vnd.google-apps.folder"
                    };
                    var folderid = service.Files.Create(fileMetadata).Execute();
                    string msg = "ChummerFolderId: " + folderid.Id;
                    _logger.LogCritical(msg);
                    throw new HubException("HubException: " + msg);
                }
                uploadFile.DownloadUrl = string.IsNullOrEmpty(uploadFile.GoogleDriveFileId)
                    ? UploadFileToDrive(service, uploadedFile, _contentType, uploadFile)
                    : UpdateFileToDrive(service, uploadedFile, _contentType, uploadFile);

                // Define parameters of request.
                FilesResource.ListRequest listRequest = service.Files.List();
                listRequest.PageSize = 10;
                listRequest.Q = "'" + _folderId + "' in parents";
                listRequest.Fields = "nextPageToken, files(id, name, webContentLink)";

                // List files.
                IList<File> files = listRequest.Execute()
                    .Files;
                url = "Folder " + _folderId + ":" + Environment.NewLine;
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        url += $"{file.Name} ({file.Id}): {file.WebContentLink}" + Environment.NewLine;
                    }
                }
                else
                {
                    url += " No files found.";
                }



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
                        url += $"{file.Name} ({file.Id}): {file.WebContentLink}" + Environment.NewLine;
                    }
                }
                else
                {
                    url += "No files found.";
                }

                //_logger.LogError("ParentUrl: " + url);
            }
            catch (Exception e)
            {

                _logger.LogError("Could not store file on GDrive: " + e);
                throw;

            }

            return uploadFile.DownloadUrl;
        }

        private string UploadFileToDrive(DriveService service, IFormFile uploadFile, string conentType, SINnerUploadAble fileMetaData)
        {
            FilesResource.CreateMediaUpload request;
            try
            {
                var fileMetadata = new File
                {
                    Properties = new Dictionary<string, string>(),
                    Parents = new List<string> {_folderId}
                };
                //foreach(var tag in fileMetaData.SINnerMetaData.Tags)
                //{
                //    fileMetadata.Properties.Add(tag.Display, tag.TagValue);
                //}
                _logger.LogError("Uploading " + fileMetaData.FileName + " as " + fileMetadata.Name);// + " to folder: " + _folderId);
                //fileMetadata.MimeType = _contentType;
                fileMetadata.OriginalFilename = fileMetaData.FileName;

                request = service.Files.Create(fileMetadata, uploadFile.OpenReadStream(), conentType);
                request.Fields = "id, webContentLink";
                var uploadprogress = request.Upload();

                while ((uploadprogress.Status != UploadStatus.Completed)
                    && (uploadprogress.Status != UploadStatus.Failed))
                {
                    if (uploadprogress.Exception != null)
                        throw uploadprogress.Exception;
                    uploadprogress = request.Resume();
                }
                if (uploadprogress.Status == UploadStatus.Failed)
                {
                    _logger.LogError("Chummer \"" + fileMetaData.Id.ToString() + "\" upload failed: " + uploadprogress.Exception);
                    throw uploadprogress.Exception;
                }

            }
            catch (Exception e)
            {
                Exception innere = e;
                while (innere.InnerException != null)
                {
                    innere = innere.InnerException;
                }
                _logger.LogError(innere.ToString());
                throw;
            }

            _logger.LogError("Chummer \"" + fileMetaData.Id.ToString() + "\" uploaded: " + request.ResponseBody?.WebContentLink);

            fileMetaData.GoogleDriveFileId = request.ResponseBody?.Id;
            fileMetaData.DownloadUrl = request.ResponseBody?.WebContentLink;

            UploadFilePermission(service, fileMetaData);

            return request.ResponseBody?.WebContentLink;


        }


        private string UpdateFileToDrive(DriveService service, IFormFile uploadFile, string conentType, SINnerUploadAble fileMetaData)
        {
            FilesResource.UpdateMediaUpload request;
            try
            {
                var googlefileMetadata = new File
                {
                    Name = fileMetaData.FileName
                    //Parents = new List<string> { _folderId }
                };
                //fileMetadata.Properties = new Dictionary<string, string>();
                //foreach(var tag in fileMetaData.SINnerMetaData.Tags)
                //{
                //    fileMetadata.Properties.Add(tag.Display, tag.TagValue);
                //}
                _logger.LogError("Updating " + uploadFile.FileName + " as " + googlefileMetadata.Name);// + " to folder: " + _folderId);
                //fileMetadata.MimeType = _contentType;
                googlefileMetadata.OriginalFilename = uploadFile.FileName;

                request = service.Files.Update(googlefileMetadata, fileMetaData.GoogleDriveFileId, uploadFile.OpenReadStream(), conentType);
                request.Fields = "id, webContentLink";
                var uploadprogress = request.Upload();

                while ((uploadprogress.Status != UploadStatus.Completed)
                    && (uploadprogress.Status != UploadStatus.Failed))
                {
                    if (uploadprogress.Exception != null)
                        throw uploadprogress.Exception;
                    uploadprogress = request.Resume();
                }
                if (uploadprogress.Status == UploadStatus.Failed)
                {
                    _logger.LogError("Chummer \"" + fileMetaData.Id.ToString() + "\" upload failed: " + uploadprogress.Exception);
                    throw uploadprogress.Exception;
                }

            }
            catch (Exception e)
            {
                Exception innere = e;
                while (innere.InnerException != null)
                {
                    innere = innere.InnerException;
                }
                _logger.LogError(innere.ToString());
                throw;
            }

            _logger.LogError("Chummer \"" + fileMetaData.Id.ToString() + "\" updated: " + request.ResponseBody?.WebContentLink);

            fileMetaData.GoogleDriveFileId = request.ResponseBody?.Id;
            fileMetaData.DownloadUrl = request.ResponseBody?.WebContentLink;

            //UploadFilePermission(service, chummerFile);

            return request.ResponseBody?.WebContentLink;


        }


        private void UploadFilePermission(DriveService service, SINnerUploadAble fileMetaData)
        {
            try
            {
                Permission permission = new Permission
                {
                    Type = "anyone",
                    Role = "reader",
                    AllowFileDiscovery = true
                };

                PermissionsResource.CreateRequest request = service.Permissions.Create(permission, fileMetaData.GoogleDriveFileId);
                request.Fields = "id";
                request.Execute();
            }
            catch (Exception e)
            {
                string msg = "Error while setting permissions for " + fileMetaData.Id + ": " + Environment.NewLine;
                msg += e.ToString();
                _logger.LogError(msg);
                throw;
            }
        }
    }
}
