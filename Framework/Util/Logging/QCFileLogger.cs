using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace QCMonitor.Framework.Util {
    public class QCFileLogger : AbstractQCLogger {

        public const string LogRootDir = @"C:\QCMonitorLogs\";
        public const string LogRootDirFirst = @"D:\QCMonitorLogs\";

        protected StreamWriter LogWriter = null;
        protected StreamWriter ErrorWriterStub = null;

        protected String LogFilePath;
        protected String FileName;

        private Int64 FlushCounter = 0;

        protected string LoggerDirName {
            get {
                return "Log_" + Namespace;
            }
        }

        protected StreamWriter ErrorWriter {
            get {
                if (ErrorWriterStub == null) {
                    ErrorWriterStub = new StreamWriter(Path.Combine(LogFilePath, FileName + ".error"));
                }
                return ErrorWriterStub;
            }
        }

        public QCFileLogger(string ns) : base(ns) {
            string path = @"D:\";
            if (Directory.Exists(path)) {
                LogFilePath = Path.Combine(LogRootDirFirst, LoggerDirName);
            } else {
                LogFilePath = Path.Combine(LogRootDir, LoggerDirName);
            }

            //LogFilePath = Path.Combine(LogRootDir, LoggerDirName);

            Directory.CreateDirectory(LogFilePath);

            FileName = DateTime.Now.ToString().Replace("/", "-").Replace(":", "_").Replace(" ", "_");

            LogWriter = new StreamWriter(Path.Combine(LogFilePath, FileName));
        }

        public override void Flush() {
            lock (FileLock) {
                LogWriter.Flush();
            }
        }

        public static object FileLock = new object();

        public override void AppendLog(string level, string message) {
            lock (FileLock) {
                LogWriter.WriteLine(DateTime.Now.ToString() + " [" + level + "]" + message);

                if (FlushCounter++ % 5 == 0) {
                    Flush();
                }

                if (level.Equals(Error_Flag)) {
                    ErrorWriter.WriteLine(DateTime.Now.ToString() + " [" + Error_Flag + "]" + message);
                    ErrorWriter.Flush();
                }
            }
        }
    }
}
