using log4net;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OktaJS_SDK.Models;
using Okta.Core.Models;
using OktaJS_SDK.Services;
using Okta.Core.Clients;
using System.Net.Http;
using Okta.Core;

namespace OktaJS_SDK.Controllers
{
    public class AltLandingController : Controller
    {
        ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        NameValueCollection appSettings = ConfigurationManager.AppSettings;
        // Org settings for primary Org
        private static string primaryOrgUrl = ConfigurationManager.AppSettings["okta.ApiUrl"];
        private static string primaryOrgApiToken = ConfigurationManager.AppSettings["okta.ApiToken"];
        private OktaOidcHelper oktaOidcHelper = new OktaOidcHelper(primaryOrgUrl, primaryOrgApiToken);

        [HttpGet]
        public ActionResult UnprotectedLanding()
        {
            logger.Debug("UnprotectedLanding");
            GetInfoResponse getInfoResponse = new GetInfoResponse();
            TempData["oktaOrg"] = primaryOrgUrl;
            return View(getInfoResponse);
        }



        [HttpGet]
        public ActionResult ImplicitLanding()
        {
            logger.Debug("ImplicitLanding");
            System.IdentityModel.Tokens.Jwt.JwtSecurityToken tokenReceived = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken();

            TempData["oktaOrg"] = primaryOrgUrl;

            return View("ImplicitLanding", tokenReceived);
        }



        [HttpPost]
        public ActionResult ImplicitLanding(string idToken)
        {
            logger.Debug("Post ImplicitLanding");
            // GetInfoResponse getInfoResponse = new GetInfoResponse();
            System.Security.Claims.ClaimsPrincipal jsonPayload = null;
            System.IdentityModel.Tokens.Jwt.JwtSecurityToken tokenReceived = null;
            string issuer = appSettings["oidc.issuer"];
            string audience = appSettings["oidc.spintweb.clientId"];

            jsonPayload = oktaOidcHelper.ValidateIdToken(idToken, issuer, audience);
            if (jsonPayload.Identity.IsAuthenticated)
            {
                TempData["errMessage"] = "Id Token Validated";
                tokenReceived = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(idToken);
            }
            else
            {
                TempData["errMessage"] = "Invalid ID Token!";

            }
            TempData["idToken"] = idToken;
            TempData["oktaOrg"] = primaryOrgUrl;

            return View("ImplicitLanding", tokenReceived);
        }




