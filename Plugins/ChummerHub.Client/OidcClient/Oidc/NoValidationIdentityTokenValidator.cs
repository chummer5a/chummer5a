using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.OidcClient.Results;

namespace IdentityModel.OidcClient
{
    /// <inheritdoc />
    public class NoValidationIdentityTokenValidator : IIdentityTokenValidator
    {
        /// <inheritdoc />
        public Task<IdentityTokenValidationResult> ValidateAsync(string identityToken, OidcClientOptions options, CancellationToken cancellationToken = default)
        {
            string[] parts = identityToken.Split('.');
            if (parts.Length != 3)
            {
                IdentityTokenValidationResult error = new IdentityTokenValidationResult
                {
                    Error = "invalid_jwt"
                };

                return Task.FromResult(error);
            }

            string payload = Encoding.UTF8.GetString((Base64Url.Decode(parts[1])));

            Dictionary<string, JsonElement> values =
                JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payload);
            
            List<Claim> claims = new List<Claim>();
            if (values != null)
            {
                foreach (KeyValuePair<string, JsonElement> element in values)
                {
                    if (element.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement item in element.Value.EnumerateArray())
                        {
                            claims.Add(new Claim(element.Key, item.ToString()));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(element.Key, element.Value.ToString()));

                    }
                }
            }

            IdentityTokenValidationResult result = new IdentityTokenValidationResult
            {
                SignatureAlgorithm = "none",
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "none", "name", "role"))
            };

            return Task.FromResult(result);
        }
    }
}
