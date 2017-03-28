using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QCMonitor.Framework.Domain;
using System.Configuration;
using QCMonitor.Framework.DBService.Hibernate;
using QCMonitor.Framework.DBService.SqlClient;
using System.Reflection;
using QCMonitor.Framework.Framework.DBService.SqlClient;

namespace QCMonitor.Framework.DBService {

    [Serializable]
    public class QCDBException : Exception {
        public QCDBException(string message) : base(message) {
        }
    }

    public class DBSwitch {
        public DBSwitch(string connectStringRef) {
            this.ConnectStringRef = connectStringRef;
        }

        public string ConnectStringRef { set; get; }

        public string ConnectionString {
            get {
                var connString = ConfigurationManager.ConnectionStrings[ConnectStringRef];
                if (connString == null) throw new NotImplementedException(ConnectStringRef + " sql connection string is not exist!");
                return connString.ConnectionString;
            }
        }

        public static readonly DBSwitch DefaultDBSwitch = new DBSwitch("MainService");
    }


    public class QCDBServiceFactory {

        private QCDBClient DefaultDBClientValue = null;

        public QCDBClient DefaultDBClient {
            get {
                if (DefaultDBClientValue == null) {
                    DefaultDBClientValue = new QCDBClient(DBSwitch.DefaultDBSwitch.ConnectionString);
                }
                return DefaultDBClientValue;
            }
        }

        public static QCDBServiceFactory Instance = new QCDBServiceFactory();

        /* ======================华丽分隔线====================== */

        private Dictionary<String, NhibernateSessionFactory> sessionFactoryMap = null;

        private Dictionary<String, NhibernateSessionFactory> SessionFactoryMap {
            get {
                if (sessionFactoryMap == null || sessionFactoryMap.Count == 0) {
                    throw new QCDBException("Please init the hibernate first!");
                } else return sessionFactoryMap;
            }
        }

        public bool IsStoreInDB(Type type) {
            return type.GetCustomAttributes(false).Any(t => t is StoreInDBAttribute);
        }


        private IList<DBSwitch> GetAllDBSwitch(Assembly[] assemblyArray) {
            HashSet<string> hashSet = new HashSet<string>();

            foreach (var assembly in assemblyArray) {
                foreach (var type in assembly.GetTypes()) {
                    if (IsStoreInDB(type)) {
                        var storeInDB = type.GetCustomAttributes(false).Single(t => t is StoreInDBAttribute) as StoreInDBAttribute;
                        hashSet.Add(storeInDB.ConnectStringRef);
                    }
                }
            }
            return hashSet.Select(t => new DBSwitch(t)).ToList();
        }

        public void InitSqlClient(Assembly[] assemblyArray) {
            foreach (var assembly in assemblyArray) {
                foreach (var type in assembly.GetTypes()) {
                    if (type.GetCustomAttributes(false).Any(t => t is InitTableAttribute)) {
                        Activator.CreateInstance(type);
                    }
                }
            }
        }

        public void InitHibernate(Assembly[] assemblyArray) {
            if (sessionFactoryMap == null || sessionFactoryMap.Count == 0) {
                sessionFactoryMap = new Dictionary<string, NhibernateSessionFactory>();
                foreach (var node in GetAllDBSwitch(assemblyArray)) {
                    sessionFactoryMap.Add(node.ConnectStringRef, new NhibernateSessionFactory(assemblyArray, node));
                }
            }
        }

        private QCDBServiceFactory() {
        }

        public NhibernateSessionFactory GetNhibernateSessionFactory<T>() where T : Entity {
            var storeInDB = typeof(T).GetCustomAttributes(false).Single(t => t is StoreInDBAttribute) as StoreInDBAttribute;
            if (SessionFactoryMap.ContainsKey(storeInDB.ConnectStringRef)) {
                return SessionFactoryMap[storeInDB.ConnectStringRef];
            } else {
                throw new NotImplementedException("The DBSwitch (" + storeInDB.ConnectStringRef.ToString() + ") is not exist!");
            }
        }
    }
}
