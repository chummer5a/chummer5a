// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using NLog;
using System;
using System.Security.Cryptography;
using System.Text;

namespace IdentityModel.OidcClient
{
    internal class CryptoHelper
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly OidcClientOptions _options;

        public CryptoHelper(OidcClientOptions options)
        {
            _options = options;
            //_logger = options.LoggerFactory.CreateLogger<CryptoHelper>();
        }

        public HashAlgorithm GetMatchingHashAlgorithm(string signatureAlgorithm)
        {
            _logger.Trace("GetMatchingHashAlgorithm");
            _logger.Debug("Determining matching hash algorithm for {signatureAlgorithm}", signatureAlgorithm);

            var signingAlgorithmBits = int.Parse(signatureAlgorithm.Substring(signatureAlgorithm.Length - 3));
            
            switch (signingAlgorithmBits)
            {
                case 256:
                    _logger.Debug("SHA256");
                    return SHA256.Create();
                case 384:
                    _logger.Debug("SHA384");
                    return SHA384.Create();
                case 512:
                    _logger.Debug("SHA512");
                    return SHA512.Create();
                default:
                    return null;
            }
        }

        public bool ValidateHash(string data, string hashedData, string signatureAlgorithm)
        {
            _logger.Trace("ValidateHash");

            var hashAlgorithm = GetMatchingHashAlgorithm(signatureAlgorithm);
            if (hashAlgorithm == null)
            {
                _logger.Error("No appropriate hashing algorithm found.");
            }

            using (hashAlgorithm)
            {
                var hash = hashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(data));
                var size = (hashAlgorithm.HashSize / 8) / 2;

                byte[] leftPart = new byte[hashAlgorithm.HashSize / size];
                Array.Copy(hash, leftPart, hashAlgorithm.HashSize / size);

                var leftPartB64 = Base64Url.Encode(leftPart);
                var match = leftPartB64.Equals(hashedData);

                if (!match)
                {
                    _logger.Error($"data ({leftPartB64}) does not match hash from token ({hashedData})");
                }

                return match;
            }
        }

        public string CreateState(int length)
        {
            _logger.Trace("CreateState");

            return CryptoRandom.CreateUniqueId(length);
        }

        public Pkce CreatePkceData()
        {
            _logger.Trace("CreatePkceData");

            var pkce = new Pkce
            {
                CodeVerifier = CryptoRandom.CreateUniqueId()
            };

            using (var sha256 = SHA256.Create())
            {
                var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(pkce.CodeVerifier));
                pkce.CodeChallenge = Base64Url.Encode(challengeBytes);
            }

            return pkce;
        }

        internal class Pkce
        {
            public string CodeVerifier { get; set; }
            public string CodeChallenge { get; set; }
        }
    }
}
