using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QCMonitor.Framework.Util;

namespace QCMonitor.Framework.DBService.Hibernate
{
    /// <summary>
    /// The property tagged is id
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = false)]
    public class IdProperty : Attribute { }

    /// <summary>
    /// Class Attribute. The tagged class will be stored in database
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, Inherited = false)]
    public class StoreInDBAttribute : Attribute {
        public bool IsJoinedClass;
        public CacheUsage Usage;
        public string ConnectStringRef;
        public bool IsInAllDB;        //判断这个类是否能被存储在所有的数据库应用中

        public StoreInDBAttribute(bool isJoinedClass, CacheUsage cacheUsage, string dbSwitch, bool isInAllDB) {
            this.IsJoinedClass = isJoinedClass;
            this.Usage = cacheUsage;
            this.ConnectStringRef = dbSwitch;
            this.IsInAllDB = isInAllDB;
        }

        public StoreInDBAttribute() 
            : this(false, CacheUsage.NONE, DBSwitch.DefaultDBSwitch.ConnectStringRef, false) {
        }


        public StoreInDBAttribute(string dbSwitch)
            : this(false, CacheUsage.NONE, dbSwitch, false) {
        }

        public StoreInDBAttribute(bool isInAllDB)
            : this(false, CacheUsage.NONE, DBSwitch.DefaultDBSwitch.ConnectStringRef, isInAllDB) {
        }

        public StoreInDBAttribute(bool isJoinedClass, CacheUsage cacheUsage, bool isInAllDB)
            : this(isJoinedClass, cacheUsage, DBSwitch.DefaultDBSwitch.ConnectStringRef, isInAllDB) {
        }
    }

    public enum CacheUsage {
        READONLY,
        READWRITE,
        NONONSTRICTREADWRITE,
        TRANSACTIONAL,
        NONE
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, Inherited = false)]
    public class TableNameAttribute : Attribute {
        public string TableName { set; get; }

        public TableNameAttribute(string tableName) {
            this.TableName = tableName;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = false)]
    public class FieldNameAttribute : Attribute {
        public string FieldName { set; get; }

        public FieldNameAttribute(string fieldName) {
            this.FieldName = fieldName;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = false)]
    public class StringTypeAttribute : Attribute {
        public int Length { set; get; }

        public StringTypeAttribute(int len) {
            this.Length = len;
        }
    }

    /// <summary>
    /// Property attribute. The tagged property will be stored in database
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = false)]
    public class SkipInDBAttribute : Attribute { }
}
