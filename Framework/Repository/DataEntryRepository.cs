using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QCMonitor.Framework.Repository;
using QCMonitor.Framework.Domain;
using NHibernate;
using NHibernate.Criterion;

namespace QCMonitor.Framework.Framework.Repository {
    public class DataEntryRepository<T> : RepositoryImpl<T> where T : DataEntry {
        public IList<T> GetAllEnabled() {
            using (var session = SessionFactory.OpenSession()) {
                ICriteria targetObjects = session.CreateCriteria<T>();
                targetObjects.Add(Expression.Eq("Enabled", true));
                IList<T> itemList = (IList<T>)targetObjects.List<T>();
                return itemList;
            }
        }

        public IList<T> GetByCriteriaEnabled(DetachedCriteria aCriteria) {
            aCriteria.Add(Expression.Eq("Enabled", true));
            return GetByCriteria(aCriteria);
        }

        public T GetUniqueByCriteriaEnabled(DetachedCriteria aCriteria) {
            aCriteria.Add(Expression.Eq("Enabled", true));
            return GetUniqueByCriteria(aCriteria);
        }
    }
}
