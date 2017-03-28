using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCMonitor.Framework.Framework.Message.Device.Exceptions {
    public class SendMessageException : MessageException {
        public SendMessageException(string message) : base(message) { }
    }
}
