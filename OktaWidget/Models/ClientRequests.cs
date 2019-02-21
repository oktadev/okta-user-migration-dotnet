using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OktaJS_SDK.Models
{
    public class OauthTokenRequest
    {
        public string grant_type { get; set; }
        public string code { get; set; }
        public string refresh_token { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string scope { get; set; }
        public string redirect_uri { get; set; }
        public string code_verifier { get; set; }
    }
}