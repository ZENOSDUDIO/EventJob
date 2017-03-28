using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Collections;
using QCMonitor.Framework.Framework.DBService.SqlClient;

namespace QCMonitor.Framework.DBService.SqlClient {
    public class QCDBClient {

        

        public string GetSqlConnectionString() {
            return SqlConnectionString;
        }

        private string SqlConnectionString;

        public QCDBClient(String connectionString) {
            SqlConnectionString = connectionString;
        }

        private static Object lockThis = new Object();

        public SqlConnection NewSqlConnection() {
            lock (lockThis) {
                return new SqlConnection(SqlConnectionString);
            }
        }

        public static String EscapeString(string str) {
            return "'" + str.Replace("'", "''") + "'";
        }

        // valueDefs : (name char(25), value char(20)
        public void CreateTable(String tableName, ColumnDefinition valuesDef) {
            var valuesDefString = valuesDef.ToSQLString();

            String cmd = String.Format(@"IF NOT EXISTS
                                           (  SELECT [name] 
                                              FROM sys.tables
                                              WHERE [name] = {1} 
                                           )
                                         CREATE TABLE {0} ({2});", tableName, QCDBClient.EscapeString(tableName), valuesDefString);

            ExecuteNonQuery(cmd);
        }

        public bool DbTableExists(string strTableNameAndSchema) {
            string strCheckTable =
               String.Format(
                  "IF OBJECT_ID('{0}', 'U') IS NOT NULL SELECT 'true' ELSE SELECT 'false'",
                  strTableNameAndSchema);
            return Convert.ToBoolean(ExecuteScalar(strCheckTable));
        }

        public void CreateIndex(String tableName, string columnName) {

            String indexName = tableName + "_" + columnName + "_index";
            String cmd = String.Format(@"IF NOT EXISTS 
                                            (   SELECT [name] FROM sysindexes 
                                            WHERE [name] = '{2}'
                                            )
                                         CREATE INDEX {2}
                                            ON {0} ({1});", tableName, columnName, indexName);

            ExecuteNonQuery(cmd);
        }

        public void DropTable(String tableName) {
            String cmdText = String.Format(@"IF  EXISTS 
                                                (SELECT * FROM sys.objects 
                                                    WHERE object_id = OBJECT_ID(N'{0}') 
                                                AND type in (N'U'))
                                            DROP TABLE {0}", tableName);
            ExecuteNonQuery(cmdText);
        }

        public void DeleteFromTable(String tableName) {
            if (tableName != null && !tableName.Equals("")) {
                String cmdText = "delete from " + tableName;
                ExecuteNonQuery(cmdText);
            }
        }

        public int ExecuteNonQuery(string cmdText, SqlConnection sqlConnection) {
            return ExecuteNonQuery(cmdText, sqlConnection, new List<SqlParameter>());
        }

        public int ExecuteNonQuery(string cmdText, SqlConnection sqlConnection, IList<SqlParameter> parameters) {
            SqlCommand sqlCommand = new SqlCommand(cmdText, sqlConnection);
            sqlCommand.Parameters.AddRange(parameters.ToArray());
            return sqlCommand.ExecuteNonQuery();
        }
        public int ExecuteNonQuery(string query, IList<SqlParameter> parameters) {
            int count = 0;
            using (SqlConnection sqlConnection = NewSqlConnection()) {
                sqlConnection.Open();
                count = ExecuteNonQuery(query, sqlConnection, parameters);
                sqlConnection.Close();
            }
            return count;
        }

        public int ExecuteNonQuery(string cmdText) {
            return ExecuteNonQuery(cmdText, new List<SqlParameter>());
        }

        public Object[] ExecuteFirstRow(string cmdText, SqlConnection sqlConnection) {
            Object[] results = null;
            SqlCommand sqlCommand = new SqlCommand(cmdText, sqlConnection);
            SqlDataReader reader = sqlCommand.ExecuteReader();
            if (reader.Read()) {
                results = new Object[reader.FieldCount];
                for (int i = 0; i < results.Length; i++) {
                    results[i] = reader[i];
                }
            }
            reader.Close();
            return results;
        }

        public Object ExecuteScalar(string cmdText, SqlConnection sqlConnection) {
            SqlCommand sqlCommand = new SqlCommand(cmdText, sqlConnection);
            return sqlCommand.ExecuteScalar();
        }

        public Object ExecuteScalar(string cmdText) {
            Object result = null;
            using (SqlConnection sqlConnection = NewSqlConnection()) {
                sqlConnection.Open();
                result = ExecuteScalar(cmdText, sqlConnection);
                sqlConnection.Close();
            }
            return result;
        }

        public Object[] ExecuteFirstRow(string cmdText) {
            Object[] results = null;
            using (SqlConnection sqlConnection = NewSqlConnection()) {
                sqlConnection.Open();
                results = ExecuteFirstRow(cmdText, sqlConnection);
                sqlConnection.Close();
            }
            return results;
        }

        public void DropAllTables() {
            using (SqlConnection sqlConnection = NewSqlConnection()) {
                while (true) {
                    sqlConnection.Open();
                    SqlCommand cmd = new SqlCommand("select [name] from sys.tables", sqlConnection);
                    var reader = cmd.ExecuteReader();
                    var stringbuilder = new StringBuilder();

                    bool hasRows = false;
                    while (reader.Read()) {
                        hasRows = true;
                        stringbuilder.Append("[" + reader[0] as string + "],");
                    }
                    reader.Close();

                    if (!hasRows) break;

                    string tables = stringbuilder.ToString().Substring(0, stringbuilder.Length - 1);

                    try {
                        ExecuteNonQuery(string.Format("drop table " + tables), sqlConnection);
                    } catch {
                    }

                    sqlConnection.Close();
                }
            }
        }
    }
}