        [HttpPost]
        public ActionResult GetUserInfo()
        {
            logger.Debug("Home GetUserInfo");

            string accessToken = Request["accessToken"];
            string idToken = Request["idToken"];
            string userinfo_but = Request["userinfo_but"];
            string introspection_but = Request["introspection_but"];
            string userinfo2_but = Request["userinfo2_but"];
            string introspection2_but = Request["introspection2_but"];
            string oidc_but = Request["oidc_but"];
            string session_id = Request["session_id"];
            string revoke_but = Request["revoke_but"];
            string revoke2_but = Request["revoke2_but"];

            string error = null;
            string error_description = null;
            string token_type = null;
            string scope = null;
            string active = null;
            IRestResponse<GetInfoResponse> getInfoRsp = null;
            GetInfoResponse rspData = new GetInfoResponse();

            if (userinfo_but == "Get UserInfo" || userinfo2_but == "Get UserInfo")
            {
                if (!string.IsNullOrEmpty(accessToken))
                {
                    var client = new RestClient(primaryOrgUrl + "/oauth2/v1/userinfo");
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("Accept", "application/json");
                    request.AddHeader("Authorization", " Bearer " + accessToken);
                    getInfoRsp = client.Execute<GetInfoResponse>(request);
                    if (getInfoRsp.Data != null)
                    {
                        error = getInfoRsp.Data.error;
                        error_description = getInfoRsp.Data.error_description;
                        rspData = getInfoRsp.Data;
                    }

                    if (error != null)
                    {
                        TempData["errMessage"] = "Get UserInfo error " + error_description;
                    }
                    else
                    {
                        if (getInfoRsp.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            TempData["errMessage"] = "Get UserInfo Bad Request ";
                        }
                        else
                        {
                            TempData["errMessage"] = "Get UserInfo SUCCESS email = " + getInfoRsp.Data.email;
                        }
                    }
                }
                else
                {
                    TempData["errMessage"] = "Get UserInfo error; access_token NOT present";
                }
            }


            if (introspection_but == "Token Introspection" || introspection2_but == "Token Introspection")
            {
                logger.Debug("Token Introspection");
                var client = new RestClient(primaryOrgUrl + "/oauth2/v1/introspect");

                IRestResponse<IntrospectionResponse> introspectRsp = null;
                string basicAuth = appSettings["oidc.spintweb.clientId"] + ":" + appSettings["oidc.spintweb.clientSecret"];

                var bytesBasicAuth = System.Text.Encoding.UTF8.GetBytes(basicAuth);
                string encodedBasicAuth = System.Convert.ToBase64String(bytesBasicAuth);

                if (!string.IsNullOrEmpty(accessToken))
                {
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("Accept", "application/json");
                    request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                    request.AddHeader("Authorization", " Basic " + encodedBasicAuth);
                    request.AddQueryParameter("token", accessToken);
                    request.AddQueryParameter("token_type_hint", "access_token");
                    //request.AddQueryParameter("token", idToken);
                    //request.AddQueryParameter("token_type_hint", "id");
                    introspectRsp = client.Execute<IntrospectionResponse>(request);
                    if (introspectRsp.Data != null)
                    {
                        error = introspectRsp.Data.error;
                        error_description = introspectRsp.Data.error_description;
                        token_type = introspectRsp.Data.token_type;
                        scope = introspectRsp.Data.scope;
                        active = introspectRsp.Data.active;
                    }

                    if (introspectRsp.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        TempData["errMessage"] = "Token Introspection Bad Request ";
                    }
                    else
                    {
                        if (error != null)
                        {
                            TempData["errMessage"] = "Token Introspection error " + error_description;
                        }
                        else
                        {
                            TempData["errMessage"] = "Token Introspection SUCCESS " + " is_active: " + active;
                        }
                    }
                }
                else
                {
                    TempData["errMessage"] = "Token Introspection; access_token NOT present";
                }
            }

            if (revoke_but == "Token Revoke" || revoke2_but == "Token Revoke")
            {
                logger.Debug("Token Revoke");
                var client = new RestClient(primaryOrgUrl + "/oauth2/v1/revoke");

                IRestResponse revokeRsp = null;
                string basicAuth = appSettings["oidc.spintweb.clientId"] + ":" + appSettings["oidc.spintweb.clientSecret"];

                var bytesBasicAuth = System.Text.Encoding.UTF8.GetBytes(basicAuth);
                string encodedBasicAuth = System.Convert.ToBase64String(bytesBasicAuth);

                if (!string.IsNullOrEmpty(accessToken))
                {
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("Accept", "application/json");
                    request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                    request.AddHeader("Authorization", " Basic " + encodedBasicAuth);
                    request.AddQueryParameter("token", accessToken);
                    request.AddQueryParameter("token_type_hint", "access_token");
                    revokeRsp = client.Execute(request);

                    if (revokeRsp.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        TempData["errMessage"] = "Token Revoke Bad Request ";
                    }
                    else
                    {
                        TempData["errMessage"] = "Token Revoke SUCCESS ";
                    }
                }
                else
                {
                    TempData["errMessage"] = "Token Revoke; access_token NOT present";
                }
            }


            if (oidc_but == "Initiate Auth OIDC")
            {
                logger.Debug("Initiate OIDC Auth Code with Session");

                Random random = new Random();
                string stateCode = random.Next(99999, 1000000).ToString();
                string oauthUrl = primaryOrgUrl + "/oauth2/v1/authorize?response_type=code&response_mode=query&client_id=" + appSettings["oidc.spintweb.clientId"] + "&scope=openid profile email address phone groups offline_access&state=" + stateCode + "&redirect_uri=" + appSettings["oidc.spintweb.RedirectUri_AuthCode"];
                return Redirect(oauthUrl);
            }

            if (oidc_but == "Initiate Implicit OIDC")
            {
                logger.Debug("Initiate OIDC Implicit with Session");

                //option 1
                Random random = new Random();
                string stateCode = random.Next(99999, 1000000).ToString();
                string oauthUrl = primaryOrgUrl + "/oauth2/v1/authorize?response_type=id_token&response_mode=form_post&client_id=" + appSettings["oidc.spintweb.clientId"] + "&scope=openid profile email address phone groups&state=" + stateCode + "&nonce=CWBb0zHdZ92WqBLkyIuExu&redirect_uri=" + appSettings["oidc.spintweb.RedirectUri_Implicit"];
                return Redirect(oauthUrl);


                //option 2
                //note this wont recognize thje sessionCooie in browser since rest API is sent from server and by - passing browser.
                //IRestResponse response = null;
                //var client = new RestClient(MvcApplication.apiUrl + "/oauth2/v1/authorize");
                //client.CookieContainer = new System.Net.CookieContainer();
                //var request = new RestRequest(Method.GET);
                //request.AddHeader("Accept", "application/json");
                //request.AddHeader("Content-Type", "application/json");
                //request.AddQueryParameter("client_id", appSettings["oidc.spintweb.clientId"]);
                //request.AddQueryParameter("response_type", "id_token");
                //request.AddQueryParameter("response_mode", "okta_post_message");
                //request.AddQueryParameter("scope", "openid");
                //request.AddQueryParameter("redirect_uri", appSettings["oidc.spintweb.RedirectUri_Implicit"]);
                //request.AddQueryParameter("state", "myStateInfo");
                //request.AddQueryParameter("nonce", "myNonce");
                //response = client.Execute(request);

                ////int myIndex_01 = response.Content.IndexOf("data.id_token =");
                ////int myIndex_02 = response.Content.IndexOf(";");
                ////var myIdToken = response.Content.Substring(myIndex_01, myIndex_02);
                ////logger.Debug(myIdToken);
                //ViewBag.HtmlStr = response.Content;
                //return View("../AltLanding/MyContent");

            }

            TempData["accessToken"] = accessToken;
            TempData["idToken"] = idToken;
            TempData["oktaOrg"] = appSettings["okta.ApiUrl"];
            return View("AuthCodeLanding", rspData);
        }


    }
}