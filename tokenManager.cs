using Microsoft.IdentityModel.Tokens;
using PCiServer.DEM.Web.Api.Modules.Admin.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace PCiServer.DEM.Web.Api.Common.Identity
{
    public class TokenManager
    {
        private static string secret = string.Empty;
        private static string issuer = ConfigurationManager.AppSettings["issuer"];
        private static string audience = ConfigurationManager.AppSettings["audience"];
        private static double tokenExpiryTime = Convert.ToDouble(ConfigurationManager.AppSettings["tokenExpiryTime"]);

        /// <summary>
        /// This mehtod is used to generate a token with claims.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GenerateToken(User user, string module)
        {
            SetSecretKey(module);
            byte[] key = Convert.FromBase64String(secret);
            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);


            var claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim("userId", user.UserName));
            claims.Add(new Claim("isAdmin", user.IsAdmin.ToString(), ClaimValueTypes.Boolean));
            claims.Add(new Claim("isInGroup", user.IsInGroup.ToString(), ClaimValueTypes.Boolean));
            claims.Add(new Claim("accessLevel", user.AccessLevel));
            claims.Add(new Claim("canDeleteRecord", user.CanDeleteRecord.ToString(), ClaimValueTypes.Boolean));
            claims.Add(new Claim("canAssignMultipleRecords", user.CanAssignMultipleRecords.ToString(), ClaimValueTypes.Boolean));

            //Create Security Token object by giving required parameters    
            var token = new JwtSecurityToken(issuer,
                            audience,
                            claims,
                            expires: DateTime.UtcNow.AddMinutes(tokenExpiryTime),
                            signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        private static void SetSecretKey(string module)
        {
            if (string.Equals(module, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                secret = ConfigurationManager.AppSettings["jwtSecretAdmin"];
            }
            else
            {
                secret = ConfigurationManager.AppSettings["jwtSecretClient"];
            }
        }

        /// <summary>
        /// This method is used to validate the token. If token is valid then it would return the principle with claims.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
                if (jwtToken == null)
                    return null;

                byte[] key = Convert.FromBase64String(secret);

                TokenValidationParameters parameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                SecurityToken securityToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out securityToken);
                return principal;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// This method is used to generate the refresh token. It would take the clamins from the existing token and add the same for the new token
        /// In order to get the refresh token existing token should not be expired.
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        public static LoginResponseModel GenerateRefreshToken(HttpContext requestContext, string module)
        {
            var identity = (ClaimsIdentity)requestContext.User.Identity;

            User user = new User
            {
                UserName = identity.FindFirst("userId").Value,
                IsAdmin = Convert.ToBoolean(identity.FindFirst("isAdmin").Value),
                IsInGroup = Convert.ToBoolean(identity.FindFirst("isInGroup").Value),
                AccessLevel = identity.FindFirst("accessLevel").Value,
                CanDeleteRecord = Convert.ToBoolean(identity.FindFirst("canDeleteRecord").Value),
                CanAssignMultipleRecords = Convert.ToBoolean(identity.FindFirst("canAssignMultipleRecords").Value),
            };

            string refreshedToken = GenerateToken(user, module);

            var response = new LoginResponseModel
            {
                status = "sucess",
                data = new ResponseData
                {
                    accessToken = refreshedToken
                }
            };

            return response;
        }
    }
}
