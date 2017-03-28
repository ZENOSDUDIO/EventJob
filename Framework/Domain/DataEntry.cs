using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QCMonitor.Framework.DBService.Hibernate;

namespace QCMonitor.Framework.Domain {
    [StoreInDB]
    public class DataEntry : Entity {
        public bool Enabled { set; get; }

        public DataEntry() {
            Enabled = true;
        }
    }
}
