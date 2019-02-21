using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OktaJS_SDK.Models
{
    public class PasswordMigration
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class PswMigrationResponse
    {
        public string isPasswordInOkta { get; set; }
        public string oktaId { get; set; }
        public string login { get; set; }
        public string status { get; set; }
    }


    public class PswMigrationUpdateObject
    {
        public PswMigrationProfile profile { get; set; }
    }

    public class PswMigrationProfile
    {
        public string isPasswordInOkta { get; set; }
    }



    public class LdapServiceModel
    {
        public string ldapServer { get; set; }
        public string ldapPort { get; set; }
        public string baseDn { get; set; }
    }
}
