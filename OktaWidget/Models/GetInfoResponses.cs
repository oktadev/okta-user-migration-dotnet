using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OktaJS_SDK.Models
{
    public class GetInfoResponse
    {
        public string sub { get; set; }
        public string name { get; set; }
        public string nickname { get; set; }
        public string given_name { get; set; }
        public string middle_name { get; set; }
        public string family_name { get; set; }
        public string profile { get; set; }
        public string zoneinfo { get; set; }
        public string locale { get; set; }
        public int updated_at { get; set; }
        public string email { get; set; }
        public bool email_verified { get; set; }
        public Address address { get; set; }
        public string phone_number { get; set; }
        public string preferred_username { get; set; }

        public string error { get; set; }
        public string error_description { get; set; }
        public string errorCode { get; set; }
        public string errorSummary { get; set; }
        public List<errorCauses> errorCauses { get; set; }
    }

    public class Address
    {
        public string street_address { get; set; }
        public string locality { get; set; }
        public string region { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
    }
}