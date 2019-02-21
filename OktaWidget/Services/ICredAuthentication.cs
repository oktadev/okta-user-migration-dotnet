using OktaJS_SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OktaJS_SDK.Services
{
    public interface ICredAuthentication
    {
        bool CheckCredentialServer(LdapServiceModel ldapServiceModel);
        //bool IsAuthenticated(string username, string password);

        bool IsAuthenticated(string username, string password, LdapServiceModel ldapServiceModel);
        CustomUser IsCreated(string username, string password, LdapServiceModel ldapServiceModel);
    }
}
