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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GoogleIDataStore._store'
        public static Dictionary<string, TokenResponse> _store;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GoogleIDataStore._store'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GoogleIDataStore.GoogleIDataStore()'
        public GoogleIDataStore()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GoogleIDataStore.GoogleIDataStore()'
        {
            _store = new Dictionary<string, TokenResponse>();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GoogleIDataStore.GoogleIDataStore(string, string, ILogger)'
        public GoogleIDataStore(string key, string refreshToken, ILogger Logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GoogleIDataStore.GoogleIDataStore(string, string, ILogger)'
        {
            try
            {
                _logger = Logger;
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                if (string.IsNullOrEmpty(refreshToken))
                    throw new ArgumentNullException("refreshToken");

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
        public async Task ClearAsync()
        {
            await Task.Run(() =>
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
                return await Task.Run(() => { return (T)(object)_store[key]; });
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
        private void AssertCorrectType<T>()
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
        public async Task DeleteAsync<T>(string key)
        {
            await Task.Run(() =>
            {
                // check type
                AssertCorrectType<T>();
                if (_store.ContainsKey(key))
                    _store.Remove(key);
            });
        }
    }
}
