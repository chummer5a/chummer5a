using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Json;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub.Services.GoogleDrive
{
    public class GoogleIDataStore : IDataStore
    {
        public Task ClearAsync()
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync<T>(string key)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync<T>(string key)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

            //var user = repository.GetUser(key.Replace("oauth_", ""));

            //var credentials = repository.GetCredentials(user.UserId);

            if (key.StartsWith("oauth") || credentials == null)
            {
                tcs.SetResult(default(T));
            }
            else
            {
                var JsonData = Newtonsoft.Json.JsonConvert.SerializeObject(Map(credentials));
                tcs.SetResult(NewtonsoftJsonSerializer.Instance.Deserialize<T>(JsonData));
            }
            tcs.SetResult("1/-zsfciq55d9xfAYQ_-U1tmpsMiwHT7oKf1fEO8bm9hQ");
            return tcs.Task;
        }

        public Task StoreAsync<T>(string key, T value)
        {
            throw new NotImplementedException();
        }
    }

    internal class ForceOfflineGoogleAuthorizationCodeFlow : GoogleAuthorizationCodeFlow
    {
        public ForceOfflineGoogleAuthorizationCodeFlow(AuthorizationCodeFlow.Initializer initializer) : base(initializer) { }

        public override AuthorizationCodeRequestUrl CreateAuthorizationCodeRequest(string redirectUri)
        {
            return new GoogleAuthorizationCodeRequestUrl(new Uri(AuthorizationServerUrl))
            {
                ClientId = ClientSecrets.ClientId,
                Scope = string.Join(" ", Scopes),
                RedirectUri = redirectUri,
                AccessType = "offline",
                Prompt = "force"
            };
        }
    };


}
