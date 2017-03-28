using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QCMonitor.Framework;
using System.Data.SqlClient;
using QCMonitor.Framework.DBService.SqlClient;
using QCMonitor.Framework.DBService;
using QCMonitor.Framework.Framework.DBService.SqlClient;

namespace QCMonitor.Framework.Repository {
    public class BlobStore {
        public static BlobStore Store {
            get { return new BlobStore(QCDBServiceFactory.Instance.DefaultDBClient); }
        }

        protected QCDBClient DB;

        protected virtual String TableName {
            get {
                return "BinaryFileStore";
            }
        }

        protected virtual ColumnDefinition ValueDefs {
            get {
                return new ColumnDefinition("key","varchar(200) PRIMARY KEY")
                    .AddDefinition(new ColumnDefinition("memo", "nvarChar(3000)"))
                    .AddDefinition(new ColumnDefinition("blob_value", "varBinary(MAX))"));
            }
        }

        protected BlobStore(QCDBClient db) {
            DB = db;

            DB.CreateTable(TableName, ValueDefs);
        }

        public String StoreBlob(String memo, byte[] blob) {
            var key = Guid.NewGuid().ToString();
            var cmd = String.Format(@"INSERT INTO {0} ([key], memo, blob_value) values( {1}, {2}, @blob)",
                                    TableName,
                                    QCDBClient.EscapeString(key),
                                    QCDBClient.EscapeString(memo)
                                    );
            var par = new SqlParameter("blob", blob);
            var pars = new List<SqlParameter>(); pars.Add(par);

            DB.ExecuteNonQuery(cmd,pars);

            return key;
        }

        public byte[] FetchBlob(String key) {
            var row = DB.ExecuteFirstRow("SELECT blob_value from " + TableName + " WHERE [key] = " + QCDBClient.EscapeString(key));
            return (byte[])row[0];
        }
    }
}
