using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BWR.ShareKernel.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        T First();
        T FirstOrDefualt();
        IEnumerable<T> GetAll();
        IQueryable<T> GetIQueryable();
        IEnumerable<T> GetAll(params Expression<Func<T, object>>[] propertySelectors);
        T GetById(object id);
        void Insert(T obj);
        void Update(T obj);
        void Delete(T obj);
        void RefershEntity(T obj);
        object Save();
        IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate);
        IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] propertySelectors);
        IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate, params string[] propertySelectors);
    }
}
