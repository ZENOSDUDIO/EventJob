using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QCMonitor.Framework.Framework.Message.Device.Exceptions;

namespace QCMonitor.Framework.Framework.Message.Device.Factory {
    public class EmailDeviceFactory {
        protected static IEmailDevice Device;

        public static void RegisterEmailDevice(IEmailDevice device) {
            Device = device;
        }

        public static IEmailDevice GetDefaultEmailDevice() {
            if (Device == null){
                throw new RegisterDeviceException("Please register EmailDevice First!");
            }
            else return Device;
        }
    }
}




