using log4net;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

using System.Text;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using OktaJS_SDK.Models;
using OktaJS_SDK.Services;
using System.Web.Routing;
//using System.IdentityModel.Tokens.Jwt;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;

namespace OktaJS_SDK.Controllers
{
    public class OidcController : Controller
    {
        ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        NameValueCollection appSettings = ConfigurationManager.AppSettings;

        // Org settings for primary Org
        private static string primaryOrgUrl = ConfigurationManager.AppSettings["okta.ApiUrl"];
        private static string primaryOrgApiToken = ConfigurationManager.AppSettings["okta.ApiToken"];
        private OktaOidcHelper oktaOidcHelper = new OktaOidcHelper(primaryOrgUrl, primaryOrgApiToken);


        [HttpGet]
        public ActionResult Endpoint_Service()
        {
            logger.Debug("Get OIDC Endpoint_Service");
            return RedirectToAction("AuthCodeLanding", "AltLanding");
        }


        [HttpGet]
        public ActionResult Endpoint_AuthCode(string code, string state)
        {
            //use this for auth code workflow
            logger.Debug("Get OIDC Endpoint_AuthCode");

            logger.Debug(" code = " + code + " state " + state);

            string error = null;
            string error_description = null;
            string token_type = null;
            string scope = null;
            string id_token_status = null;
            string idToken = null;
            string access_token_status = null;
            string accessToken = null;
            string refresh_token_status = null;
            string refreshToken = null;
            System.Security.Claims.ClaimsPrincipal newIdentity = null;
            IRestResponse<TokenRequestResponse> response = null;

            OidcAccessToken oidcAccessToken = new OidcAccessToken();
            string basicAuth = appSettings["oidc.spintweb.clientId"] + ":" + appSettings["oidc.spintweb.clientSecret"];

            var bytesBasicAuth = System.Text.Encoding.UTF8.GetBytes(basicAuth);
            string encodedBasicAuth = System.Convert.ToBase64String(bytesBasicAuth);


            try
            {
                var client = new RestClient(primaryOrgUrl + "/oauth2/v1/token");
                var request = new RestRequest(Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("Authorization", " Basic " + encodedBasicAuth);
                request.AddQueryParameter("grant_type", "authorization_code");
                request.AddQueryParameter("code", code);
                request.AddQueryParameter("redirect_uri", appSettings["oidc.spintweb.RedirectUri_AuthCode"]);
                response = client.Execute<TokenRequestResponse>(request);
                if (response.Data.error != null)
                {
                    error = response.Data.error;
                    error_description = response.Data.error_description;
                }

                token_type = response.Data.token_type;
                scope = response.Data.scope;

                if (response.Data.id_token != null)
                {
                    id_token_status = "id_token present";
                    idToken = response.Data.id_token;
                    string issuer = appSettings["oidc.issuer"];
                    string audience = appSettings["oidc.spintweb.clientId"];
                    newIdentity = oktaOidcHelper.ValidateIdToken(idToken, issuer, audience);
                    if (newIdentity.Identity.IsAuthenticated)
                    {
                        TempData["errMessage"] = "ID Token Valid!";
                    }
                    else
                    {
                        TempData["errMessage"] = "Invalid ID Token!";

                    }
                    TempData["idToken"] = idToken;
                }
                else
                {
                    id_token_status = "id_token NOT present";
                }

                if (response.Data.access_token != null)
                {
                    accessToken = response.Data.access_token;
                    access_token_status = "access_token present";
                }
                else
                {
                    access_token_status = "access_token NOT present";
                }

                if (response.Data.refresh_token != null)
                {
                    refreshToken = response.Data.refresh_token;
                    refresh_token_status = "refresh_token present";
                }
                else
                {
                    refresh_token_status = "refresh_token NOT present";
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            if (error != null)
            {
                TempData["oktaOrg"] = primaryOrgUrl;
                return RedirectToAction("UnprotectedLanding", "AltLanding");
            }
            else
            {
                TempData["oktaOrg"] = primaryOrgUrl;
                return RedirectToAction("AuthCodeLanding", "AltLanding");
            }
        }



        [HttpPost]
        public ActionResult Endpoint_Implicit()
        {
            //use this for implicit workflow
            logger.Debug("Post OIDC Endpoint_Implicit");
            string myState = Request["state"];
            string idToken = Request["id_token"];
            string accessToken = Request["access_token"];
            string refreshToken = Request["refresh_token"];
            string tokenType = Request["token_type"];
            string expires = Request["expires_in"];
            string scope = Request["scope"];

            System.Security.Claims.ClaimsPrincipal jsonPayload = null;
            System.IdentityModel.Tokens.Jwt.JwtSecurityToken tokenReceived = null;
            string accessTokenStatus = null;
            string idTokenStatus = null;


            if (idToken != null)
            {
                idTokenStatus = " ID Token Present";
                string issuer = appSettings["oidc.issuer"];
                string audience = appSettings["oidc.spintweb.clientId"];
                jsonPayload = oktaOidcHelper.ValidateIdToken(idToken, issuer, audience);
                if (jsonPayload.Identity.IsAuthenticated)
                {
                    TempData["errMessage"] = jsonPayload.ToString();
                    tokenReceived = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(idToken);
                }
                else
                {
                    TempData["errMessage"] = "Invalid ID Token!";
                }
                TempData["idToken"] = idToken;
            }
            else
            {
                idTokenStatus = " ID Token Not Found";
            }

            if (accessToken != null)
            {
                accessTokenStatus = "access_token Present";
                TempData["accessToken"] = accessToken;
            }
            else
            {
                accessTokenStatus = "access_token NOT Found";
            }

            if (accessToken != null || idToken != null)
            {
                TempData["errMessage"] = "OIDC_Get Oauth Endpoint_Native SUCCESS = " + idTokenStatus + " : " + accessTokenStatus;
                TempData["oktaOrg"] = primaryOrgUrl;
                return View("../AltLanding/ImplicitLanding", tokenReceived);
            }
            else
            {
                TempData["errMessage"] = "OIDC_Get Oauth Endpoint_Native Error = " + idTokenStatus + " : " + accessTokenStatus;
                TempData["oktaOrg"] = primaryOrgUrl;
                return View("../AltLanding/UnprotectedLanding");
            }


        }

    }
}