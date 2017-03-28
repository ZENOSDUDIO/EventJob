using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using QCMonitor.Framework.DBService.SqlClient;
using QCMonitor.Framework.DBService;
using System.IO;
using QCMonitor.Framework.Util;

namespace QCMonitor.Framework {
    public class QCLogger : AbstractQCLogger {

        private IList<AbstractQCLogger> Loggers = new List<AbstractQCLogger>();

        public QCLogger(string ns) : base(ns) {
            Loggers.Add(new QCDBLogger(ns));
            Loggers.Add(new QCFileLogger(ns));

            //QCFileLogger has bug. Cannot work at multithread.
        }

        public override void AppendLog(string level, string message) {
            foreach (var logger in Loggers) {
                logger.AppendLog(level, message);
            }
        }

        public override void Flush() {
            foreach (var logger in Loggers) {
                logger.Flush();
            }
        }
    }
}
