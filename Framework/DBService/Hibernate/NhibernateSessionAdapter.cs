using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using QCMonitor.Framework.Domain;
using NHibernate.Criterion;

namespace QCMonitor.Framework.DBService.Hibernate {
    public class NhibernateSessionAdapter : IDisposable {
        public void Dispose() {
            session.Dispose();
        }

        protected ISession session;

        public ISession SessionImpl {
            get { return session; }
        }

        public NhibernateSessionAdapter(ISession session) {
            this.session = session;
        }

        public ITransaction BeginTransaction() {
            return session.BeginTransaction();
        }

        public T Load<T>(object id) where T : Entity {
            T t = null;
            try {
                t = session.Load<T>(id);
            } catch (ObjectNotFoundException) {
                t = null;
            }
            if (t == null) return t;
            if (t.IsDelete) return null;
            else return t;
        }

        public ICriteria CreateCriteria<T>() where T : Entity {
            var criteria = session.CreateCriteria(typeof(T));
            criteria.Add(Expression.Eq("IsDelete", false));
            return criteria;
        }

        [Obsolete("Need to consider the IsDelete Property")]
        public IList<T> ExecuteHql<T>(string hql) {
            var query = session.CreateQuery(hql);
            return query.List<T>();
        }

        [Obsolete("Need to consider the IsDelete Property")]
        public T ExecuteScalarHql<T>(string hql) {
            var query = session.CreateQuery(hql);
            return query.UniqueResult<T>();
        }

        [Obsolete("Need to consider the IsDelete Property")]
        public IList<T> ExecuteHql<T>(string hql, int startRowNum, int maxRowNum) {
            var query = session.CreateQuery(hql);
            query.SetFirstResult(startRowNum);
            query.SetMaxResults(maxRowNum);

            return query.List<T>();
        }

        [Obsolete("Need to consider the IsDelete Property and TableName")]
        public IList<T> ExecuteSql<T>(string sql) {
            var query = session.CreateSQLQuery(sql).AddEntity(typeof(T));
            return query.List<T>();
        }

        [Obsolete("Need to consider the IsDelete Property and TableName")]
        public T ExecuteScalarSql<T>(string sql) {
            var query = session.CreateSQLQuery(sql).AddEntity(typeof(T));
            return query.UniqueResult<T>();
        }

        public void PhysicsDelete(object id) {
            session.Delete(id);
        }

        public void Update<T>(T obj) where T : Entity {
            session.Update(obj);
        }

        public void SaveOrUpdate<T>(T obj) where T : Entity {
            session.SaveOrUpdate(obj);
        }

        public int Delete<T>(object id) where T : Entity {
            T t = session.Load<T>(id);
            if (t == null) return -1;
            if (t.IsDelete) return -2;
            t.IsDelete = true;
            session.Update(t);
            return 0;
        }

        public int PhysicsDeleteAll<T>() where T : Entity {

            IList<T> ls = session.CreateCriteria(typeof(T)).List<T>();
            foreach (T t in ls) {
                session.Delete(t);
            }
            return 0;
        }

        public int DeleteAll<T>() where T : Entity {
            IList<T> ls = session.CreateCriteria(typeof(T)).List<T>();
            foreach (T t in ls) {
                t.IsDelete = true;
                session.Update(t);
            }
            return 0;
        }

        public int GetCountValueBySql(String hql) {
            var query = session.CreateQuery(hql.Trim());
            return Convert.ToInt32(query.UniqueResult());
        }
    }
}
