using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCMonitor.Framework.Framework.Message.Device.Exceptions {
    public class InitDeviceException : MessageException {
        public InitDeviceException() : base("Init Device Error") { }
        public InitDeviceException(string message) : base(message) { }
    }
}
