// Copyright Siemens 2025
using Camstar.WebPortal.Utilities;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Web;
using System.Web.UI;

/// <summary>
/// Summary description for SingleSignOnHelper
/// </summary>
/// 
namespace Camstar.WebPortal.Helpers
{
    public class SingleSignOnHelper
    {

        public static bool ValidateToken(string token, out ClaimsPrincipal claimsPrincipal)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var issuer = ConfigurationManager.AppSettings["SSOIssuer"];
            var keyId = ConfigurationManager.AppSettings["SSOKeyId"];
            var keyRaw = ConfigurationManager.AppSettings["SSOKey"];
            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyRaw)) { KeyId = keyId };

            // Define token validation parameters
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,

                ValidateAudience = false,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out Microsoft.IdentityModel.Tokens.SecurityToken validatedToken);
                return true;
            }
            catch (Exception ex)
            {
                claimsPrincipal = null;
                return false;
            }
        }
        public static bool ValidateRefreshAccessToken()
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    var context = HttpContext.Current;
                    if (context == null || context.Request == null)
                        return false;
                    var claims = new Dictionary<string, string>()
                            {
                                { "AccessKey", AuthManager.AccessKey },
                                { "SecretAccessKey", AuthManager.SecretAccessKey },
                                { "RefreshToken", AuthManager.RefreshToken},
                                { "TokenExpirationDate", AuthManager.TokenExpirationDate.ToString() },
                                { "AccessToken", AuthManager.AccessToken },
                                { "IdToken", AuthManager.IdToken }
                            };
                    var tokenClaim = GenerateToken(claims);
                    httpClient.DefaultRequestHeaders.Add("X-Token", tokenClaim);
                    string baseUrl = context.Request.Url.GetLeftPart(UriPartial.Authority);
                    string relativePath = ConfigurationManager.AppSettings["validateSamTokenApiRelativePath"];
                    HttpResponseMessage response = httpClient.GetAsync(baseUrl + relativePath).GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                        return false;
                    else
                    {
                        string token = response.Content.ReadAsStringAsync().Result;

                        if (ValidateToken(token, out ClaimsPrincipal principal))
                        {

                            AuthManager.AccessKey = principal.FindFirst("AccessKey")?.Value;
                            AuthManager.SecretAccessKey = principal.FindFirst("SecretAccessKey")?.Value;
                            AuthManager.RefreshToken = principal.FindFirst("RefreshToken")?.Value;
                            AuthManager.TokenExpirationDate = DateTime.Parse(principal.FindFirst("TokenExpirationDate")?.Value);
                            AuthManager.AccessToken = principal.FindFirst("AccessToken")?.Value;
                            AuthManager.IdToken = principal.FindFirst("IdToken")?.Value;
                            return true;
                        }
                        else
                            return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public static string GenerateToken(Dictionary<string, string> claimData)
        {
            try
            {
                var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["SSOKey"]));
                var keyid = ConfigurationManager.AppSettings["SSOKeyId"];
                var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);
                var expireTime = ConfigurationManager.AppSettings["SessionExpireTime"];
                var issuer = ConfigurationManager.AppSettings["SSOIssuer"];

                var header = new JwtHeader(credentials);
                header["kid"] = keyid;

                var claims = claimData.Select(item => new Claim(item.Key, item.Value)).ToArray();

                var payload = new JwtPayload(
                    issuer: issuer,
                    audience: null,
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(expireTime))
                );

                var token = new JwtSecurityToken(header, payload);
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
}
