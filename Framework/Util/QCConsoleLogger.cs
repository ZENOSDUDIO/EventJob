using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCMonitor.Framework.Util {
    public class QCConsoleLogger : AbstractQCLogger {

        public QCConsoleLogger(string ns) : base(ns) {
        }

        public override void AppendLog(string level, string message) {
            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString() + " [" + level + "]" + message);
        }

        public override void Flush() {
        }
    }
}
