using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Okta.Core.Models;

namespace OktaJS_SDK.Models
{
    public class CustomUser : User
    {

        public CustomUser(User oktaBase)
        {
            Profile = new CustomUserProfile();
            //Credentials = new LoginCredentials();
            this.Profile.Email = oktaBase.Profile.Email;
            this.Profile.FirstName = oktaBase.Profile.FirstName;
            this.Profile.LastName = oktaBase.Profile.LastName;
            this.Profile.Login = oktaBase.Profile.Login;
            this.Profile.MobilePhone = oktaBase.Profile.MobilePhone;
            this.Profile.SecondaryEmail = oktaBase.Profile.SecondaryEmail;
            this.Id = oktaBase.Id;
            this.Links = oktaBase.Links;
            this.Activated = oktaBase.Activated;
            this.Created = oktaBase.Created;
            this.Credentials = oktaBase.Credentials;
            this.LastLogin = oktaBase.LastLogin;
            this.LastUpdated = oktaBase.LastUpdated;
            this.PasswordChanged = oktaBase.PasswordChanged;
            this.recoveryQuestion = oktaBase.recoveryQuestion;
            this.Status = oktaBase.Status;
            this.StatusChanged = oktaBase.StatusChanged;
            this.TransitioningToStatus = oktaBase.TransitioningToStatus;
        }

        [JsonProperty("profile")]
        public new CustomUserProfile Profile { get; set; }

    }


    public class CustomAttributes
    {
        [JsonProperty("profile")]
        public CustomUserProfile Profile { get; set; }
    }


    public class CustomUserProfile : UserProfile
    {
        [JsonProperty("streetAddress")]
        public string streetAddress { get; set; }
        [JsonProperty("city")]
        public string city { get; set; }
        [JsonProperty("state")]
        public string state { get; set; }
        [JsonProperty("zipCode")]
        public string zipCode { get; set; }
        [JsonProperty("primaryPhone")]
        public string primaryPhone { get; set; }
        [JsonProperty("isPasswordInOkta")]
        public string IsPasswordInOkta { get; set; }

    }


   

}