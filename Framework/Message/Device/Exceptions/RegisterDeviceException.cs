using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCMonitor.Framework.Framework.Message.Device.Exceptions {
    class RegisterDeviceException : MessageException {
        public RegisterDeviceException() : base("Please register the device first!") { }
        public RegisterDeviceException(string message) : base(message) { }
    }
}
