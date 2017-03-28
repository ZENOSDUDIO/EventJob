using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCMonitor.Framework.Util.Logging {
    public class QCLoggerFactory {
        static AbstractQCLogger Logger;

        public static void RegisterLogger(AbstractQCLogger logger) {
            if (Logger != null) {
                throw new NotImplementedException("Logger has been inited already");
            }
            Logger = logger;
        }

        public static AbstractQCLogger DefaultLogger {
            get {
                return Logger;
            }
        }

    }
}
