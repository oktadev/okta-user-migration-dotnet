using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Okta.Core.Models;
using Okta.Core;
using Okta.Core.Clients;
using OktaJS_SDK.Models;
using OktaJS_SDK.Services;
using System.Configuration;
using System.Collections.Specialized;

namespace OktaJS_SDK.Controllers
{
    public class FeatureController : Controller
    {

        NameValueCollection appSettings = ConfigurationManager.AppSettings;
        private string _apiToken;
        private string _apiUrl;
        private string _userdomain;


        private OktaUserMgmt _oktaUserMgmt;
        private ICredAuthentication _credAuthentication;
        CustomUser rspAddCustomUser;

        public FeatureController(ICredAuthentication credAuthentication)
        {

            _credAuthentication = credAuthentication; //the service is injected by the GL app, the parameters are passed in on the method call
 
            _apiUrl = appSettings["okta.ApiUrl"];
            _apiToken = appSettings["okta.ApiToken"];
            _oktaUserMgmt = new OktaUserMgmt(_apiUrl, _apiToken);
            _userdomain = appSettings["user.domain"];
        }



        //api assocaited with Password Migration feature
        [HttpPost]
        [Route("api/ValidateUser")]
        public ActionResult ValidateUser()
        {
            PswMigrationResponse pswMigrationRsp = new PswMigrationResponse();
            
            LdapServiceModel ldapServiceModel = null;
            CustomUser oktaUser = null;
            string username = null;
            string password = null;

            username = Request["username"];
            password = Request["password"];

            ldapServiceModel = new LdapServiceModel();
            ldapServiceModel.ldapServer = appSettings["ldap.server"];
            ldapServiceModel.ldapPort = appSettings["ldap.port"];
            ldapServiceModel.baseDn = appSettings["ldap.baseDn"];

            //use received username and password to bind with LDAP
            //if password is valid, set password in Okta
            try
            {
                //check username in Okta and password status
                oktaUser = _oktaUserMgmt.GetCustomUser(username);
            }
            catch (OktaException)
            {
                //trap error, handle User is null
            }

            if (oktaUser != null)
            {

                if (string.IsNullOrEmpty(oktaUser.Profile.IsPasswordInOkta) || oktaUser.Profile.IsPasswordInOkta == "false")
                {
                    //check user credentials in LDAP
                    bool rspIsAuthenticated = _credAuthentication.IsAuthenticated(username, password, ldapServiceModel);

                    if (rspIsAuthenticated)
                    {
                        //set password in Okta
                        bool rspSetPsw = _oktaUserMgmt.SetUserPassword(oktaUser.Id, password);
                        if (rspSetPsw)
                        {
                            //update attribute in user profile when set password successful
                            oktaUser.Profile.IsPasswordInOkta = "true";
                            bool rspPartialUpdate = _oktaUserMgmt.UpdateCustomUserAttributesOnly(oktaUser);
                            if (rspPartialUpdate)
                            {
                                pswMigrationRsp.status = "set password in Okta successful";
                                pswMigrationRsp.isPasswordInOkta = "true";
                            }
                            else
                            {
                                pswMigrationRsp.status = "set password in Okta successful";
                                pswMigrationRsp.isPasswordInOkta = "unknown";
                            }
                        }
                        else
                        {
                            //update attribute in user profile when set password fails
                            oktaUser.Profile.IsPasswordInOkta = "true";
                            bool rspPartialUpdate = _oktaUserMgmt.UpdateCustomUserAttributesOnly(oktaUser);
                            if (rspPartialUpdate)
                            {
                                pswMigrationRsp.status = "set password in Okta failed";
                                pswMigrationRsp.isPasswordInOkta = "false";
                            }
                            else
                            {
                                pswMigrationRsp.status = "set password in Okta failed";
                                pswMigrationRsp.isPasswordInOkta = "unknown";
                            }
                        }
                    }
                    else
                    {
                        //arrive here is user creds not validated in Ldap
                        pswMigrationRsp.status = "LDAP validation failed";
                        pswMigrationRsp.isPasswordInOkta = "false";
                    }

                }
                else
                {
                    //no work required
                    pswMigrationRsp.status = oktaUser.Status;
                    pswMigrationRsp.isPasswordInOkta = "true";
                }
                //build response
                pswMigrationRsp.oktaId = oktaUser.Id;
                pswMigrationRsp.login = oktaUser.Profile.Login;

            }
            else
            {
                //arrive here if user not found in Okta
                //check user credentials and get profile from LDAP
                CustomUser rspCustomUser = _credAuthentication.IsCreated(username, password, ldapServiceModel);
                if (rspCustomUser != null)
                {
                    rspCustomUser.Profile.Login = username + _userdomain;                   
                    Okta.Core.Models.Password pswd = new Okta.Core.Models.Password();
                    pswd.Value = password;
                    rspCustomUser.Credentials.Password = pswd;

                    //create Okta user with password
                    rspAddCustomUser = _oktaUserMgmt.AddCustomUser(rspCustomUser);
                    if (rspAddCustomUser != null)
                    {
                        Uri rspUri = new Uri("https://tbd.com");
                        bool rspActivate = _oktaUserMgmt.ActivateUser(rspAddCustomUser, out rspUri);
                        if (rspActivate)
                        {
                            rspCustomUser.Profile.IsPasswordInOkta = "true";
                            bool rspPartialUpdate = _oktaUserMgmt.UpdateCustomUserAttributesOnly(rspAddCustomUser);
                            if (rspPartialUpdate)
                            {
                                pswMigrationRsp.oktaId = rspAddCustomUser.Id;
                                pswMigrationRsp.login = rspAddCustomUser.Profile.Login;
                                pswMigrationRsp.status = "Created in Okta";
                                pswMigrationRsp.isPasswordInOkta = "true";
                            }
                            else
                            {
                                pswMigrationRsp.oktaId = rspAddCustomUser.Id;
                                pswMigrationRsp.login = rspAddCustomUser.Profile.Login;
                                pswMigrationRsp.status = "Created in Okta";
                                pswMigrationRsp.isPasswordInOkta = "unknown";
                            }

                        }
                        else
                        {
                            pswMigrationRsp.oktaId = "none";
                            pswMigrationRsp.login = "none";
                            pswMigrationRsp.status = "User NOT Created in Okta";
                            pswMigrationRsp.isPasswordInOkta = "false";
                        }
                    }
                    else
                    {
                        pswMigrationRsp.oktaId = "none";
                        pswMigrationRsp.login = "none";
                        pswMigrationRsp.status = "User NOT Created in Okta";
                        pswMigrationRsp.isPasswordInOkta = "false";
                    }

                }
                else
                {
                    pswMigrationRsp.oktaId = "none";
                    pswMigrationRsp.login = "none";
                    pswMigrationRsp.status = "User NOT Created in Okta";
                    pswMigrationRsp.isPasswordInOkta = "false";
                }


            }

            return Content(content: JsonConvert.SerializeObject(pswMigrationRsp), contentType: "application/json");
        }




    }
}