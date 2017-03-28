using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCMonitor.Framework.Framework.Message.Device {
    public interface IEmailDevice {
        void InitDevice();
        void SendEmail(string emailAddress, string subject, string content);
        void StopDevice();
    }
}





