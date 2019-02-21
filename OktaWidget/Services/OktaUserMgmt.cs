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
using OktaJS_SDK.Models;
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
using RestSharp;

namespace OktaJS_SDK.Services
{

    public class OktaUserMgmt
    {
        private static OktaSettings _orgSettings;
        private static string _apiToken;
        private static string _orgUrl;
        private UsersClient _usersClient;
        private OktaClient _oktaClient;

        private ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public OktaUserMgmt(string orgUrlParam, string apiToken)
        {
            _orgUrl = orgUrlParam;
            Uri orgUri = new Uri(OrgUrl);
            _orgSettings = new OktaSettings();
            _orgSettings.ApiToken = apiToken;
            _orgSettings.BaseUri = orgUri;
            _oktaClient = new OktaClient(_orgSettings);
            _usersClient = new UsersClient(_orgSettings);

        }


        public string OrgUrl { get { return _orgUrl; } }

        public User GetOktaUserStatus(string oktaId)
        {
            User oktaUser = null;
            //dont trap error allow null user to be processed in main 
            oktaUser = _usersClient.Get(oktaId);

            return oktaUser;
        }
        public User GetBasicOktaUser(string oktaId)
        {
            User oktaUser = null;
            try
            {
                oktaUser = _usersClient.Get(oktaId);
            }
            catch (OktaException ex)
            {
                if (ex.ErrorCode == "E0000007")//resource not found
                {
                    _logger.Debug("User not found; okta Id = " + oktaId);
                }
                else
                {
                    _logger.Error(string.Format("Error searching for Okta user in Okta. Okta Id: {0}.", oktaId), ex);
                }

            }

            return oktaUser;
        }
        public CustomUser GetCustomUser(string oktaId)
        {
            //input parameter can be okta username or oktaId
            User oktaUser = null;
            CustomUser customUser = null;

            try
            {

                oktaUser = _usersClient.Get(oktaId);
                customUser = new CustomUser(oktaUser);
                //customUser.extProfile = new CustomUserProfile();

                List<string> customAttributes = oktaUser.Profile.GetUnmappedPropertyNames();
                foreach (var item in customAttributes)
                {

                    PropertyInfo tempProp = customUser.Profile.GetType().GetProperty(item);
                    if (tempProp != null)
                    {
                        object myValue = oktaUser.Profile.GetProperty(item);
                        if (tempProp.CanWrite)
                        {
                            tempProp.SetValue(customUser.Profile, myValue, null);
                        }
                    }
                    else
                    {
                        _logger.Debug("unmapped okta attribute " + item + " is not defined as an extention");
                        //customUser.Status = "ERROR";
                    }

                }


            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error searching for Okta user in Okta. Okta Id: {0}.", oktaId), ex);
            }

            return customUser;
        }


        public PagedResults<User> ListBasicUsers(Uri nextPage = null)
        {
            PagedResults<User> oktaUserList = null;

            try
            {
                oktaUserList = _usersClient.GetList(nextPage, pageSize: 200);

            }
            catch (Exception ex)
            {
                _logger.Error("Error searching for Okta user in Okta. ");
            }

            return oktaUserList;
        }


