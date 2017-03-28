using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QCMonitor.Framework.Domain;
using NHibernate.Cfg;
using NHibernate;
using NHibernate.Criterion;
using QCMonitor.Framework.DBService.Hibernate;
using QCMonitor.Framework.DBService;

namespace QCMonitor.Framework.Repository {
    public class RepositoryImpl<T> : IRepository<T> where T : Entity {

        public RepositoryImpl() {
            SessionFactory = QCDBServiceFactory.Instance.GetNhibernateSessionFactory<T>();
        }

        protected NhibernateSessionFactory SessionFactory;

        public IList<T> GetByCriteria(DetachedCriteria aCriteria) {
            using (var session = SessionFactory.OpenSession()) {
                ICriteria criteria = aCriteria.GetExecutableCriteria(session.SessionImpl);
                criteria.Add(Expression.Eq("IsDelete", false));
                IList<T> rs = criteria.List<T>();
                return rs;
            }
        }

        public T GetUniqueByCriteria(DetachedCriteria aCriteria) {
            using (var session = SessionFactory.OpenSession()) {
                ICriteria criteria = aCriteria.GetExecutableCriteria(session.SessionImpl);
                criteria.Add(Expression.Eq("IsDelete", false));
                return criteria.UniqueResult<T>();
            }
        }

        [Obsolete("Need to consider the IsDelete Property and TableName")]
        public IList<T> ExecuteSql(string sql) {
            using (var session = SessionFactory.OpenSession()) {
                return session.ExecuteSql<T>(sql);
            }
        }

        [Obsolete("Need to consider the IsDelete Property and TableName")]
        public int ExecuteSqlCount(string sql) {
            using (var session = SessionFactory.OpenSession()) {
                return session.ExecuteScalarSql<int>(sql);
            }
        }

        [Obsolete("Need to consider the IsDelete Property")]
        public IList<T> ExecuteHql(string hql) {
            using (var session = SessionFactory.OpenSession()) {
                return session.ExecuteHql<T>(hql);
            }
        }

        [Obsolete("Need to consider the IsDelete Property")]
        public Int64 ExecuteCount(string hql) {
            using (var session = SessionFactory.OpenSession()) {
                return session.ExecuteScalarHql<Int64>(hql);
            }
        }

        [Obsolete("Need to consider the IsDelete Property")]
        public IList<T> ExecuteHql(string hql, int startNum, int maxNum) {
            using (var session = SessionFactory.OpenSession()) {
                return session.ExecuteHql<T>(hql, startNum, maxNum);
            }
        }

        //public IList<T> ExecuteSql(string sql)
        //{
        //    using (var session = SessionFactory.OpenSession())
        //    {
        //        return session.ExecuteSql<T>(sql);
        //    }
        //}

        public IList<T> Load(IList<String> ids) {
            IList<T> list = new List<T>();
            using (var session = SessionFactory.OpenSession()) {
                foreach (var id in ids) {
                    T t = session.Load<T>(id);
                    list.Add(t);
                }

                return list;
            }
        }

        public T Load(string id) {
            using (var session = SessionFactory.OpenSession()) {
                T t = session.Load<T>(id);
                return t;
            }
        }

        public T Get(string id) {
            return Load(id);
        }

        public IList<T> GetAll() {
            using (var session = SessionFactory.OpenSession()) {
                ICriteria targetObjects = session.CreateCriteria<T>();
                IList<T> itemList = (IList<T>)targetObjects.List<T>();
                return itemList;
            }
        }

        public void SaveOrUpdate(T entity) {
            using (var session = SessionFactory.OpenSession()) {
                var transaction = session.BeginTransaction();
                session.SaveOrUpdate(entity);
                transaction.Commit();
            }
        }

        public void SaveOrUpdateRange(IEnumerable<T> entities) {
            foreach (T t in entities) {
                SaveOrUpdate(t);
            }
        }

        public void Update(T entity) {
            using (var session = SessionFactory.OpenSession()) {
                var transaction = session.BeginTransaction();
                session.Update(entity);
                transaction.Commit();
            }
        }

        public void Delete(string id) {
            using (var session = SessionFactory.OpenSession()) {
                var transaction = session.BeginTransaction();
                session.Delete<T>(id);
                transaction.Commit();
            }
        }

        public void Delete(T entity) {
            using (var session = SessionFactory.OpenSession()) {
                var transaction = session.BeginTransaction();
                session.Delete<T>(entity);
                transaction.Commit();
            }
        }

        public int DeleteAll() {
            int count = -1;
            using (var session = SessionFactory.OpenSession()) {
                var transaction = session.BeginTransaction();
                count = session.DeleteAll<T>();
                transaction.Commit();
            }
            return count;
        }

        public void DeleteList(IList<T> itemsToDelete) {
            using (var session = SessionFactory.OpenSession()) {
                using (var transaction = session.BeginTransaction()) {
                    foreach (T item in itemsToDelete) {
                        session.Delete<T>(item);    
                    }
                    transaction.Commit();
                }
            }
        }
        public int PhysicsDeleteAll() {
            int count = -1;
            using (var session = SessionFactory.OpenSession()) {
                var transaction = session.BeginTransaction();
                count = session.PhysicsDeleteAll<T>();
                transaction.Commit();
            }
            return count;
        }

        public void PhysicsDelete(string id) {
            using (var session = SessionFactory.OpenSession()) {
                var transaction = session.BeginTransaction();
                session.PhysicsDelete(id);
                transaction.Commit();
            }
        }

        public int GetSize() {
            using (var session = SessionFactory.OpenSession()) {
                String hql = "select count(*) from " + typeof(T);
                return session.GetCountValueBySql(hql);
            }
        }

        [Obsolete("Just a estimate of the count, not a accurate number")]
        public int GetCount() {
            return GetCountByCriteria(DetachedCriteria.For(typeof(T)));
        }

        [Obsolete("Just a estimate of the count, not a accurate number")]
        public int GetCountByCriteria(DetachedCriteria aCriteria) {
            using (var session = SessionFactory.OpenSession()) {
                var count = aCriteria.GetExecutableCriteria(session.SessionImpl)
                                   .SetProjection(Projections.Count(Projections.Id()))
                                   .SetCacheable(true)
                                   .UniqueResult<int>();
                return count;
            }
        }
    }
}
