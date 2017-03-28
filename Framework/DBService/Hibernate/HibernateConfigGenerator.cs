using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Collections;
using System.Diagnostics;
using QCMonitor.Framework.Domain;

namespace QCMonitor.Framework.DBService.Hibernate {
    /// <summary>
    /// Configuration Xml
    /// </summary>
    public class ConfigXml {
        const bool HasBracket = true;

        private string Bracket(string name) {
            if (name.StartsWith("[") && name.EndsWith("]")) {
                return name;
            }
            return HasBracket ? "[" + name + "]" : name;
        }

        public XElement CreateManyToOne(string name, string class_name, string cascade) {
            XElement xelement = new XElement("many-to-one",
                new XAttribute("name", name),
                new XAttribute("class", class_name),
                new XAttribute("column", Bracket(name)),
                new XAttribute("cascade", cascade));
            return xelement;
        }

        public XElement CreateID(string name) {
            var xelement = new XElement("id",
                new XAttribute("name", name),
                new XAttribute("column", Bracket(name)));
            return xelement;
        }

        public XElement CreateJoinedClass(string name, string table, bool lazy, bool isJoinedClass, string fatherTypeName) {
            var xelement = new XElement(isJoinedClass ? "joined-subclass" : "union-subclass", new XAttribute("name", name),
                new XAttribute("lazy", lazy.ToString().ToLower()),
                new XAttribute("table", Bracket(table)),
                new XAttribute("extends", fatherTypeName)
                );
            //if (cacheUsage != null) {
            //    xelement.Add(new XElement("cache",
            //        new XAttribute("usage", cacheUsage)));
            //}
            return xelement;
        }

        public XElement CreateClass(string name, string table, bool lazy, string cacheUsage) {
            XElement xelement = new XElement("class", new XAttribute("name", name),
                new XAttribute("lazy", lazy.ToString().ToLower()),
                new XAttribute("table", Bracket(table)));
            if (cacheUsage != null) {
                xelement.Add(new XElement("cache",
                    new XAttribute("usage", cacheUsage)));
            }
            return xelement;
        }

        //public XElement CreateHibernateTitle(string name) {
        //    var xelement = new XElement("hibernate-mapping",
        //        //new XAttribute(XNamespace.Xmlns.ToString(), "urn:nhibernate-mapping-2.2"),
        //        //new XAttribute("xmlns", "urn:nhibernate-mapping-2.2"),
        //        new XAttribute("assembly", name));

        //    return xelement;
        //}

        public XElement CreateStringProperty(string name, string columnName, int length) {
            var xelement = new XElement("property",
                new XAttribute("name", name),
                new XAttribute("type", "string"),
                new XAttribute("length", length),
                new XElement("column",
                    new XAttribute("name", Bracket(columnName)),
                    new XAttribute("length", length)));
            return xelement;
        }

        public XElement CreateProperty(string name, string columnName) {
            var xelement = new XElement("property",
                new XAttribute("name", name),
                new XAttribute("column", Bracket(columnName)));
            return xelement;
        }

        //public XElement CreateOneToManyBag(string name, string key, string )
        //{

        //}

        public XElement CreateBag(string name, string key, string relatedClassName) {
            var xelement = new XElement("bag",
                new XAttribute("name", name),
                new XAttribute("table", Bracket(name)),
                new XAttribute("cascade", "none"),
                new XAttribute("lazy", "false"),
                new XElement("key",
                    new XAttribute("column", Bracket(key + "Id"))),
                new XElement("many-to-many",
                    new XAttribute("class", relatedClassName),
                    new XAttribute("column", Bracket(relatedClassName + "Id")))
            );

            return xelement;
        }
    }

    /// <summary>
    /// Auto genenrate the hibernate configuration files.
    /// </summary>
    public class HibernateConfigGenerator {
        private string ConnectStringRef;

        public HibernateConfigGenerator(string connectStringRef) {
            ConnectStringRef = connectStringRef;
        }

        /// <summary>
        /// Get all the direct son type of given type
        /// </summary>
        /// <param name="father"></param>
        /// <returns>son types</returns>
        protected IEnumerable<Type> GetAllSonNode(Type father) {
            Assembly ass = Assembly.GetExecutingAssembly();
            IEnumerable<Type> typelist = ass.GetTypes().Where<Type>((temptype) => (temptype.BaseType != null && temptype.BaseType.Name == father.Name));
            return typelist;
        }


        /// <summary>
        /// Check whether the given property should be store in DB. The property with SkipInDB attribute will not be stored in 
        /// DB. Default is stored in DB
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool PropertyIsStoreInDB(PropertyInfo property) {
            return !property.GetCustomAttributes(false).Any(t => t is SkipInDBAttribute);
        }

        public int GetStringLength(PropertyInfo property) {
            var list = property.GetCustomAttributes(false).Where(t => t is StringTypeAttribute).ToList();
            if (list.Count == 0) return 255;
            else return (list[0] as StringTypeAttribute).Length;
        }

        public string GetTableName(Type type) {
            var list = type.GetCustomAttributes(false).Where(t => t is TableNameAttribute).ToList();
            if (list.Count == 0) return null;
            else return (list[0] as TableNameAttribute).TableName;
        }

