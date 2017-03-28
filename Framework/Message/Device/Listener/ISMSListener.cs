using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCMonitor.Framework.Framework.Message.Device.Listener {
    public interface ISMSListener {
        void SendMessageFailed(string trackingId);
        void ReceiveMessage(string fromPhoneNo, string content, DateTime receiveTime);
    }
}
