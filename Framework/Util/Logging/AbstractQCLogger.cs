using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCMonitor.Framework.Util {
    public abstract class AbstractQCLogger {
        public const string Error_Flag = "Rrror";
        public const string Info_Flag = "Info";
        public const string Debug_Flag = "Debug";
        public const string Warn_Flag = "Warn";

        public string Namespace = null;

        public AbstractQCLogger(string ns) {
            Namespace = ns;
        }

        public void LogException(Exception e) {
            Error("Exception: " + e.ToString());
            Error("Exception Message: " + e.Message);
            Error("Exception Trace: " + e.StackTrace);

            if (e.InnerException != null) {
                Error("InnerException: " + e.InnerException.ToString());
                Error("InnerException Message: " + e.InnerException.Message);
                Error("InnerException Trace: " + e.InnerException.StackTrace);
            }
            
            Flush();
        }

        public void Info(String message) {
            AppendLog(Info_Flag, message);
        }
         
        public void Debug(String message) {
            AppendLog(Debug_Flag, message);
        }

        public void Warn(String message) {
            AppendLog(Warn_Flag, message);
        }

        public void Error(String message) {
            AppendLog(Error_Flag, message);
            Flush();
        }

        public abstract void AppendLog(string level, string message);

        public abstract void Flush();
    }
}
