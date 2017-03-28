using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using QCMonitor.Framework.DBService.SqlClient;
using QCMonitor.Framework.DBService;
using QCMonitor.Framework.Framework.DBService.SqlClient;

namespace QCMonitor.Framework {

    // don't delete this class
    public class QCFrameworkDummyObject {
    }

    public abstract class QCOptions : ITableInit {
        public string Namespace;
        
        protected String TableName {
            get {
                return "Options_" + Namespace;
            }
        }

        protected QCDBClient DB = QCDBServiceFactory.Instance.DefaultDBClient;

        private QCOptions() {
        }

        public void InitTable() {
            DB.CreateTable(TableName, new ColumnDefinition("name", "nvarchar(250)")
                            .AddDefinition(new ColumnDefinition("value", "nvarchar(250)")));
        }
  
        protected QCOptions(String ns) {
            Namespace = ns;
            // table schema
            // key, value
            // init

            InitTable();
        }

        // basic methods
        // get
        public String GetString(String key) {
            String value = null;
            using (SqlConnection sqlConnection = DB.NewSqlConnection()) {
                String cmdText = @"SELECT value FROM " + TableName + " WHERE name = " + QCDBClient.EscapeString(key);
                SqlCommand sqlCommand = new SqlCommand(cmdText, sqlConnection);
                sqlConnection.Open();
                Object r = sqlCommand.ExecuteScalar();
                if ( r != null) {
                    value = r.ToString();
                }
                sqlConnection.Close();
            }
            return value;
        }

        public void SaveOrUpdateString(String key, String value)
        {
            if (GetString(key) == null)
                AddString(key, value);
            else
                UpdateString(key, value);
        }

        // update
        public void UpdateString(String key, String value) {
            String cmd = String.Format(@"UPDATE {0} SET value = {1} WHERE name = {2}", TableName, QCDBClient.EscapeString(value),
                                                                                                 QCDBClient.EscapeString(key));
            DB.ExecuteNonQuery(cmd);
        }

        // set
        public void AddString(String key, String value) {
            String cmd = String.Format(@"INSERT INTO {0} VALUES ({2}, {1})", TableName, QCDBClient.EscapeString(value),
                                                                                                 QCDBClient.EscapeString(key));
            DB.ExecuteNonQuery(cmd);
        }

        public String GetString(String key, String defaultValue) {
            String value = GetString(key);
            if (value == null) {
                AddString(key, defaultValue);
                value = defaultValue;
            }
            return value;
        }
        // must be thread safe
        public int GetInt(String key, int defaultValue) {
            String strValue = GetString(key, "" + defaultValue);
            return Int32.Parse(strValue);
        }

        public void UpdateInt(String key, int value) {
            UpdateString(key, "" + value);
        }

        private Object QuickIncrLock = new Object();
        // must be thread safe, could be changed by more than one thread
        public void QuickIncrementInt(String key) {
            int value = GetInt(key, 0);
            UpdateInt(key, value + 1);
        }
    }
}
