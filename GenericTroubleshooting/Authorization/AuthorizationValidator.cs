using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using GenericTroubleshooting.Config;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace GenericTroubleshooting.Authorization
{
    public static class AuthorizationValidator
    {
        private static readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        static AuthorizationValidator()
        {
            string openIdWellKnownConfigUrl = $"{SolutionConstants.AmazonCognito.ApprovedIssuer}/.well-known/openid-configuration";
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            var documentRetriever = new HttpDocumentRetriever { RequireHttps = openIdWellKnownConfigUrl.StartsWith("https://") };
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                metadataAddress: openIdWellKnownConfigUrl,
                configRetriever: new OpenIdConnectConfigurationRetriever(),
                docRetriever: documentRetriever
            );
        }

        public static async Task<ClaimsPrincipal> ValidateTokenAsync(string value, Dictionary<string, object> loggingAttributeDictionary)
        {
            var authScheme = "Bearer ";
            var authPrefixIndex = value.IndexOf(authScheme);
            if (authPrefixIndex == -1) return null; //not the right scheme
            //remove scheme
            value = value.Substring(authPrefixIndex + authScheme.Length);

            var config = await _configurationManager.GetConfigurationAsync(CancellationToken.None);
            var validationParameter = new TokenValidationParameters

            {
                RequireSignedTokens = true,
                //ValidAudience = SolutionConstants.AmazonCognito.ApprovedAudience,
                ValidateAudience = false,
                ValidIssuer = SolutionConstants.AmazonCognito.ApprovedIssuer,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKeys = config.SigningKeys
            };

            ClaimsPrincipal result = null;
            var tries = 0;
            while (result == null && tries <= 1)
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    result = handler.ValidateToken(token: value, validationParameters: validationParameter, out var token);
                }
                catch (SecurityTokenSignatureKeyNotFoundException ex1)
                {
                    // This exception is thrown if the signature key of the JWT could not be found.
                    // This could be the case when the issuer changed its signing keys, so we trigger a 
                    // refresh and retry validation.
                    loggingAttributeDictionary.Add(key: "SecurityTokenSignatureKeyNotFoundException", value: ex1.Message);
                    _configurationManager.RequestRefresh();
                    tries++;
                }
                catch (SecurityTokenExpiredException ex2)
                {
                    loggingAttributeDictionary.Add(key: "SecurityTokenExpiredException", value: ex2.Message);
                    return null;
                }
                catch (SecurityTokenInvalidAudienceException ex3)
                {
                    loggingAttributeDictionary.Add(key: "SecurityTokenInvalidAudienceException", value: ex3.Message);
                    return null;
                }
                catch (SecurityTokenInvalidIssuerException ex4)
                {
                    loggingAttributeDictionary.Add(key: "SecurityTokenInvalidIssuerException", value: ex4.Message);
                    return null;
                }
                catch (SecurityTokenInvalidSigningKeyException ex5)
                {
                    loggingAttributeDictionary.Add(key: "SecurityTokenInvalidSigningKeyException", value: ex5.Message);
                    return null;
                }
                catch (SecurityTokenInvalidLifetimeException ex6)
                {
                    loggingAttributeDictionary.Add(key: "SecurityTokenInvalidLifetimeException", value: ex6.Message);
                    return null;
                }
                catch (SecurityTokenInvalidSignatureException ex7)
                {
                    loggingAttributeDictionary.Add(key: "SecurityTokenInvalidSignatureException", value: ex7.Message);
                    return null;
                }
                catch (SecurityTokenException ex8)
                {
                    loggingAttributeDictionary.Add(key: "SecurityTokenException", value: ex8.Message);
                    return null;
                }
            }
            return result;
        }
    }
}
