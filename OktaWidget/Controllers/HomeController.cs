using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.IO;
using System.Web.UI;
using System.Configuration;
using Newtonsoft;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using log4net;
using System.Collections.Specialized;
using OktaJS_SDK.Models;

namespace OktaJS_SDK.Controllers
{
    public class HomeController : Controller
    {

        ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        NameValueCollection appSettings = ConfigurationManager.AppSettings;
        // Org settings for primary Org
        private static string primaryOrgUrl = ConfigurationManager.AppSettings["okta.ApiUrl"];
        private static string primaryOrgApiToken = ConfigurationManager.AppSettings["okta.ApiToken"];

        public ActionResult Index()
        {
            //duplicated from below
            ViewBag.Message = "Okta Login Widget page.";

            TempData["clientId"] = appSettings["oidc.spintweb.clientId"];
            TempData["redirectUri"] = appSettings["oidc.spintweb.RedirectUri_AuthCode"];
            TempData["oktaOrg"] = appSettings["okta.ApiUrl"];
            return View("oidcPswMigrationLogin");
        }

        [HttpGet]
        public ActionResult Help()
        {
            return View("LoginHelp");
        }



    }
}