using log4net;
using Okta.Core;
using Okta.Core.Clients;
using Okta.Core.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Security.Cryptography;
using RestSharp;
using System.IdentityModel.Tokens;
using System.Reflection;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Diagnostics;
using System.Web.Mvc;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Web.UI;
using OktaJS_SDK.Models;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace OktaJS_SDK.Services
{
    public class OktaOidcHelper
    {
        //private OktaSettings _orgSettings;
        //private string _apiToken;
        //private string _orgUrl;

        //private UsersClient _usersClient;
        //private OktaClient _oktaClient;

        private static ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public OktaOidcHelper(string orgUrlParam, string apiToken)
        {
            //_orgUrl = orgUrlParam;
            //Uri orgUri = new Uri(OrgUrl);
            //_orgSettings = new OktaSettings();
            //_orgSettings.ApiToken = apiToken;
            //_orgSettings.BaseUri = orgUri;

            //_oktaClient = new OktaClient(_orgSettings);
            //_usersClient = new UsersClient(_orgSettings);

        }

        public System.Security.Claims.ClaimsPrincipal ValidateIdToken(string idToken, string issuer, string audience)
        {
            System.Security.Claims.ClaimsPrincipal claimPrincipal = null;

            IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{issuer}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
            OpenIdConnectConfiguration openIdConfig = AsyncHelper.RunSync(async () => await configurationManager.GetConfigurationAsync(CancellationToken.None));

            TokenValidationParameters validationParameters =
                new TokenValidationParameters
                {
                    ValidAudience = audience,
                    ValidIssuer = issuer,
                    IssuerSigningKeys = openIdConfig.SigningKeys,
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true
                };

            Microsoft.IdentityModel.Tokens.SecurityToken validatedToken;
            System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

            try
            {
                claimPrincipal = handler.ValidateToken(idToken, validationParameters, out validatedToken);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
            }

            return claimPrincipal;
        }

    }
}