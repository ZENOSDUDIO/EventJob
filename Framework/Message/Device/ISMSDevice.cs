using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QCMonitor.Framework.Framework.Message.Device.Listener;

namespace QCMonitor.Framework.Framework.Message.Device {
    public interface ISMSDevice {
        void InitDevice(ISMSListener listener);
        void SendMessage(string phoneNo, string content, string trackingId);
        void StopDevice();
    }
}
