using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QCMonitor.Framework.Domain;
using QCMonitor.Framework.DBService.Hibernate;

namespace QCMonitor.Crontab.Domain {
    [StoreInDB]
    public class DBCrontabEntry : Entity {

        public DBCrontabEntry(String key) {
            Key = key;
            LastRunTime = new DateTime(1970, 1, 1);
        }

        public String Key { get; set; }
        public DateTime LastRunTime { get; set; }

        // for hibernate usage
        protected DBCrontabEntry() {
        }
    }
}
