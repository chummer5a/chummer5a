using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ChummerHub.Services.JwT
{

    public class JwtAuthenticationDefaults
    {
        public const string AuthenticationScheme = "JWT";
        public const string HeaderName = "Authorization";
        public const string BearerPrefix = "Bearer";
    }

    ///// <summary>
    ///// User to Login
    ///// </summary>
    //public class User
    //{
    //    /// <summary>
    //    /// UserName or Email
    //    /// </summary>
    //    /// <example>archon.megalon@gmail.com</example>
    //    public string Username { get; internal set; }
    //    /// <summary>
    //    /// password
    //    /// </summary>
    //    /// <example>yourPassword</example>
    //    public string Password { get; internal set; }
    //}

    //public class UserState
    //{
    //    public UserState()
    //    {
    //        Roles = new List<string>();
    //    }
    //    public Guid UserId { get; set; }
    //    public string Name { get; set; }
    //    public string Email { get; set; }
    //    public List<string> Roles { get; internal set; }
    //}


    public class JwtHelper
    {
        private ILogger _logger;
        private readonly IConfiguration _configuration;

        public JwtHelper(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        private  JwtTokenClass _jwtToken = null;

        public  JwtTokenClass jwtToken
        {
            get
            {
                if (_jwtToken == null)
                {
                    _jwtToken = new JwtTokenClass(_logger, _configuration);
                }
                return _jwtToken;
            }
        }

        public static async Task<ApplicationUser> GetApplicationUserAsync(ClaimsPrincipal claimsuser, UserManager<ApplicationUser> userManager)
        {
            ApplicationUser user = null;
            if ((from a in claimsuser.Claims where a.Type == ClaimTypes.Name select a).Any())
            {
                var username = (from a in claimsuser.Claims where a.Type == ClaimTypes.Name select a.Value).FirstOrDefault();
                user = await userManager.FindByNameAsync(username);
            }
            return user;
        }

        public JwtSecurityToken GenerateJwTSecurityToken(ApplicationUser user, IList<string> roles)
        {
            JwtSecurityToken token = null;
            List<Claim> claims = new List<Claim>();
            //claims.Add(new Claim("idp", "local");
            claims.Add(new Claim("issued at", DateTime.UtcNow.ToString()));
            claims.Add(new Claim("notbefore", DateTime.UtcNow.ToString()));
            claims.Add(new Claim(ClaimTypes.Role, "PublicAccess"));
            if (user != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserName));
                claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
                claims.Add(new Claim(ClaimTypes.IsPersistent, true.ToString()));
                if (roles != null)
                {
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }
            }
            JwtHelper helper = new JwtHelper(_logger, _configuration);
            // create a new token with token helper and add our claim
            token = helper.GetJwtToken(
                user?.UserName,
                helper.jwtToken.SigningKey,
                helper.jwtToken.Issuer,
                helper.jwtToken.Audience,
                TimeSpan.FromMinutes(helper.jwtToken.TokenTimeoutMinutes),
                claims.ToArray());
            return token;
        }

        /// <summary>
        /// Returns a Jwt Token from basic input parameters
        /// </summary>
        /// <param name="username"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="issuer"></param>
        /// <param name="audience"></param>
        /// <param name="expiration"></param>
        /// <param name="additionalClaims"></param>
        /// <returns></returns>
        public JwtSecurityToken GetJwtToken(
            string username,
            string uniqueKey,
            string issuer,
            string audience,
            TimeSpan expiration,
            Claim[] additionalClaims = null)
        {
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            if (!String.IsNullOrWhiteSpace(username))
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, username));

            if (additionalClaims is object)
            {
                var claimList = new List<Claim>(claims);
                claimList.AddRange(additionalClaims);
                claims = claimList;
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(uniqueKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                expires: DateTime.UtcNow.Add(expiration),
                claims: claims,
                signingCredentials: creds
            );
        }

        /// <summary>
        /// Returns a token string from base claims
        /// </summary>
        /// <param name="username"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="issuer"></param>
        /// <param name="audience"></param>
        /// <param name="expiration"></param>
        /// <param name="additionalClaims"></param>
        /// <returns></returns>
        public string GetJwtTokenString(ILogger logger,
            string username,
            string uniqueKey,
            string issuer,
            string audience,
            TimeSpan expiration,
            Claim[] additionalClaims = null)
        {
            var token = GetJwtToken(username, uniqueKey, issuer, audience, expiration, additionalClaims);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Converts an existing Jwt Token to a string
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static string GetJwtTokenString(JwtSecurityToken token)
        {
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Returns an issuer key
        /// </summary>
        /// <param name="issuerKey"></param>
        /// <returns></returns>
        public static SymmetricSecurityKey GetSymetricSecurityKey(string issuerKey)
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerKey));
        }
    }
}
