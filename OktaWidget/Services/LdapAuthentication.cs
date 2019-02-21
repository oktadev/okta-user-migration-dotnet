
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Web;
using OktaJS_SDK.Models;
using OktaJS_SDK.Services;
using Okta.Core;
using Okta.Core.Clients;
using Okta.Core.Models;

namespace OktaJS_SDK.Services
{
 
    public class LdapAuthentication : ICredAuthentication
    {
        
        NameValueCollection appSettings = ConfigurationManager.AppSettings;

        // LDAP settings
        private string _ldapServer = null;
        private string _ldapPort = null;
        private string _baseDn = null;

        private const int LDAPError_InvalidCredentials = 0x31;
        private string _ldapPath = null;

        public LdapAuthentication()
        {

        }

        public bool CheckCredentialServer(LdapServiceModel ldapServiceModel)
        {
            bool success = false;
            _ldapServer = ldapServiceModel.ldapServer;
            _ldapPort = ldapServiceModel.ldapPort;
            LdapDirectoryIdentifier ldapDirectoryIdentifier = new LdapDirectoryIdentifier(_ldapServer, Convert.ToInt16(_ldapPort), true, false);
            LdapConnection ldapConnection = new LdapConnection(ldapDirectoryIdentifier);

            try
            {
                ldapConnection.AuthType = AuthType.Anonymous;
                ldapConnection.AutoBind = false;
                ldapConnection.Timeout = new TimeSpan(0, 0, 0, 1);
                ldapConnection.Bind();
                ldapConnection.Dispose();
                success = true;
            }
            catch (LdapException)
            {
                success = false;
            }

            return success;
        }


        public bool IsAuthenticated(string username, string password, LdapServiceModel ldapServiceModel)
        {
            _ldapServer = ldapServiceModel.ldapServer;
            _ldapPort = ldapServiceModel.ldapPort;
            _baseDn = ldapServiceModel.baseDn;

            _ldapPath = string.Format("LDAP://{0}:{1}", _ldapServer, _ldapPort);

            bool success = false;
            //username must be user full domain name. build UID string
            string modUsername = "uid=" + username + _baseDn;

            try
            {
                LdapDirectoryIdentifier ldapDirectoryIdentifier = new LdapDirectoryIdentifier(_ldapServer, Convert.ToInt16(_ldapPort), true, false);
                LdapConnection ldapConnection = new LdapConnection(ldapDirectoryIdentifier);
                //LdapConnection ldapConnection = new LdapConnection(new LdapDirectoryIdentifier((_ldapServer + (":" + _ldapPort))));

                //ldapConnection.SessionOptions.VerifyServerCertificate = new VerifyServerCertificateCallback((con, cer) => true);
                //ldapConnection.SessionOptions.SecureSocketLayer = true;
                ldapConnection.SessionOptions.ProtocolVersion = 3;
                ldapConnection.AuthType = AuthType.Basic;
                //ldapConnection.AuthType = AuthType.Negotiate;;
                ldapConnection.Timeout = new TimeSpan(0, 0, 10);
                ldapConnection.Credential = new NetworkCredential(modUsername, password);
                ldapConnection.Bind();
                //if bind does not cause error then it is successful
                ldapConnection.Dispose();
                //_logger.Debug("ldap successfully validated username " + username);
                success = true;
            }
            catch (LdapException ldapException)
            {
                //add additional error handling
                if (ldapException.ErrorCode.Equals(LDAPError_InvalidCredentials))
                { success = false; }

                success = false;
            }
            catch (Exception ex)
            {
                //add additional error handling
                success = false;
            }

            return success;
        }



        public CustomUser IsCreated(string username, string password, LdapServiceModel ldapServiceModel)
        {
            _ldapServer = ldapServiceModel.ldapServer;
            _ldapPort = ldapServiceModel.ldapPort;
            _baseDn = ldapServiceModel.baseDn;

            bool success = false;
            CustomUser customUser = null;

            //username must be user full domain name. build UID string
            string modUsername = "uid=" + username + _baseDn;

            //string tempPath = "LDAP://ldap.server.com:389/uid=admin,o=Users,dc=okta,dc=com";
            _ldapPath = string.Format("LDAP://{0}:{1}", _ldapServer, _ldapPort);
  
            DirectoryEntry entry = new DirectoryEntry(_ldapPath, modUsername, password);
            entry.AuthenticationType = AuthenticationTypes.None;
            try
            {
                // Bind to the native AdsObject to force authentication.
                // if no exception, then login successful
                Object connected = entry.NativeObject;

                DirectorySearcher directorySearcher = new DirectorySearcher(entry);
                directorySearcher.Filter = "(uid=" + username + ")";
                SearchResult result = directorySearcher.FindOne();
                if (result != null)
                {
                    User oktaUser = new User();
                    customUser = new CustomUser(oktaUser);

                    string path = result.Path;
                    customUser.Profile.LastName = (String)result.Properties["sn"][0];
                    customUser.Profile.FirstName = (String)result.Properties["givenname"][0];
                    customUser.Profile.Email = (String)result.Properties["mail"][0];
                    
                    //success = true;
                }


            }
            catch (LdapException ldapException)
            {
                //add additional error handling
                if (ldapException.ErrorCode.Equals(LDAPError_InvalidCredentials))
                { success = false; }

                success = false;
            }
            catch (Exception ex)
            {
                //add additional error handling
                success = false;
            }

            return customUser;
        }


    }
}