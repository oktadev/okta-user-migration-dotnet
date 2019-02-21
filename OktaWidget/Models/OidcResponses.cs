using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OktaJS_SDK.Models
{


    public class OidcAccessToken
    {
        public int ver { get; set; }
        public string jti { get; set; }
        public string iss { get; set; }
        public string aud { get; set; }
        public string sub { get; set; }
        public int iat { get; set; }
        public int exp { get; set; }
        public string cid { get; set; }
        public string uid { get; set; }
        public string[] scp { get; set; }
    }





    public class OidcGetKeys
    {
        public List<Key> keys { get; set; }
    }

    public class Key
    {
        public string alg { get; set; }
        public string e { get; set; }
        public string n { get; set; }
        public string kid { get; set; }
        public string kty { get; set; }
        public string use { get; set; }
    }

    public class OidcHeader
    {
        public string alg { get; set; }
        public string kid { get; set; }
    }



    public class OidcIdTokenMin
    {
        public string sub { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public int ver { get; set; }
        public string iss { get; set; }
        public string aud { get; set; }
        public int iat { get; set; }
        public int exp { get; set; }
        public string jti { get; set; }
        public string[] amr { get; set; }
        public string idp { get; set; }
        public string preferred_username { get; set; }
        public int auth_time { get; set; }
        public string at_hash { get; set; }
    }

    public class OidcIdToken
    {
        public string sub { get; set; }
        public string name { get; set; }
        public string locale { get; set; }
        public string email { get; set; }
        public int ver { get; set; }
        public string iss { get; set; }
        public string aud { get; set; }
        public int iat { get; set; }
        public int exp { get; set; }
        public string jti { get; set; }
        public string[] amr { get; set; }
        public string idp { get; set; }
        public string preferred_username { get; set; }
        public string given_name { get; set; }
        public string middle_name { get; set; }
        public string family_name { get; set; }
        public string zoneinfo { get; set; }
        public int updated_at { get; set; }
        public bool email_verified { get; set; }
        public string phone_number { get; set; }
        public int auth_time { get; set; }
        public string locationUrl { get; set; }
        public Address1 address { get; set; }
    }

    public class Address1
    {
        public string street_address { get; set; }
        public string locality { get; set; }
        public string region { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
    }


}