        public string GetFieldName(PropertyInfo property) {
            var list = property.GetCustomAttributes(false).Where(t => t is FieldNameAttribute).ToList();
            if (list.Count == 0) return null;
            else return (list[0] as FieldNameAttribute).FieldName;
        }

        /// <summary>
        /// Check whether the given class should be store in DB. The property with StoreInDB attribute will be stored in 
        /// DB. Default is not stored in DB
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool ClassIsStoreInDB(Type type) {
            return type.GetCustomAttributes(false).Any(t => t is StoreInDBAttribute);
        }


        private string CacheEnumToString(CacheUsage usage) {
            switch (usage) {
                case CacheUsage.READWRITE:
                    return "read-write";
                case CacheUsage.READONLY:
                    return "read-only";
                case CacheUsage.NONONSTRICTREADWRITE:
                    return "nononstrict-read-write";
                case CacheUsage.TRANSACTIONAL:
                    return "transactional";
                case CacheUsage.NONE:
                    return null;
                default:
                    throw new NotImplementedException();
            }
        }

        private bool IsIdProperty(PropertyInfo property) {
            return property.GetCustomAttributes(false).Any(t => t is IdProperty);
        }



        protected ConfigXml Cxml = new ConfigXml();

        /// <summary>
        /// Generate hibernate xml configuration file 
        /// </summary>
        /// <param name="roottype">the root type</param>
        /// <param name="xmldoc">xml document</param>
        /// <param name="ele">xml Element</param>
        private XElement Generate(Type roottype) {
            if (ClassIsStoreInDB(roottype)) {
                XElement classNode;
                var storeInDB = roottype.GetCustomAttributes(false).Single(t => t is StoreInDBAttribute) as StoreInDBAttribute;

                if (roottype.Equals(typeof(QCMonitor.Framework.Domain.Entity))) {
                    string tableName = GetTableName(roottype);
                    classNode = Cxml.CreateClass(roottype.FullName, tableName != null ? tableName : roottype.Name, false, CacheEnumToString(storeInDB.Usage));
                } else {
                    if (!storeInDB.IsInAllDB && storeInDB.ConnectStringRef != ConnectStringRef) return null;

                    if (!ClassIsStoreInDB(roottype.BaseType) &&
                        !roottype.BaseType.Equals(typeof(QCMonitor.Framework.Domain.Entity))) {
                        throw new NotImplementedException("StoreInDB type must be the son of Entity or be the son of son of Entity!");
                    }

                    string tableName = GetTableName(roottype);
                    classNode = Cxml.CreateJoinedClass(roottype.FullName, tableName != null ? tableName : roottype.Name, false, storeInDB.IsJoinedClass, roottype.BaseType.FullName);
                }

                var fatherProperties = new HashSet<string>(roottype.BaseType.GetProperties().Select(t => t.Name));

                PropertyInfo[] propertylist = roottype.GetProperties();

                var newPropertyList = propertylist.Where(t => !fatherProperties.Contains(t.Name)).ToList();

                if (newPropertyList.Any(t => IsIdProperty(t))) {
                    var idProperty = newPropertyList.Single(t => IsIdProperty(t));
                    if (idProperty != null) {
                        classNode.Add(Cxml.CreateID(idProperty.Name));
                    }
                }

                foreach (PropertyInfo property in newPropertyList) {
                    if (PropertyIsStoreInDB(property) && !IsIdProperty(property)) {

                        //if (property.PropertyType.IsGenericType) { // TODO 需要调试这段代码
                        //    Type[] types = property.PropertyType.GetGenericArguments();
                        //    if (types.Length != 1)
                        //        throw new NotImplementedException();
                        //    XElement propertyEle = Cxml.CreateBag(property.Name, roottype.Name, types[0].Name);
                        //    classNode.Add(propertyEle);
                        //} else 

                        if (property.PropertyType.IsEnum ||
                          !property.PropertyType.IsClass ||
                          property.PropertyType.Name == "String") {
                            string fieldName = GetFieldName(property);

                            XElement propertyEle;
                            if (property.PropertyType.Equals(typeof(string)))
                                propertyEle = Cxml.CreateStringProperty(property.Name, fieldName != null ? fieldName : property.Name, GetStringLength(property));
                            else propertyEle = Cxml.CreateProperty(property.Name, fieldName != null ? fieldName : property.Name);

                            classNode.Add(propertyEle);
                        } else {

                            XElement propertyEle = Cxml.CreateManyToOne(property.Name, property.PropertyType.FullName, "none");
                            classNode.Add(propertyEle);
                        }
                    }
                }
                return classNode;
            } else {
                return null;
            }
        }


        public string Generate(Assembly assembly) {
            StringBuilder xmlConfig = new StringBuilder();
            string firstLine = @"<?xml version=""1.0"" encoding=""utf-8""?>";
            xmlConfig.AppendLine(firstLine);

            string title = string.Format(@"<hibernate-mapping xmlns=""urn:nhibernate-mapping-2.2"" assembly=""{0}"">",
                assembly.GetName().Name);

            xmlConfig.AppendLine(title);

            foreach (var type in assembly.GetTypes()) {
                var ele = Generate(type);
                if (ele != null) xmlConfig.AppendLine(ele.ToString());
            }

            xmlConfig.AppendLine("</hibernate-mapping>");

            return xmlConfig.ToString();
        }
    }
}