        public PagedResults<CustomUser> ListCustomUsersExtended(string searchType, string criteria, Uri nextPage = null)
        {

            string encodedfilter = null;
            string stringFilter = criteria.ToString();
            encodedfilter = HttpUtility.UrlPathEncode(stringFilter);
            FilterBuilder filterBuilder = new FilterBuilder(encodedfilter);
            PagedResults<User> oktaUserList = null;
            List<CustomUser> customUserList = new List<CustomUser>();
            CustomUser customUser = null;
            try
            {
                if (searchType == "query")
                {
                    oktaUserList = _usersClient.GetList(nextPage, query: criteria, pageSize: 200);
                }
                else if (searchType == "search")
                {
                    oktaUserList = _usersClient.GetList(nextPage: nextPage, filter: filterBuilder, searchType: SearchType.ElasticSearch, pageSize: 200);
                }
                else if (searchType == "filter")
                {
                    oktaUserList = _usersClient.GetList(nextPage: nextPage, filter: filterBuilder, searchType: SearchType.Filter, pageSize: 200);
                }



                foreach (var user in oktaUserList.Results)
                {
                    customUser = new CustomUser(user);

                    List<string> customAttributes = user.Profile.GetUnmappedPropertyNames();
                    foreach (var item in customAttributes)
                    {

                        PropertyInfo tempProp = customUser.Profile.GetType().GetProperty(item);
                        if (tempProp != null)
                        {
                            object myValue = user.Profile.GetProperty(item);
                            if (tempProp.CanWrite)
                            {
                                tempProp.SetValue(customUser.Profile, myValue, null);
                            }
                        }
                        else
                        {
                            _logger.Debug("unmapped okta attribute " + item + " is not defined as an extention");
                        }

                    }
                    customUserList.Add(customUser);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error searching for Okta user in Okta. Okta username: {0}.", criteria), ex);
            }
            PagedResults<CustomUser> pagedCustomUserList = new PagedResults<CustomUser>(customUserList);
            if (!pagedCustomUserList.IsLastPage)
            {
                pagedCustomUserList.NextPage = oktaUserList.NextPage;
                pagedCustomUserList.PrevPage = oktaUserList.PrevPage;
            }

            //pagedCustomUserList.RequestUri = oktaUserList.RequestUri;
            return pagedCustomUserList;

        }


        public bool SetupUserInOkta(string username, string password)
        {
            bool returnCode = false;
            CustomUser customUser = GetCustomUser(username);
            if (customUser != null)
            {
                // activate the user if not already activated - don't send activation email
                if (string.Equals(customUser.Status, "STAGED", StringComparison.InvariantCultureIgnoreCase))
                {
                    _usersClient.Activate(customUser.Id, false);
                }

                // Update user's password
                _usersClient.SetPassword(customUser, password);
                returnCode = true;
            }

            return returnCode;
        }

        public bool ActivateUser(CustomUser customUser, out Uri rspUri, bool sendEmail = false)
        {
            rspUri = null;
            bool returnCode = false;
            try
            {
                rspUri = _usersClient.Activate(customUser, sendEmail);
                _logger.Info(string.Format("Activated user in Okta. Okta Name: {0}.", customUser));
                returnCode = true;
            }
            catch (Exception ex)
            {
                _logger.Error("Error activating user. {0}", ex);
                rspUri = null;
            }

            return returnCode;
        }

        //public bool UpdateUserStatus(string status, string oktaId)
        //{
        //    bool success = false;

        //    try
        //    {
        //        if (status.Equals("InActive", StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            // deactivate user
        //            _usersClient.Deactivate(oktaId);

        //            _logger.Info(string.Format("Deactivated user in Okta. Okta Id: {0}.", oktaId));
        //            success = true;
        //        }
        //        else
        //        {
        //            // activate user
        //            // _usersClient.Activate(oktaId, sendActivationEmail: true);
        //            _usersClient.Activate(oktaId);

        //            _logger.Info(string.Format("Activated user in Okta. Okta Id: {0}.", oktaId));

        //            success = true;
        //        }
        //    }
        //    catch (OktaException ex)
        //    {
        //        _logger.Error(string.Format("Error udpating user status in Okta. Okta Id: {0}  Expected status: {1]", oktaId, status), ex);
        //        success = false;
        //    }

        //    return success;
        //}
        public bool SetUserPassword(string oktaId, string password)
        {
            bool success = false;

            try
            {
                //User user = GetCustomUser(oktaId);
                User oktaUser = new User();
                oktaUser.Id = oktaId;
                _usersClient.SetPassword(oktaUser, password);
                success = true;
            }
            catch (OktaException ex)
            {
                _logger.Error("Error resetting user's password. {0}", ex);
            }

            return success;
        }

        public bool ChangeUserPassword(string oktaId, string oldPassword, string newPassword)
        {
            bool success = false;

            try
            {
                User oktaUser = new User();

                _usersClient.ChangePassword(oktaUser, oldPassword, newPassword);
                success = true;
            }
            catch (OktaException ex)
            {
                _logger.Error("Error resetting user's password. {0}", ex);
            }

            return success;
        }

        public bool UpdateOktaUser(User oktaUser)
        {
            bool result = false;
            User oktaUserRsp = null;
            try
            {
                oktaUserRsp = _usersClient.Update(oktaUser);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error updating Okta user. Okta username: {0}.", oktaUser.Profile.Login), ex);
            }

            return result;
        }

        public bool DeactivateUser(User oktaUser)
        {
            bool result = false;

            try
            {
                _usersClient.Deactivate(oktaUser);
                _logger.Info(string.Format("Deactivated user in Okta. Okta Login: {0}.", oktaUser.Profile.Login));
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error deactivating Okta user. Okta username: {0}.", oktaUser.Profile.Login), ex);
            }

            return result;
        }

        public bool UpdateCustomUser(CustomUser customUser)
        {
            bool result = false;

            //User oktaUser = GetOktaUserById(oktaId);
            string oktaId = customUser.Id;
            string apiEndPoint = _orgUrl + Constants.EndpointV1 + Constants.UsersEndpoint + "/" + oktaId;

            //CustomAttributes customAttributes = new CustomAttributes();
            //customAttributes.Profile = customUser.Profile;

            string serializedbody = JsonConvert.SerializeObject(customUser.Profile);

            try
            {

                // OktaHttpClient baseClient = new OktaHttpClient(_orgSettings);

                HttpResponseMessage response = _oktaClient.BaseClient.Post(apiEndPoint, serializedbody);
                string responseTopContent = response.ToString();
                var index = responseTopContent.IndexOf(",", 20);
                var rspStatus = responseTopContent.Substring(0, index);
                string content = response.Content.ReadAsStringAsync().Result;
                _logger.Debug("response " + rspStatus + " content " + content);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error updating Okta user. Okta username: {0}.", customUser.Profile.Login), ex);
            }

            return result;
        }
        public bool UpdateCustomUserAttributesOnly(CustomUser customUser)
        {
            bool result = false;

            //User oktaUser = GetOktaUserById(oktaId);
            string oktaId = customUser.Id;
            string apiEndPoint = _orgUrl + Constants.EndpointV1 + Constants.UsersEndpoint + "/" + oktaId;

            CustomAttributes customAttributes = new CustomAttributes();
            customAttributes.Profile = customUser.Profile;

            string serializedbody = JsonConvert.SerializeObject(customAttributes);

            try
            {

                // OktaHttpClient baseClient = new OktaHttpClient(_orgSettings);

                HttpResponseMessage response = _oktaClient.BaseClient.Post(apiEndPoint, serializedbody);
                string responseTopContent = response.ToString();
                var index = responseTopContent.IndexOf(",", 20);
                var rspStatus = responseTopContent.Substring(0, index);
                string content = response.Content.ReadAsStringAsync().Result;
                _logger.Debug("response " + rspStatus + " content " + content);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error updating Okta user. Okta username: {0}.", customUser.Profile.Login), ex);
            }

            return result;
        }


        public string GetUnmappedProperty(User user, string propertyName)
        {
            string result = null;

            try
            {
                result = user.Profile.GetProperty(propertyName);
            }
            catch
            {
                result = string.Empty;
            }

            return result;
        }

        public User AddBasicUser(User localUser)
        {
            _logger.Debug("AddBasicUser ");
            User oktaUser = null;
            try
            {
                //oktaUser = _usersClient.Add(user: localUser, activate: true, sendActivaionEmail: false);
                oktaUser = _usersClient.Add(user: localUser, activate: true);
            }
            catch (OktaException ex)
            {
                //log the conditions
                if (ex.ErrorSummary != null)
                {
                    //logger.Error(ex.ErrorSummary.ToString() + ":" + ex.HttpStatusCode.ToString());
                }
                if (ex.ErrorCauses != null)
                {
                    for (int tt = 0; tt < ex.ErrorCauses.Length; tt++)
                    {
                        _logger.Error(ex.ErrorCauses[tt].ErrorSummary.ToString());
                    }
                }
                _logger.Error(ex.ToString());
                //throw new Exception("failed to add user " + localUser.Profile.Login);
                _logger.Error("failed to add user " + oktaUser.Profile.Login);

            }

            return oktaUser;
        }
        public CustomUser AddCustomUser(CustomUser newCustomUser)
        {

            _logger.Debug("AddCustomUser email " + newCustomUser.Profile.Email);

            CustomUser rspCustomUser = null;

            //User addOktaUser = new User();
            //addOktaUser.Profile = newCustomUser.Profile;
            //sanity check on required attributes
            if (string.IsNullOrEmpty(newCustomUser.Profile.FirstName))
            {
                _logger.Error("Missing required field  firstname");
                return null;
            }
            if (string.IsNullOrEmpty(newCustomUser.Profile.LastName))
            {
                _logger.Error("Missing required field  LastName");
                return null;
            }
            if (string.IsNullOrEmpty(newCustomUser.Profile.Email))
            {
                _logger.Error("Missing required fields  login and email");
                return null;
            }
            //if (string.IsNullOrEmpty(newCustomUser.Profile.Email))
            //{
            //    _logger.Error("Missing required fields  login and email");
            //    return null;
            //}

            //if (!string.IsNullOrEmpty(activation_passCode))
            //{
            //    _logger.Debug("add activation passCode " + activation_passCode);
            //    newCustomUser.Profile.activation_passCode = activation_passCode;
            //}

            //if (!string.IsNullOrEmpty(activation_setDate))
            //{
            //    _logger.Debug("add activation_setDate " + activation_setDate);
            //    newCustomUser.Profile.activation_setDate = activation_setDate;
            //}

            try
            {

                User rspOktaUser = null;
                rspOktaUser = _usersClient.Add(newCustomUser, false);
                rspCustomUser = new CustomUser(rspOktaUser);

                List<string> customAttributes = rspOktaUser.Profile.GetUnmappedPropertyNames();
                foreach (var item in customAttributes)
                {

                    PropertyInfo tempProp = rspCustomUser.Profile.GetType().GetProperty(item);
                    if (tempProp != null)
                    {
                        object myValue = rspOktaUser.Profile.GetProperty(item);
                        if (tempProp.CanWrite)
                        {
                            tempProp.SetValue(rspCustomUser.Profile, myValue, null);
                        }
                    }
                    else
                    {
                        _logger.Debug("unmapped okta attribute " + item + " is not defined as an extention");
                    }

                }
            }
            catch (OktaException ex)
            {
                _logger.Error("Error Creating User " + newCustomUser.Profile.Email + " Code: " + ex.ErrorCode + " " + ex.ErrorSummary);
                return null;
            }//end catch

            return rspCustomUser;
        }



        public CustomUser SetSecQuestion(string oktaId, string secQuestion, string secQuestionAnswer)
        {
            User oktaUser = new User();
            User rspOktaUser = new User();

            LoginCredentials myLoginCredentials = new LoginCredentials();
            myLoginCredentials.RecoveryQuestion.Question = secQuestion;
            myLoginCredentials.RecoveryQuestion.Answer = secQuestionAnswer;
            oktaUser.Id = oktaId;

            rspOktaUser = _usersClient.SetCredentials(oktaUser, myLoginCredentials);

            CustomUser customUser = new CustomUser(oktaUser);

            _logger.Debug("set credentials for " + rspOktaUser.Profile.Login);

            return customUser;
        }



    }
}