using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OktaJS_SDK.Models
{
    public class IntrospectionResponse
    {
        public string active { get; set; }
        public string scope { get; set; }
        public string username { get; set; }
        public string exp { get; set; }
        public string iat { get; set; }
        public string sub { get; set; }
        public string aud { get; set; }
        public string iss { get; set; }
        public string jti { get; set; }
        public string nbf { get; set; }
        public string token_type { get; set; }
        public string client_id { get; set; }
        public string device_id { get; set; }
        public string error { get; set; }
        public string error_description { get; set; }
    }


    public class ClientResponses
    {
        public string recoveryToken { get; set; }
        public string stateToken { get; set; }

        public string error { get; set; }
        public string error_description { get; set; }
        public string errorCode { get; set; }
        public string errorSummary { get; set; }
        public List<errorCauses> errorCauses { get; set; }
    }

    public class errorCauses
    {
        public string errorSummary { get; set; }
    }

    public class TokenRequestResponse
    {
        public string error { get; set; }
        public string error_description { get; set; }
        public string errorCode { get; set; }
        public string errorSummary { get; set; }
        public List<errorCauses> errorCauses { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string scope { get; set; }
        public string id_token { get; set; }
        public string refresh_token { get; set; }
    }



    public class UserResponse
    {
        public string status { get; set; }
        public DateTime created { get; set; }
        public DateTime activated { get; set; }
        public DateTime statusChanged { get; set; }
        public DateTime lastLogin { get; set; }
        public DateTime lastUpdated { get; set; }
        public DateTime passwordChanged { get; set; }
        public Profile profile { get; set; }
        public Credentials credentials { get; set; }
        public _Links _links { get; set; }
    }

    public class Profile
    {
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string login { get; set; }
        public object mobilePhone { get; set; }
        public object secondEmail { get; set; }
    }

    public class Credentials
    {
        public Password password { get; set; }
        public Recovery_Question recovery_question { get; set; }
        public Provider provider { get; set; }
    }

    public class Password
    {
    }

    public class Recovery_Question
    {
        public string question { get; set; }
    }

    public class Provider
    {
        public string type { get; set; }
        public string name { get; set; }
    }

    public class _Links
    {
        public Suspend suspend { get; set; }
        public Resetpassword resetPassword { get; set; }
        public Expirepassword expirePassword { get; set; }
        public Forgotpassword forgotPassword { get; set; }
        public Self self { get; set; }
        public Changerecoveryquestion changeRecoveryQuestion { get; set; }
        public Deactivate deactivate { get; set; }
        public Changepassword changePassword { get; set; }
        public Refresh refresh { get; set; }
        public SessionUser user { get; set; }
    }


    public class Suspend
    {
        public string href { get; set; }
        public string method { get; set; }
    }

    public class Resetpassword
    {
        public string href { get; set; }
        public string method { get; set; }
    }

    public class Expirepassword
    {
        public string href { get; set; }
        public string method { get; set; }
    }

    public class Forgotpassword
    {
        public string href { get; set; }
        public string method { get; set; }
    }

    //public class Self
    //{
    //public string href { get; set; }
    //}

    public class Changerecoveryquestion
    {
        public string href { get; set; }
        public string method { get; set; }
    }

    public class Deactivate
    {
        public string href { get; set; }
        public string method { get; set; }
    }

    public class Changepassword
    {
        public string href { get; set; }
        public string method { get; set; }
    }


    //public class ValidateSessionRsp
    //{
    //    public string id { get; set; }
    //    public string userId { get; set; }
    //    public string login { get; set; }
    //    public DateTime expiresAt { get; set; }
    //    public string status { get; set; }
    //    public DateTime lastPasswordVerification { get; set; }
    //    public object lastFactorVerification { get; set; }
    //    public string[] amr { get; set; }
    //    public Idp idp { get; set; }
    //    public bool mfaActive { get; set; }
    //    public _Links _links { get; set; }
    //}

    public class ValidateSessionRsp
    {
        public string id { get; set; }
        public string userId { get; set; }
        public string login { get; set; }

        public string status { get; set; }

        public string[] amr { get; set; }

        public bool mfaActive { get; set; }

    }


    public class Idp
    {
        public string id { get; set; }
        public string type { get; set; }
    }


    public class Self
    {
        public string href { get; set; }
        public Hints hints { get; set; }
    }

    public class Hints
    {
        public string[] allow { get; set; }
    }

    public class Refresh
    {
        public string href { get; set; }
        public Hints1 hints { get; set; }
    }

    public class Hints1
    {
        public string[] allow { get; set; }
    }

    public class SessionUser
    {
        public string name { get; set; }
        public string href { get; set; }
        public Hints2 hints { get; set; }
    }

    public class Hints2
    {
        public string[] allow { get; set; }
    }
}