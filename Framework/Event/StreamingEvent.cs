using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using QCMonitor.Framework.DBService.SqlClient;
using QCMonitor.Framework.Framework.DBService.SqlClient;

namespace QCMonitor.Framework.Event
{
    //
    [Serializable]
    public class StreamingEvent : QCEvent
    {
        public string CurrentRowId { get; set; }

        public override string StatusString()
        {
            return base.StatusString()
                + "\n" + "EventRowId" + CurrentRowId;
        }
    }

    [InitTable]
    public class StreamingEventOptions : QCOptions
    {
        public StreamingEventOptions(String ns) : base(ns) { }
        public Int64 CurrentRow
        {
            get
            {
                return Int64.Parse(GetString("CurrentRow", "-1"));
            }
            set
            {
                UpdateString("CurrentRow", "" + value);
            }
        }

        internal void IncrCurrentRow()
        {
            QuickIncrementInt("CurrentRow");
        }
    }

    public abstract class StreamingEventSource<T> : QCEventSource<T>, ITableInit where T : StreamingEvent
    {
        public abstract String TableName { get; }

        public virtual ColumnDefinition ValueDefs
        {
            get
            {
                return new ColumnDefinition("row_id", "int identity(1,1) PRIMARY KEY");
            }
        }
        protected string FetchNextCursorCmd;
        protected string FetchPriorCursorCmd;
        protected string FetchScrollCursorCmd;

        public String CursorName
        {
            get
            {
                return "EventsCursor_" + TableName.ToString();
            }
        }

        protected StreamingEventOptions Options;
        protected SqlConnection SqlConnection;
        protected QCDBClient DB; // initialized in constructor
        protected String[] ValueNames;
        protected IDictionary<String, Object> CurrentRowDictionary;

        protected abstract StreamingEventOptions InitStreamingOptions();

        protected abstract QCDBClient InitDB();

        public void InitTable()
        {
            DB = InitDB();
            DB.CreateTable(TableName, ValueDefs);
        }

        public StreamingEventSource()
        {
            DB = InitDB();
            Options = InitStreamingOptions();

            DB.CreateTable(TableName, ValueDefs);
            SqlConnection = DB.NewSqlConnection();
            SqlConnection.Open();

            // init the cursor and options
            String dropCursorCmd = String.Format(@"DEALLOCATE  {0};", CursorName);
            String createCursorCmd = String.Format(@"DECLARE {0}  CURSOR Dynamic Read_Only
                    FOR SELECT * FROM {1} where row_id>={2}
              OPEN {0};", CursorName, TableName, Options.CurrentRow);

            DB.ExecuteNonQuery(createCursorCmd, SqlConnection);

            FetchNextCursorCmd = String.Format(@"FETCH NEXT FROM {0};", CursorName);
            FetchPriorCursorCmd = String.Format(@"FETCH PRIOR FROM {0};", CursorName);
            FetchScrollCursorCmd = String.Format(@"FETCH ABSOLUTE {0} FROM {1};", Options.CurrentRow, CursorName);

            ValueNames = ValueDefs.ToList()
                .Select(def => def.ColumnName).ToArray();


           

            // skip to the unhappend event
            //while (true)
            //{
                Object[] rs = NextRow();

                if (rs != null)
                {

                    if (IsValidRow(rs))
                    {
                         //back one
                        PriroRow();
                        //break;
                    }
                    //else continue; // visited rows
                }
            //    else break;
            //}
        }

        /// <summary>
        /// Could filter the row which u think is not valid
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        protected virtual Boolean IsValidRow(Object[] rs)
        {
            Int64 rowId = Int64.Parse(rs[0].ToString());
            return rowId >= Options.CurrentRow;
        }

        protected Object[] NextRow()
        {
            return DB.ExecuteFirstRow(FetchNextCursorCmd, SqlConnection);
        }

        protected Object[] PriroRow()
        {
            return DB.ExecuteFirstRow(FetchPriorCursorCmd, SqlConnection);
        }

        protected Object[] LatestRow()
        {
            Object[] rs = NextRow();
            if (rs == null)
            {
                rs = PriroRow();
            }
            return rs;
        }

        protected Object[] ScrollRow()
        {
            return DB.ExecuteFirstRow(FetchScrollCursorCmd, SqlConnection);
        }

        protected Object[] current_row;
        protected Object[] CurrentRow
        {
            get
            {
                return current_row;
            }
            set
            {
                current_row = value;
                for (int i = 0; i < ValueNames.Length; i++)
                {
                    CurrentRowDictionary = new Dictionary<String, Object>();
                    CurrentRowDictionary[ValueNames[i]] = current_row[i];
                }
            }
        }

        public override bool HasNextEvent()
        {
            Object[] rs = LatestRow();
            if (rs != null)
            {
                if (IsValidRow(rs))
                {
                    Int64 rowId = Int64.Parse(rs[0].ToString());
                    CurrentRow = rs;
                    Options.CurrentRow = rowId + 1;
                    return true;
                }
            }
            return false;
        }

        protected sealed override T DecorateEvent(T newEvent)
        {
            newEvent.CurrentRowId = CurrentRow[0].ToString();
            return DecorateStreamingEvent(newEvent);
        }

        protected virtual T DecorateStreamingEvent(T streamingEvent)
        {
            return streamingEvent;
        }

        public void ClearEvents()
        {
            DB.DeleteFromTable(TableName);
        }

        public void DropTable()
        {
            DB.DropTable(TableName);
        }

        public override string StatusString()
        {
            return base.StatusString()
                + "\n" + "CurrentRowId : " + CurrentRow
                //+ "\n" + "CurrentRow : " + CurrentRow.Aggregate("", 
                //                                            (sum, obj) => sum + "\n " + obj.ToString())
                                                            ;
        }
    }
}
