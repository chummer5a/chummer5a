//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Auth.OAuth2.Flows;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Logging;
//using Google.Apis.Auth.OAuth2.Requests;
//using Google.Apis.Json;
//using Google.Apis.Util.Store;

namespace ChummerHub.Services.GoogleDrive
{
    /// <summary>
    /// Handles internal token storage, bypassing filesystem
    /// </summary>
    public class GoogleIDataStore : IDataStore
    {
        private static ILogger _logger;
        public static Dictionary<string, TokenResponse> _store;

        public GoogleIDataStore()
        {
            _store = new Dictionary<string, TokenResponse>();
        }

        public GoogleIDataStore(string key, string refreshToken, ILogger Logger)
        {
            try
            {
                _logger = Logger;
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException(nameof(key));
                if (string.IsNullOrEmpty(refreshToken))
                    throw new ArgumentNullException(nameof(refreshToken));

                _store = new Dictionary<string, TokenResponse>();

                // add new entry
                StoreAsync(key,
                    new TokenResponse { RefreshToken = refreshToken, TokenType = "Bearer" }).Wait();
            }
            catch (Exception e)
            {
                _logger.LogError("Could create GoogleIDataStore: " + e);
            }

        }

        /// <summary>
        /// Remove all items
        /// </summary>
        /// <returns></returns>
        public Task ClearAsync()
        {
            return Task.Run(() =>
            {
                _store.Clear();
            });
        }

        /// <summary>
        /// Obtain object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string key)
        {
            // check type
            AssertCorrectType<T>();
            if (_store.ContainsKey(key))
                return await Task.Run(() => (T)(object)_store[key]);
            // key not found
            return default;
        }

        /// <summary>
        /// Add/update value for key/value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task StoreAsync<T>(string key, T value)
        {
            return Task.Run(() =>
            {
                if (_store.ContainsKey(key))
                    _store[key] = (TokenResponse)(object)value;
                else
                    _store.Add(key, (TokenResponse)(object)value);
            });
        }

        /// <summary>
        /// Validate we can store this type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static void AssertCorrectType<T>()
        {
            if (typeof(T) != typeof(TokenResponse))
                throw new NotImplementedException(typeof(T).ToString());
        }

        /// <summary>
        /// Remove single entry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task DeleteAsync<T>(string key)
        {
            return Task.Run(() =>
            {
                // check type
                AssertCorrectType<T>();
                if (_store.ContainsKey(key))
                    _store.Remove(key);
            });
        }
    }
}
