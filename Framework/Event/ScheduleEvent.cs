using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using QCMonitor.Framework.DBService.SqlClient;
using QCMonitor.Framework.DBService;
using QCMonitor.Framework.Framework.DBService.SqlClient;

namespace QCMonitor.Framework.Event {
    [Serializable]
    public class ScheduleEvent : QCEvent {

    }

    [InitTable]
    public class ScheduleEventSource : QCEventSource<QCEvent>, ITableInit {
        public const String Name = "ScheduleEvent";
        protected const string Status_Dead = "d";
        protected const string Status_Live = "l";

        // notice there's a static method at the end of this file
        protected QCDBClient DB = QCDBServiceFactory.Instance.DefaultDBClient;
        protected static QCDBClient SDB = QCDBServiceFactory.Instance.DefaultDBClient;

        public virtual String TableName {
            get {
                return Name;
            }
        }

        public virtual ColumnDefinition ValueDefs {
            get {
                return new ColumnDefinition("row_id", "int identity(1,1) PRIMARY KEY")
                .AddDefinition(new ColumnDefinition("fire_time", "datetime"))
                .AddDefinition(new ColumnDefinition("status", "char(1)"))
                .AddDefinition(new ColumnDefinition("event_xml", "nvarchar(max)"));
            }
        }

        public void InitTable() {
            DB.CreateTable(TableName, ValueDefs);
            //DB.CreateIndex(TableName, "fire_time");
            DB.CreateIndex(TableName, "status");
        }

        public ScheduleEventSource() {
            InitTable();
        }

        public override bool HasNextEvent() {
            DateTime now = DateTime.Now; 
            String cmdText = String.Format(@"Select TOP 1 row_id FROM {0} WHERE status = {1} AND fire_time < {2} ", 
                                            TableName, QCDBClient.EscapeString(Status_Live),
                                            QCDBClient.EscapeString(now.ToString()));

            return DB.ExecuteScalar(cmdText) != null;
        }

        public override QCEvent NextEventImpl() {
            String fetchEventText = String.Format(@"SELECT TOP 1 * FROM {0} WHERE status = {1} AND fire_time < {2} ",
                                                  TableName,
                                                  QCDBClient.EscapeString(Status_Live),
                                                  QCDBClient.EscapeString(DateTime.Now.ToString()));
            Object[] results = DB.ExecuteFirstRow(fetchEventText);
            String rowIdText = results[0].ToString();
            String fireTime = results[1].ToString();
            String status = results[2].ToString();
            String eventText = results[3].ToString();
            String updateEventText = String.Format(@"UPDATE {0} SET status = {1} WHERE row_id = {2}",
                                            TableName,
                                            QCDBClient.EscapeString(Status_Dead),
                                            // rowId is integer, don't need to be escaped
                                            rowIdText);
            DB.ExecuteNonQuery(updateEventText);

            // de serelize 
            return QCEvent.FromSoapString(eventText);
        }

        protected static Object InsertLock = new Object();

        protected static void InsertEventInTable(DateTime fireTime, QCEvent e, String tableName) {
            lock (InsertLock) {
                String eventXML = e.ToSoapString();
                String cmdText = String.Format(@"INSERT INTO {0} (fire_time, status, event_xml) VALUES ({1}, {2}, {3})",
                                               tableName,
                                               QCDBClient.EscapeString(fireTime.ToString()),
                                               QCDBClient.EscapeString(Status_Live),
                                               QCDBClient.EscapeString(eventXML));
                SDB.ExecuteNonQuery(cmdText);
            }
        }

        /// <summary>
        /// Insert a new Event into the schedule queue.
        /// Static method, need be rewrite to work.
        /// </summary>
        /// <param name="fireTime">The time want it to be happen.</param>
        /// <param name="e">The event to schedule, it must be [Seriliable]</param>
        public static void InsertEvent(DateTime fireTime, QCEvent e) {
            InsertEventInTable(fireTime, e, Name);
        }

        public static void ClearEvents() {
            SDB.ExecuteNonQuery(Name);
        }
    }
}
