using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Cfg;
using System.Reflection;
using NHibernate;
using System.Xml;
using System.IO;
using QCMonitor.Framework.Util;
using QCMonitor.Framework.Domain;

namespace QCMonitor.Framework.DBService.Hibernate {
    public class NhibernateSessionFactory {
        public NhibernateSessionAdapter OpenSession() {
            ISession session = SessionFactory.OpenSession();
            var sessionAdapter = new NhibernateSessionAdapter(session);
            return sessionAdapter;
        }

        public ISessionFactory GetSessionFactory() {
            return SessionFactory;
        }

        protected ISessionFactory SessionFactory = null;

        internal NhibernateSessionFactory(Assembly[] assemblyArray, DBSwitch dbswitch) {
            if (dbswitch.ConnectionString == null) throw new NotImplementedException("Hibernate Connection String is NULL");
            
            Configuration cfg = new Configuration();
            cfg.Configure();

            cfg.SetProperty("connection.connection_string", dbswitch.ConnectionString);

            HibernateConfigGenerator gen = new HibernateConfigGenerator(dbswitch.ConnectStringRef);

            var assemblyList = new List<Assembly>();
            assemblyList.Add(typeof(Entity).Assembly);
            assemblyList.AddRange(assemblyArray);

            foreach (Assembly assembly in assemblyList) {
                var xmlConfig = gen.Generate(assembly);
                string hibernateXmlFileDir = "";
                string path = @"D:\";
                if (Directory.Exists(path)) {
                    hibernateXmlFileDir = QCFileLogger.LogRootDirFirst + @"\Log_HibernateConfigFiles";

                } else {
                    hibernateXmlFileDir = QCFileLogger.LogRootDir + @"\Log_HibernateConfigFiles";

                }

                //string hibernateXmlFileDir = QCFileLogger.LogRootDir + @"\Log_HibernateConfigFiles";
                Directory.CreateDirectory(hibernateXmlFileDir);
                string configFilePath = hibernateXmlFileDir + @"\" + dbswitch.ConnectStringRef + "_" + assembly.GetName().Name
                    + "_" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "_").Replace(" ", "_") + ".hbm.xml";
                StreamWriter writer = new StreamWriter(configFilePath);
                writer.Write(xmlConfig);
                writer.Close();

                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(xmlConfig);

                cfg.AddDocument(xdoc);
            }


            SessionFactory = cfg.BuildSessionFactory();
        }
    }
}
