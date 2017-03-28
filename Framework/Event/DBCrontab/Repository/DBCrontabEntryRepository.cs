using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QCMonitor.Framework.Repository;
using QCMonitor.Crontab.Domain;
using NHibernate.Criterion;

namespace QCMonitor.Crontab.Repository {
    public class DBCrontabEntryRepository : RepositoryImpl<DBCrontabEntry> {
        public DBCrontabEntry LoadCrontabByKey(String key) {
            var criteria = DetachedCriteria.For<DBCrontabEntry>();
            criteria.Add(Expression.Eq("Key", key));
            return GetUniqueByCriteria(criteria);
        }
    }
}
