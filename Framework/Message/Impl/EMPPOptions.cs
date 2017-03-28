using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QCMonitor.Framework;
using QCMonitor.Framework.Framework.DBService.SqlClient;

namespace QCMonitor.Framework.Framework.Message.Impl
{
    [InitTable]
    class EMPPOptions : QCOptions
    {
        public EMPPOptions() : base("EMPP") { }

        public int Delay
        {
            get { return GetInt("HostPort", 1000); }
        }

        public int ResendTimeOut
        {
            get { return GetInt("ResendTimeOut", 10000) ; }
        }

        public string Account
        {
            get { return GetString("AccountId", "10657001024543"); }
        }


        public string HostIP
        {
            get { return GetString("HostIP", "211.136.163.68"); }
        }

        public int Port
        {
            get { return GetInt("HostPort", 9981); }
        }

        public string PassWord
        {
            get { return GetString("Password", "Link0903"); }
        }

        public string ServiceID
        {
            get { return GetString("ServiceId", "10657001024543"); }
        }
    }
   
}
