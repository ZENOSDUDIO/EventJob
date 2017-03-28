using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QCMonitor.Framework.DBService.Hibernate;

namespace QCMonitor.Framework.Domain {
    /// <summary>
    /// The base class for database entity
    /// </summary>
    [StoreInDB(false, CacheUsage.READWRITE, true)]
    [Serializable]
    public abstract class Entity {
        public Entity() {
            Id = Guid.NewGuid().ToString();
            CreateTime = DateTime.Now;
            IsDelete = false;
        }

        [IdProperty]
        public virtual string Id { get; protected set; }


        public virtual DateTime CreateTime { get; protected set; }
        public virtual bool IsDelete { get; set; }
        public virtual Int32 Version { get; protected set; }
    }
}
