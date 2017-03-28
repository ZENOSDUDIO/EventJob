using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QCMonitor.Framework.DBService.SqlClient;
using QCMonitor.Framework.DBService;
using QCMonitor.Framework.Framework.DBService.SqlClient;

namespace QCMonitor.Framework.Util {
    public class QCDBLogger : AbstractQCLogger, ITableInit {
        protected String TableName {
            get {
                return "Log_" + Namespace;
            }
        }

        protected QCDBClient DB = QCDBServiceFactory.Instance.DefaultDBClient;

        // arguments:
        //  ns: the namespace of the logger
        public QCDBLogger(String ns) : base(ns) {

            InitTable();

        }

        public void InitTable() {
            DB.CreateTable(TableName, new ColumnDefinition("level", "nvarchar(250)")
                .AddDefinition(new ColumnDefinition("time", "datetime"))
                .AddDefinition(new ColumnDefinition("message", "nvarchar(4000)"))
                );
        }

        public override void AppendLog(String level, String message) {
            String cmd = String.Format(@"INSERT INTO {0} VALUES ({1}, {2}, {3})", 
                                           TableName, 
                                           QCDBClient.EscapeString(level),
                                           QCDBClient.EscapeString(DateTime.Now.ToString()), 
                                           QCDBClient.EscapeString(message.Substring(0, Math.Min(2000, message.Length))));
            DB.ExecuteNonQuery(cmd);
            System.Diagnostics.Debug.Print(DateTime.Now.ToShortTimeString() + @" [" + level + "]" + ": " + message);
        }

        public override void Flush() {
        }
    }
}
