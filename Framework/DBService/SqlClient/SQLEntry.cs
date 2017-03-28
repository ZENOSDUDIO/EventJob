using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCMonitor.Framework.Framework.DBService.SqlClient {
    public interface ISQLEntry {
        String ToSQLString();
    }

    public abstract class SQLEntry {
        public abstract String ToSQLString();
        public override String ToString() {
            return base.ToString() + " SQL: " + ToSQLString();
        }
    }

    public class TextEntry : SQLEntry {
        protected String Text;
        protected TextEntry(String text) {
            Text = text;
        }

        public override string ToSQLString() {
            return Text;
        }
    }

    public class ColumnDefinition : SQLEntry {
        public String ColumnName;
        public String ColumnType;

        public ColumnDefinition PreviousColumn;

        public ColumnDefinition(String name, String type, ColumnDefinition previousColumn) {
            ColumnName = name;
            ColumnType = type;
            PreviousColumn = previousColumn;
        }

        public ColumnDefinition(String name, String type) 
            : this(name, type, null){
        }

        public ColumnDefinition(ColumnDefinition definition) 
            : this(definition.ColumnName, definition.ColumnType, definition.PreviousColumn) {
        }

        public ColumnDefinition AddDefinition(ColumnDefinition nextDefinition) {
            var newDefinition = new ColumnDefinition(nextDefinition);
            newDefinition.PreviousColumn = this;
            return newDefinition;
        }

        public override string ToSQLString() {
            var columnString = "[" + ColumnName + "] " + ColumnType;
            if (PreviousColumn == null)
                return columnString;
            return PreviousColumn.ToSQLString() + " , " + columnString;
        }

        public IList<ColumnDefinition> ToList() {
            if (PreviousColumn == null) {
                IList<ColumnDefinition> list = new List<ColumnDefinition>();
                list.Add(this);
                return list;
            }
            IList<ColumnDefinition> olist = PreviousColumn.ToList();
            olist.Add(this);
            return olist;
        }
    }
}
