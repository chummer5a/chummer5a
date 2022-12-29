using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.OidcClient.Results;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NLog;
using JsonWebKey = IdentityModel.Jwk.JsonWebKey;

namespace IdentityModel.OidcClient
{
    /// <inheritdoc />
    public class JwtHandlerIdentityTokenValidator : IIdentityTokenValidator
    {

        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        /// <inheritdoc />
        public Task<IdentityTokenValidationResult> ValidateAsync(string identityToken, OidcClientOptions options, CancellationToken cancellationToken = default)
        {
            //var logger = options.LoggerFactory.CreateLogger<JwtHandlerIdentityTokenValidator>();
         
            Log.Trace("Validate");
            
            // setup general validation parameters
            TokenValidationParameters parameters = new TokenValidationParameters
            {
                ValidIssuer = options.ProviderInformation.IssuerName,
                ValidAudience = options.ClientId,
                ValidateIssuer = options.Policy.ValidateTokenIssuerName,
                NameClaimType = JwtClaimTypes.Name,
                RoleClaimType = JwtClaimTypes.Role,

                ClockSkew = options.ClockSkew
            };

            // read the token signing algorithm
            JsonWebTokenHandler handler = new JsonWebTokenHandler();
            JsonWebToken jwt;

            try
            {
                jwt = handler.ReadJsonWebToken(identityToken);
            }
            catch (Exception ex)
            {
                return Task.FromResult(new IdentityTokenValidationResult
                {
                    Error = $"Error validating identity token: {ex}"
                });
            }

            string algorithm = jwt.Alg;
            
            // if token is unsigned, and this is allowed, skip signature validation
            if (string.Equals(algorithm, "none", StringComparison.OrdinalIgnoreCase))
            {
                if (options.Policy.RequireIdentityTokenSignature)
                {
                    return Task.FromResult(new IdentityTokenValidationResult
                    {
                        Error = "Identity token is not signed. Signatures are required by policy"
                    });
                }

                Log.Info("Identity token is not signed. This is allowed by configuration.");
                parameters.RequireSignedTokens = false;
            }
            else
            {
                // check if signature algorithm is allowed by policy
                if (!options.Policy.ValidSignatureAlgorithms.Contains(algorithm))
                {
                    return Task.FromResult(new IdentityTokenValidationResult
                    {
                        Error = $"Identity token uses invalid algorithm: {algorithm}"
                    });
                }
            }

            TokenValidationResult result = ValidateSignature(identityToken, handler, parameters, options, Log);
            if (!result.IsValid)
            {
                if (result.Exception is SecurityTokenSignatureKeyNotFoundException)
                {
                    Log.Warn("Key for validating token signature cannot be found. Refreshing keyset.");

                    return Task.FromResult(new IdentityTokenValidationResult
                    {
                        Error = "invalid_signature"
                    });
                }

                if (result.Exception is SecurityTokenUnableToValidateException)
                {
                    return Task.FromResult(new IdentityTokenValidationResult
                    {
                        Error = "unable_to_validate_token"
                    });
                }

                throw result.Exception;
            }

            ClaimsPrincipal user = new ClaimsPrincipal(result.ClaimsIdentity);
            
            string error = CheckRequiredClaim(user);
            if (error.IsPresent())
            {
                return Task.FromResult(new IdentityTokenValidationResult
                {
                    Error = error
                });
            }

            return Task.FromResult(new IdentityTokenValidationResult
            {
                User = user,
                SignatureAlgorithm = algorithm
            });
        }

        private TokenValidationResult ValidateSignature(string identityToken, JsonWebTokenHandler handler, TokenValidationParameters parameters, OidcClientOptions options, Logger logger)
        {
            if (parameters.RequireSignedTokens)
            {
                // read keys from provider information
                List<SecurityKey> keys = new List<SecurityKey>();

                foreach (JsonWebKey webKey in options.ProviderInformation.KeySet.Keys)
                {
                    if (webKey.E.IsPresent() && webKey.N.IsPresent())
                    {
                        // only add keys used for signatures
                        if (webKey.Use == "sig" || webKey.Use == null)
                        {
                            byte[] e = Base64Url.Decode(webKey.E);
                            byte[] n = Base64Url.Decode(webKey.N);

                            RsaSecurityKey key = new RsaSecurityKey(new RSAParameters { Exponent = e, Modulus = n })
                            {
                                KeyId = webKey.Kid
                            };

                            keys.Add(key);

                            logger.Debug("Added signing key with kid: {kid}", key.KeyId ?? "not set");
                        }
                    }
                    else if (webKey.X.IsPresent() && webKey.Y.IsPresent() && webKey.Crv.IsPresent())
                    {
                        ECDsa ec = ECDsa.Create(new ECParameters
                        {
                            Curve = GetCurveFromCrvValue(webKey.Crv),
                            Q = new ECPoint
                            {
                                X = Base64Url.Decode(webKey.X),
                                Y = Base64Url.Decode(webKey.Y)
                            }
                        });

                        ECDsaSecurityKey key = new ECDsaSecurityKey(ec)
                        {
                            KeyId = webKey.Kid
                        };

                        keys.Add(key);
                    }
                    else
                    {
                        logger.Debug("Signing key with kid: {kid} currently not supported", webKey.Kid ?? "not set");
                    }
                }

                parameters.IssuerSigningKeys = keys;
            }
            
            return handler.ValidateToken(identityToken, parameters);
        }

        private static string CheckRequiredClaim(ClaimsPrincipal user)
        {
            List<string> requiredClaims = new List<string>
            {
                JwtClaimTypes.Issuer,
                JwtClaimTypes.Subject,
                JwtClaimTypes.IssuedAt,
                JwtClaimTypes.Audience,
                JwtClaimTypes.Expiration,
            };

            foreach (string claimType in requiredClaims)
            {
                Claim claim = user.FindFirst(claimType);
                if (claim == null)
                {
                    return $"{claimType} claim is missing";
                }
            }

            return null;
        }

        internal static ECCurve GetCurveFromCrvValue(string crv)
        {
            switch (crv)
            {
                case JsonWebKeyECTypes.P256:
                    return ECCurve.NamedCurves.nistP256;
                case JsonWebKeyECTypes.P384:
                    return ECCurve.NamedCurves.nistP384;
                case JsonWebKeyECTypes.P521:
                    return ECCurve.NamedCurves.nistP521;
                default:
                    throw new InvalidOperationException($"Unsupported curve type of {crv}");
            }
        }
    }
}
